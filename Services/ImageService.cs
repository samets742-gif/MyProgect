using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CRUDSamsonovCAD.Services;

public static class ImageService
{
    public static byte[] LoadImageBytes(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    public static ImageSource? ToImageSource(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        using var ms = new MemoryStream(bytes);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}
