using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using PreviewSymbol.Helpers;


namespace PreviewSymbol.UIX
{
	internal class PreviewSymbolDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "PreviewSymbol_UIX_PreviewSymbolDockpane";

		private TextEditor AvalonTextEditor = null;
		private FoldingManager _foldingManager;
		private XmlFoldingStrategy _xmlFolding;
		private CIMSymbol _selectedSymbol = null;
		private GraphicElement _selectedElement = null;

		private ImageSource _img;
		private string _msg = "Select a symbol";
		private string _xmlerror = "";

		protected PreviewSymbolDockpaneViewModel() { }

		protected override Task InitializeAsync()
		{
			SelectedSymbolChangedEvent.Subscribe((args) =>
			{
				if (!LockSymbolPreview || _selectedSymbol == null)
					_selectedSymbol = args.SelectedSymbol;
				_selectedElement = args.SelectedElement;
				string msg = "";
				if (args.SelectedOID >= 0)
				{
					msg = $"{args.SelectedLayerName}: {args.SelectedOID}";
				}
				if (!LockSymbolPreview)
				{
					SetXmlText(_selectedSymbol);
					RefreshSymbol(_selectedSymbol, msg, true);
				}
			});
			return Task.FromResult(0);
		}

		internal void SetAvalonTextEditor(TextEditor editor)
		{
			AvalonTextEditor = editor;
			_foldingManager = FoldingManager.Install(this.AvalonTextEditor.TextArea);
			_xmlFolding = new XmlFoldingStrategy();
		}

		public ImageSource SymbolImageSource => _img;

		public bool LockSymbolPreview
		{
			get
			{
				return Module1.Current.LockSymbolPreview;
			}
			set
			{
				if (Module1.Current.LockSymbolPreview != value)
				{
					Module1.Current.LockSymbolPreview = value;
					NotifyPropertyChanged("");
				}
			}
		}

		public string Message => _msg;

		#region Commands

		ICommand _refreshCommand;
		ICommand _selectCommand;
		ICommand _previewCommand;
		ICommand _applyCommand;
		ICommand _changeFontSizeCommand;
		ICommand _updateFoldings;

		public ICommand RefreshCommand
		{
			get
			{
				return _refreshCommand ?? (_refreshCommand = new RelayCommand(() => {
					if (!LockSymbolPreview)
						_selectedSymbol = Module1.Current.SelectedSymbol;
					_selectedElement = Module1.Current.SelectedElement;
					if (!LockSymbolPreview)
					{
						SetXmlText(_selectedSymbol);
						RefreshSymbol(_selectedSymbol, "");
					}
				}));
			}
		}

		public ICommand SelectCommand
		{
			get
			{
				return _selectCommand ?? (_selectCommand = new RelayCommand(() => {
					FrameworkApplication.SetCurrentToolAsync("PreviewSymbol_Ribbon_SelectFeatureTool");
				}));
			}
		}

		public ICommand ApplyCommand
		{
			get
			{
				return _applyCommand ?? (_applyCommand = new RelayCommand(() => {
					if (_selectedElement == null)
					{
						ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("No graphic element was selected", "Apply failed");
						return;
					}
					var xml = this.AvalonTextEditor.Text;
					if (ValidateXML(xml))
					{
						_selectedSymbol = GenerateSymbol(xml);
						RefreshSymbol(_selectedSymbol, "");
						QueuedTask.Run(() =>
						{
							//If user locked the preview then it is their
							//responsibility to ensure that the selected element
							//matches the current symbol type
							var graphic = _selectedElement.GetGraphic();
							try
							{
								graphic.Symbol.Symbol = _selectedSymbol;
								_selectedElement.SetGraphic(graphic);
							}
							catch(Exception ex)
							{
								//we are not on the UI thread here
								System.Diagnostics.Debug.WriteLine(ex.ToString());
							}
						});
					}
					else
					{
						var msg = $"Invalid xml\r\n{_xmlerror}";
						ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(msg, "Preview failed");
					}
				}, () => _selectedElement != null));
			}
		}

		public ICommand PreviewCommand
		{
			get
			{
				return _previewCommand ?? (_previewCommand = new RelayCommand(() => {
					var xml = this.AvalonTextEditor.Text;
					if (ValidateXML(xml))
					{
						_selectedSymbol = GenerateSymbol(xml);
						RefreshSymbol(_selectedSymbol, "");
					}
					else
					{
						var msg = $"Invalid xml\r\n{_xmlerror}";
						ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(msg, "Preview failed");
					}
				}));
			}
		}

		public ICommand ChangeFontSizeCommand
		{
			get
			{
				return _changeFontSizeCommand ?? (_changeFontSizeCommand = new RelayCommand(
								 (Action<object>)ChangeTextSize, (Func<bool>)(() => { return true; })));
			}
		}

		public ICommand UpdateFoldingsCommand
		{
			get
			{
				return _updateFoldings ?? (_updateFoldings = new RelayCommand(() =>
				{
					if (!string.IsNullOrEmpty(this.AvalonTextEditor.Text))
						this._xmlFolding.UpdateFoldings(this._foldingManager, this.AvalonTextEditor.Document);
				}));
			}
		}

		PreviewControlCommand _cutCommand;
		PreviewControlCommand _copyCommand;
		PreviewControlCommand _pasteCommand;
		PreviewControlCommand _undoCommand;
		PreviewControlCommand _redoCommand;

		public PreviewControlCommand CutCommand
		{
			get
			{
				return _cutCommand ?? (_cutCommand = new PreviewControlCommand(ApplicationCommands.Cut));
			}
		}

		public PreviewControlCommand CopyCommand
		{
			get
			{
				return _copyCommand ?? (_copyCommand = new PreviewControlCommand(ApplicationCommands.Copy));
			}
		}

		public PreviewControlCommand PasteCommand
		{
			get
			{
				return _pasteCommand ?? (_pasteCommand = new PreviewControlCommand(ApplicationCommands.Paste));
			}
		}

		public PreviewControlCommand UndoCommand
		{
			get
			{
				return _undoCommand ?? (_undoCommand = new PreviewControlCommand(ApplicationCommands.Undo));
			}
		}

		public PreviewControlCommand RedoCommand
		{
			get
			{
				return _redoCommand ?? (_redoCommand = new PreviewControlCommand(ApplicationCommands.Redo));
			}
		}

		#endregion Commands

		#region Private

		private void ChangeTextSize(object cmdParam)
		{
			string delta = cmdParam.ToString();

			this.AvalonTextEditor.FontSize = (delta == "-1"
				? this.AvalonTextEditor.FontSize - 1
				: this.AvalonTextEditor.FontSize + 1);
		}

		private void SetXmlText(CIMSymbol symbol)
		{
			var xml = symbol?.ToXml() ?? "";
			this.AvalonTextEditor.Text = FormatXml(xml);
			this._xmlFolding.UpdateFoldings(this._foldingManager, this.AvalonTextEditor.Document);
		}

		private static string FormatXml(string xml)
		{
			if (string.IsNullOrEmpty(xml))
				return "";
			var doc = new XmlDocument();
			var sb = new StringBuilder();
			try
			{
				doc.LoadXml(xml);
				var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
				doc.Save(XmlWriter.Create(sb, xmlWriterSettings));
			}
			catch (System.Xml.XmlException xmle)
			{
				System.Diagnostics.Debug.WriteLine("FormatXml Exception: {0}", xmle.ToString());
				sb.Append(xml);
			}
			return sb.ToString();
		}

		private bool ValidateXML(string xml)
		{
			if (string.IsNullOrEmpty(xml))
				return false;
			var doc = new XmlDocument();
			bool valid = false;
			try
			{
				doc.LoadXml(xml);//This will throw XmlException
				valid = true;
			}
			catch (XmlException xe)
			{
				_xmlerror = xe.ToString();
			}
			doc = null;
			return valid;
		}

		private CIMSymbol GenerateSymbol(string xml)
		{
			return Module1.Current.DeserializeXmlDefinition<CIMSymbol>(xml);
		}

		private void RefreshSymbol(CIMSymbol symbol, string msg, bool forceMsg = false)
		{
			_img = GenerateBitmapImageAsync(symbol).Result;//block
			if (!string.IsNullOrEmpty(msg) || forceMsg)
			{
				_msg = msg;
			}
			NotifyPropertyChanged("SymbolImageSource");
			NotifyPropertyChanged("Message");
		}

		private Task<ImageSource> GenerateBitmapImageAsync(CIMSymbol symbol)
		{
			return QueuedTask.Run(() => {
				if (symbol == null)
					return null;
				var si = new SymbolStyleItem()
				{
					Symbol = symbol,
					PatchHeight = 80,
					PatchWidth = 80
				};
				var bm = si.PreviewImage as BitmapSource;
				bm.Freeze();
				return (ImageSource)bm;
				//var bm = new TransformedBitmap((BitmapSource)si.PreviewImage, new ScaleTransform(2,2));
				//return (ImageSource)bm;
			});
		}

		#endregion Private

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "My DockPane";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class PreviewSymbolDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			PreviewSymbolDockpaneViewModel.Show();
		}
	}
}
