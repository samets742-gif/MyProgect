using System.Collections.ObjectModel;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.ViewModels;

public sealed class CustomParamEditViewModel : BaseViewModel
{
    public CustomPartParamDto Param { get; }

    public ObservableCollection<string> Kinds { get; } = new()
    {
        "Text",
        "Number",
        "Date",
        "List"
    };

    public CustomParamEditViewModel(CustomPartParamDto param)
    {
        Param = param;
    }
}
