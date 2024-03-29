using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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


namespace PreviewSymbol.UIX
{
	/// <summary>
	/// Interaction logic for PreviewSymbolDockpaneView.xaml
	/// </summary>
	public partial class PreviewSymbolDockpaneView : UserControl, INotifyPropertyChanged
	{

		public event PropertyChangedEventHandler PropertyChanged;

		public PreviewSymbolDockpaneView()
		{
			InitializeComponent();
			this.Loaded += PreviewSymbolView_Loaded;
		}

		private void PreviewSymbolView_Loaded(object sender, RoutedEventArgs e)
		{
			var vm = this.DataContext as PreviewSymbolDockpaneViewModel;
			this.AvalonTextEditor.ContextMenu.SetValue(TextEditorProperty,
									this.AvalonTextEditor);
			vm.SetAvalonTextEditor(this.AvalonTextEditor);
			Loaded -= PreviewSymbolView_Loaded;//one shot
		}

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
			DependencyProperty.RegisterAttached("TextEditor", typeof(TextEditor), typeof(PreviewSymbolDockpaneView),
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
	}
}
