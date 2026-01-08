using System.Windows;

namespace CRUDSamsonovCAD.Views;

public partial class PartEditWindow : Window
{
    public PartEditWindow()
    {
        InitializeComponent();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.PartEditViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Part.Name))
            {
                MessageBox.Show("Введите наименование детали.", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
