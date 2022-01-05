using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace PreviewSymbol.UIX
{
	public class PreviewControlCommand : ICommand
	{
		readonly RoutedCommand _routedCommand;

		public PreviewControlCommand(RoutedCommand routedCommand)
		{
			_routedCommand = routedCommand;
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		public bool CanExecute(object parameter)
		{
			return _routedCommand.CanExecute(parameter, GetTextArea(GetEditor(parameter)));
		}

		public void Execute(object parameter)
		{
			_routedCommand.Execute(parameter, GetTextArea(GetEditor(parameter)));
		}

		private TextEditor GetEditor(object param)
		{
			var contextMenu = param as ContextMenu;
			if (contextMenu == null) return null;
			return contextMenu.GetValue(PreviewSymbolDockpaneView.TextEditorProperty) as TextEditor;
		}
		private static TextArea GetTextArea(TextEditor editor)
		{
			return editor == null ? null : editor.TextArea;
		}
	}
}
