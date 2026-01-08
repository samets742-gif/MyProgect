using Microsoft.Win32;

namespace CRUDSamsonovCAD.Services;

public interface IFileDialogService
{
    string? ShowOpenImage();
    string? ShowOpenNxModel();
    string? ShowOpenWord();
    string? ShowOpenExcel();
    string? ShowOpenXml();
    string? ShowSaveWord();
    string? ShowSaveExcel();
    string? ShowSaveXml();
}

public sealed class FileDialogService : IFileDialogService
{
    public string? ShowOpenImage()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выберите изображение чертежа",
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowOpenNxModel()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выберите 3D-модель Siemens NX",
            Filter = "NX файлы|*.prt;*.asm;*.jt|Все файлы|*.*"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowSaveWord()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Сохранить отчет Word",
            Filter = "Word (*.docx)|*.docx",
            FileName = "Отчет_прибор.docx"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowSaveExcel()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Сохранить отчет Excel",
            Filter = "Excel (*.xlsx)|*.xlsx",
            FileName = "Отчет_прибор.xlsx"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowOpenWord()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Открыть Word",
            Filter = "Word (*.docx)|*.docx"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowOpenExcel()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Открыть Excel",
            Filter = "Excel (*.xlsx)|*.xlsx"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowOpenXml()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Открыть XML",
            Filter = "XML (*.xml)|*.xml"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    public string? ShowSaveXml()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Сохранить отчет XML",
            Filter = "XML (*.xml)|*.xml",
            FileName = "Отчет_прибор.xml"
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
