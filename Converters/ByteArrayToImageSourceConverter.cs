using System;
using System.Globalization;
using System.Windows.Data;
using CRUDSamsonovCAD.Services;

namespace CRUDSamsonovCAD.Converters;

public sealed class ByteArrayToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is byte[] bytes ? ImageService.ToImageSource(bytes) : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Array.Empty<byte>();
    }
}

public sealed class ByteArrayNullOrEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is byte[] bytes && bytes.Length > 0
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Array.Empty<byte>();
    }
}

public sealed class ImageSourceZoomWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var verticalWidth = 600.0;
        var horizontalWidth = 900.0;

        if (parameter is string s)
        {
            var parts = s.Split(new[] { ';', ',' });
            if (parts.Length >= 2)
            {
                if (double.TryParse(parts[0], out var v)) verticalWidth = v;
                if (double.TryParse(parts[1], out var h)) horizontalWidth = h;
            }
        }

        if (value is System.Windows.Media.Imaging.BitmapSource bmp)
        {
            return bmp.PixelWidth >= bmp.PixelHeight ? horizontalWidth : verticalWidth;
        }

        return verticalWidth;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return 600.0;
    }
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var invert = parameter is string s && s.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        var flag = value is bool b && b;
        if (invert) flag = !flag;
        return flag ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}

public sealed class ParamKindToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var kind = value?.ToString() ?? "";
        var target = parameter?.ToString() ?? "";
        return kind.Equals(target, StringComparison.OrdinalIgnoreCase)
            ? System.Windows.Visibility.Visible
            : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return "Text";
    }
}
