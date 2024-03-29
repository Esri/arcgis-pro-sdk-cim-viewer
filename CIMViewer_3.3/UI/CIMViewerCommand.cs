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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;

namespace CIMViewer.UI {
    public class CIMViewerCommand : ICommand {

        readonly RoutedCommand _routedCommand;

        public CIMViewerCommand(RoutedCommand routedCommand) {
            _routedCommand = routedCommand;
        }

        public event EventHandler CanExecuteChanged {
            add {
                CommandManager.RequerySuggested += value;
            }
            remove {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter) {
            return _routedCommand.CanExecute(parameter, GetTextArea(GetEditor(parameter)));
        }

        public void Execute(object parameter) {
            _routedCommand.Execute(parameter, GetTextArea(GetEditor(parameter)));
        }
        private TextEditor GetEditor(object param) {
            var contextMenu = param as ContextMenu;
            if (contextMenu == null) return null;
            return contextMenu.GetValue(CIMViewer.TextEditorProperty) as TextEditor;
        }
        private static TextArea GetTextArea(TextEditor editor) {
            return editor == null ? null : editor.TextArea;
        }
    }
}
