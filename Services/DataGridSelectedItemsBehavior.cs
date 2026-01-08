using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CRUDSamsonovCAD.Services;

public static class DataGridSelectedItemsBehavior
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(DataGridSelectedItemsBehavior),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    public static void SetSelectedItems(DependencyObject element, IList value)
    {
        element.SetValue(SelectedItemsProperty, value);
    }

    public static IList GetSelectedItems(DependencyObject element)
    {
        return (IList)element.GetValue(SelectedItemsProperty);
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid)
            return;

        grid.SelectionChanged -= GridOnSelectionChanged;
        if (e.NewValue != null)
            grid.SelectionChanged += GridOnSelectionChanged;
    }

    private static void GridOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not DataGrid grid)
            return;

        var target = GetSelectedItems(grid);
        if (target == null)
            return;

        target.Clear();
        foreach (var item in grid.SelectedItems)
            target.Add(item);
    }
}
