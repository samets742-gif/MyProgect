using System.Windows;

namespace CRUDSamsonovCAD.Views;

public partial class CustomTypeEditWindow : Window
{
    public CustomTypeEditWindow()
    {
        InitializeComponent();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
