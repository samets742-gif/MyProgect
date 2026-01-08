using System;
using System.Globalization;
using System.Windows.Data;
using CRUDSamsonovCAD.Services;

namespace CRUDSamsonovCAD.Converters;

public sealed class PartTypeToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string s ? PartTypeProvider.GetDisplayName(s) : "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? "";
    }
}
