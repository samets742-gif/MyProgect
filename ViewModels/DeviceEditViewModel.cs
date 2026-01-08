using System.Windows.Input;
using System.Windows.Media;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.Services;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class DeviceEditViewModel : BaseViewModel
{
    public DeviceDto Device { get; }
    private readonly IFileDialogService _fileDialog;

    private ImageSource? _drawingImagePreview;
    public ImageSource? DrawingImagePreview
    {
        get => _drawingImagePreview;
        set { _drawingImagePreview = value; OnPropertyChanged(); }
    }

    public ICommand BrowseImageCommand { get; }
    public ICommand ClearImageCommand { get; }
    public ICommand BrowseNxCommand { get; }

    public DeviceEditViewModel(DeviceDto device, IFileDialogService fileDialog)
    {
        Device = device;
        _fileDialog = fileDialog;
        DrawingImagePreview = ImageService.ToImageSource(Device.DrawingImage);

        BrowseImageCommand = new RelayCommand(_ => BrowseImage());
        ClearImageCommand = new RelayCommand(_ => ClearImage());
        BrowseNxCommand = new RelayCommand(_ => BrowseNx());
    }

    private void BrowseImage()
    {
        var path = _fileDialog.ShowOpenImage();
        if (string.IsNullOrWhiteSpace(path)) return;
        Device.DrawingImage = ImageService.LoadImageBytes(path);
        DrawingImagePreview = ImageService.ToImageSource(Device.DrawingImage);
    }

    private void ClearImage()
    {
        Device.DrawingImage = null;
        DrawingImagePreview = null;
    }

    private void BrowseNx()
    {
        var path = _fileDialog.ShowOpenNxModel();
        if (string.IsNullOrWhiteSpace(path)) return;
        Device.NxModelPath = path;
        OnPropertyChanged(nameof(Device));
    }
}
