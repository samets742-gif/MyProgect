using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DSS_Avalonia.Views
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
            Title = title;
        }

        private void OnYes(object? sender, RoutedEventArgs e) => Close(true);
        private void OnNo(object? sender, RoutedEventArgs e)  => Close(false);
    }
}
