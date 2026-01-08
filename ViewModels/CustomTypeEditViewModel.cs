using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class CustomTypeEditViewModel : BaseViewModel
{
    public CustomPartTypeDto Type { get; }

    public CustomTypeEditViewModel(CustomPartTypeDto type)
    {
        Type = type;
    }
}
