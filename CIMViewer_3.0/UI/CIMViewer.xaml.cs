//   Copyright 2016 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using CIMViewer.Helpers;

namespace CIMViewer.UI
{
  /// <summary>
  /// Interaction logic for CIMViewer.xaml
  /// </summary>
  /// <remarks>Context menu and Cut, Copy, Paste issues for Avalon TextEditor:
  /// <a href="http://stackoverflow.com/questions/14909421/hooking-up-commands-in-avalonedit-used-in-listviews-itemtemplate-doesnt-work"/>
  /// </remarks>
  public partial class CIMViewer : UserControl, INotifyPropertyChanged
  {

    ////private TextEditor _editor;
    private FoldingManager _foldingManager;
    private XmlFoldingStrategy _xmlFolding;
    private CIMService _cimService = null;
    private string _validationText = "";

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    public CIMViewer()
    {
      InitializeComponent();

      _foldingManager = FoldingManager.Install(this.AvalonTextEditor.TextArea);
      _xmlFolding = new XmlFoldingStrategy();
      //http://blog.jerrynixon.com/2013/07/solved-two-way-binding-inside-user.html
      (this.Content as FrameworkElement).DataContext = this;
      //this.AvalonTextEditor.DataContext = this;

      ////http://stackoverflow.com/questions/13344982/does-avalonedit-texteditor-has-quick-search-replace-functionality
      //this.AvalonTextEditor.TextArea.DefaultInputHandler.NestedInputHandlers.Add(
      //    new SearchInputHandler(this.AvalonTextEditor.TextArea));
      SearchPanel.Install(this.AvalonTextEditor);
      this.Loaded += (s, e) =>
      {
        try
        {
          this.AvalonTextEditor.ContextMenu.SetValue(CIMViewer.TextEditorProperty,
              this.AvalonTextEditor);
        }
        catch (System.Exception ex)
        {
          System.Diagnostics.Debug.WriteLine("Exception: {0}", ex.ToString());
        }
      };
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
      throw new NotImplementedException();
    }

    #region CIMService Property

    public static readonly DependencyProperty CIMServiceProperty =
    DependencyProperty.Register("CIMService", typeof(CIMService), typeof(CIMViewer),
        new FrameworkPropertyMetadata(null,
            new PropertyChangedCallback(CIMServicePropertyChanged)));

    private static async void CIMServicePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      CIMViewer _this = sender as CIMViewer;
      if (e.NewValue == null)
      {
        _this.AvalonTextEditor.Text = "";
        _this._cimService = null;
      }
      else
      {
        _this._cimService = (CIMService)e.NewValue;
        var xml = await _this._cimService.GetDefinitionAsync();
        _this.AvalonTextEditor.Text = FormatXml(xml);
        _this._xmlFolding.UpdateFoldings(_this._foldingManager, _this.AvalonTextEditor.Document);
      }
      _this._validationText = "";
      _this.OnPropertyChanged("ValidationText");
    }

    public CIMService CIMService
    {
      get
      {
        return (CIMService)GetValue(CIMServiceProperty);
      }
      set
      {
        SetValue(CIMServiceProperty, value);
      }
    }

    #endregion CIMService Property

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
        DependencyProperty.RegisterAttached("TextEditor", typeof(TextEditor), typeof(CIMViewer),
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

    public string ValidationText => _validationText;

    #region Commands

    ICommand _refreshCommand;
    ICommand _saveCommand;
    ICommand _clearCommand;
    ICommand _validateCommand;
    ICommand _changeFontSizeCommand;
    ICommand _updateFoldingsCommand;


    public ICommand RefreshCommand
    {
      get
      {
        return _refreshCommand ?? (_refreshCommand = new RelayCommand(async () =>
        {
          if (CIMService != null)
          {
            CIMViewerModule.IgnoreEvents = true;
            var xml = await _cimService.GetDefinitionAsync();
            CIMViewerModule.IgnoreEvents = false;
            this.AvalonTextEditor.Text = FormatXml(xml);
            this._xmlFolding.UpdateFoldings(this._foldingManager, this.AvalonTextEditor.Document);
            this._validationText = "";
            this.OnPropertyChanged("ValidationText");
          }
        }));
      }
    }

    public ICommand SaveCommand
    {
      get
      {
        return _saveCommand ?? (_saveCommand = new RelayCommand(async () =>
        {
          if (CIMService != null)
          {
            if (!string.IsNullOrEmpty(this.AvalonTextEditor.Text))
            {
              try
              {
                CIMViewerModule.IgnoreEvents = true;
                await _cimService.SetDefinitionAsync(this.AvalonTextEditor.Text);
                CIMViewerModule.IgnoreEvents = false;
                MessageBox.Show("Definition saved", "Save", MessageBoxButton.OK,
                    MessageBoxImage.Information);
              }
              catch (Exception ex)
              {
                CIMViewerModule.IgnoreEvents = false;
                MessageBox.Show(string.Format("Error saving definition {0}",
                           ex.Message), "Save Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
              }

            }
          }
        }));
      }
    }

    public ICommand ClearCommand
    {
      get
      {
        return _clearCommand ?? (_clearCommand = new RelayCommand(() => CIMService = null));
      }
    }

    public ICommand ValidateCommand
    {
      get
      {
        return _validateCommand ?? (_validateCommand = new RelayCommand(() => Validate()));
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
        return _updateFoldingsCommand ?? (_updateFoldingsCommand = new RelayCommand(
            () => {
              if (!string.IsNullOrEmpty(this.AvalonTextEditor.Text))
                this._xmlFolding.UpdateFoldings(this._foldingManager, this.AvalonTextEditor.Document);
            }
          ));
      }
    }

    private void ChangeTextSize(object cmdParam)
    {
      string delta = cmdParam.ToString();

      this.AvalonTextEditor.FontSize = (delta == "-1"
          ? this.AvalonTextEditor.FontSize - 1
          : this.AvalonTextEditor.FontSize + 1);
    }

    #region Editing Commands

    CIMViewerCommand _copyCommand;
    CIMViewerCommand _pasteCommand;
    CIMViewerCommand _cutCommand;
    CIMViewerCommand _undoCommand;
    CIMViewerCommand _redoCommand;

    public CIMViewerCommand CutCommand
    {
      get
      {
        return _cutCommand ?? (_cutCommand = new CIMViewerCommand(ApplicationCommands.Cut));
      }
    }

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

    #endregion Editing Commands

    #endregion Commands

    private static string FormatXml(string xml)
    {
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

    private void Validate()
    {

      try
      {
        var document = new XmlDocument { XmlResolver = null };
        document.LoadXml(this.AvalonTextEditor.Text);
        _validationText = "No errors";
      }
      catch (XmlException ex)
      {
        _validationText = string.Format("Error: {0}\r\n", ex.Message);
        DisplayValidationError(ex.Message, ex.LinePosition, ex.LineNumber);
      }
      this.Validation.IsExpanded = true;
      OnPropertyChanged("ValidationText");
    }

    private void DisplayValidationError(string message, int linePosition, int lineNumber)
    {
      if (lineNumber >= 1 && lineNumber <= this.AvalonTextEditor.Document.LineCount)
      {
        int index = message.ToLower().IndexOf(" line ");
        int index2 = -1;
        int beginLine = -1;
        int beginPos = -1;
        int offset1 = -1;
        //Example error message:
        //"The 'VerticalExaggeration' start tag on line 24 position 29 does not match the end tag of 'Layer3DProperties'.
        if (index >= 0)
        {
          index2 = message.Substring(index + 6).ToLower().IndexOf(" position ");
          string line = message.Substring(index + 6, index2).Replace(",", "").Trim();

          //now position
          string remainder = message.Substring(index + 6 + index2 + 10);
          string position = remainder.Substring(0, remainder.IndexOf(" ")).Replace(",", "").Trim();

          if (Int32.TryParse(line, out beginLine) && Int32.TryParse(position, out beginPos))
          {
            offset1 = this.AvalonTextEditor.Document.GetOffset(new TextLocation(beginLine, beginPos - 1));
          }
        }
        int offset2 = this.AvalonTextEditor.Document.GetOffset(new TextLocation(lineNumber, linePosition));
        if (offset1 > 0)
        {
          this.AvalonTextEditor.Select(offset1, (offset2 - offset1));
        }
        else
        {
          offset1 = this.AvalonTextEditor.Document.GetOffset(new TextLocation(lineNumber, 0));
          this.AvalonTextEditor.Select(offset2, (offset2 - offset1));
        }
      }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propName = "")
    {
      PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      this.AvalonTextEditor.FontSize = (e.Delta < 0
          ? this.AvalonTextEditor.FontSize - 1
          : this.AvalonTextEditor.FontSize + 1);
    }
  }
}
