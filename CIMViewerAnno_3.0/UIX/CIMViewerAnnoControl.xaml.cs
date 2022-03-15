using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using CIMViewerAnno.Helpers;

namespace CIMViewerAnno.UIX
{
  /// <summary>
  /// Interaction logic for CIMViewerAnno.xaml
  /// </summary>
  public partial class CIMViewerAnnoControl : UserControl, INotifyPropertyChanged
  {

    private FoldingManager _foldingManager;
    private XmlFoldingStrategy _xmlFolding;
    private ImageSource _img;

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public CIMViewerAnnoControl()
    {
      InitializeComponent();
      _foldingManager = FoldingManager.Install(this.AvalonTextEditor.TextArea);
      _xmlFolding = new XmlFoldingStrategy();
      (this.Content as FrameworkElement).DataContext = this;
    }

    #region CIMTextGraphic

    public static readonly DependencyProperty CIMTextGraphicProperty =
      DependencyProperty.Register("CIMTextGraphic", typeof(CIMTextGraphic), typeof(CIMViewerAnnoControl),
        new FrameworkPropertyMetadata(null,
          new PropertyChangedCallback(CIMTextGraphicPropertyChanged)));

    private static async void CIMTextGraphicPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      CIMViewerAnnoControl _this = sender as CIMViewerAnnoControl;
      if (e.NewValue == null) {
        _this.AvalonTextEditor.Text = "";
        _this._img = null;
        _this.NotifyPropertyChanged("TextGraphicImageSource");
      }
      else {
        _this.SetXmlText(_this.CIMTextGraphic);
        _this._img = await _this.GenerateBitmapImageAsync(_this.AvalonTextEditor.Text);
        _this.NotifyPropertyChanged("TextGraphicImageSource");
      }
    }

    public CIMTextGraphic CIMTextGraphic
    {
      get
      {
        return (CIMTextGraphic)GetValue(CIMTextGraphicProperty);
      }
      set
      {
        SetValue(CIMTextGraphicProperty, value);
      }
    }

    private void SetXmlText(CIMTextGraphic textGraphic)
    {
      var xml = textGraphic?.ToXml() ?? "";
      this.AvalonTextEditor.Text = FormatXml(xml);
      this._xmlFolding.UpdateFoldings(this._foldingManager, this.AvalonTextEditor.Document);
    }

    #endregion CIMTextGraphic

    #region TextEditor Property

    public static TextEditor GetTextEditor(ContextMenu menu)
    {
      return (TextEditor)menu.GetValue(TextEditorProperty);
    }

    public static void SetTextEditor(ContextMenu menu, TextEditor value)
    {
      menu.SetValue(TextEditorProperty, value);
    }

    public static readonly DependencyProperty TextEditorProperty =
      DependencyProperty.RegisterAttached("TextEditor", typeof(TextEditor), typeof(CIMViewerAnnoControl),
        new UIPropertyMetadata(null, OnTextEditorChanged));

    static void OnTextEditorChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
    {
      ContextMenu menu = depObj as ContextMenu;
      if (menu == null || e.NewValue is DependencyObject == false)
        return;
      TextEditor editor = (TextEditor)e.NewValue;
      NameScope.SetNameScope(menu, NameScope.GetNameScope(editor));
    }

    #endregion

    #region Graphic ImageSource

    public ImageSource TextGraphicImageSource => _img;

    #endregion Graphic ImageSource

    #region Commands

    ICommand _refreshCommand;
    ICommand _saveCommand;
    ICommand _applyCommand;
    ICommand _changeFontSizeCommand;

    public ICommand RefreshCommand
    {
      get
      {
        return _refreshCommand ?? (_refreshCommand = new RelayCommand(() => {
          this.CIMTextGraphic = Module1.Current.SelectedGraphic;
        }));
      }
    }

    public ICommand SaveCommand
    {
      get
      {
        return _saveCommand ?? (_saveCommand = new RelayCommand(async () => {
          var ok = await Module1.Current.ChangeAnnotationTextGraphicAsync(this.AvalonTextEditor.Text);
          //always do the preview
          _img = await GenerateBitmapImageAsync(this.AvalonTextEditor.Text);
          NotifyPropertyChanged("TextGraphicImageSource");
          if (!ok) {
            if (Module1.Current.LastError.Length > 0)
              MessageBox.Show(Module1.Current.LastError, "Save Failed",MessageBoxButton.OK,MessageBoxImage.Error);
          }
        }));
      }
    }

    public ICommand ApplyCommand
    {
      get
      {
        return _applyCommand ?? (_applyCommand = new RelayCommand(async () => {
          _img = await GenerateBitmapImageAsync(this.AvalonTextEditor.Text);
          NotifyPropertyChanged("TextGraphicImageSource");
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

    private void ChangeTextSize(object cmdParam)
    {
      string delta = cmdParam.ToString();

      this.AvalonTextEditor.FontSize = (delta == "-1"
        ? this.AvalonTextEditor.FontSize - 1
        : this.AvalonTextEditor.FontSize + 1);
    }

    private Task<ImageSource> GenerateBitmapImageAsync(string graphicXml)
    {
      return QueuedTask.Run(() => {
        if (string.IsNullOrEmpty(graphicXml))
          return null;
        var textGraphic = Module1.Current.GraphicFromXml(graphicXml);
        var si = new SymbolStyleItem() {
          Symbol = textGraphic.Symbol.Symbol,
          PatchHeight = 80,
          PatchWidth = 80
        };
        return si.PreviewImage;
      });
    }


    CIMViewerCommand _copyCommand;
    CIMViewerCommand _pasteCommand;
    CIMViewerCommand _undoCommand;
    CIMViewerCommand _redoCommand;

    public CIMViewerCommand CopyCommand
    {
      get
      {
        return _copyCommand ?? (_copyCommand = new CIMViewerCommand(ApplicationCommands.Copy));
      }
    }

    public CIMViewerCommand PasteCommand
    {
      get
      {
        return _pasteCommand ?? (_pasteCommand = new CIMViewerCommand(ApplicationCommands.Paste));
      }
    }

    public CIMViewerCommand UndoCommand
    {
      get
      {
        return _undoCommand ?? (_undoCommand = new CIMViewerCommand(ApplicationCommands.Undo));
      }
    }

    public CIMViewerCommand RedoCommand
    {
      get
      {
        return _redoCommand ?? (_redoCommand = new CIMViewerCommand(ApplicationCommands.Redo));
      }
    }

    #endregion Commands

    protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = "")
    {
      PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    private static string FormatXml(string xml)
    {
      if (string.IsNullOrEmpty(xml))
        return "";
      var doc = new XmlDocument();
      var sb = new StringBuilder();
      try {
        doc.LoadXml(xml);
        var xmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };
        doc.Save(XmlWriter.Create(sb, xmlWriterSettings));
      }
      catch (System.Xml.XmlException xmle) {
        System.Diagnostics.Debug.WriteLine("FormatXml Exception: {0}", xmle.ToString());
        sb.Append(xml);
      }
      return sb.ToString();
    }
  }
}
