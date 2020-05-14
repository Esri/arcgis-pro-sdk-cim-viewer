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
using ArcGIS.Desktop.Mapping;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace PreviewSymbol.UIX
{
	internal class PreviewSymbolDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "PreviewSymbol_UIX_PreviewSymbolDockpane";

		private TextEditor AvalonTextEditor = null;
		private FoldingManager _foldingManager;
		private XmlFoldingStrategy _xmlFolding;
		private CIMSymbol _selectedSymbol = null;

		private ImageSource _img;
		private string _msg = "Select a symbol";
		private string _xmlerror = "";

		protected PreviewSymbolDockpaneViewModel() { }

		protected override Task InitializeAsync()
		{
			SelectedSymbolChangedEvent.Subscribe((args) =>
			{
				_selectedSymbol = args.SelectedSymbol;
				string msg = "";
				if (args.SelectedOID >= 0)
				{
					msg = $"{args.SelectedFeatureLayer}: {args.SelectedOID}";
				}
				SetXmlText(_selectedSymbol);
				RefreshSymbol(_selectedSymbol, msg, true);
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

		public string Message => _msg;

		#region Commands

		ICommand _refreshCommand;
		ICommand _selectCommand;
		ICommand _applyCommand;
		ICommand _changeFontSizeCommand;

		public ICommand RefreshCommand
		{
			get
			{
				return _refreshCommand ?? (_refreshCommand = new RelayCommand(() => {
					_selectedSymbol = Module1.Current.SelectedSymbol;
					SetXmlText(_selectedSymbol);
					RefreshSymbol(_selectedSymbol, "");
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

		PreviewControlCommand _copyCommand;
		PreviewControlCommand _pasteCommand;
		PreviewControlCommand _undoCommand;
		PreviewControlCommand _redoCommand;

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
					PatchHeight = 64,
					PatchWidth = 64
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
