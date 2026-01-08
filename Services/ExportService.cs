using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using W = DocumentFormat.OpenXml.Wordprocessing;
using A = DocumentFormat.OpenXml.Drawing;
using DW = DocumentFormat.OpenXml.Drawing.Wordprocessing;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;

namespace CRUDSamsonovCAD.Services;

public sealed class ExportService
{
    private readonly SpecRepository _specRepo;
    private readonly CustomPartRepository _customRepo;
    public ExportService(SpecRepository specRepo, CustomPartRepository customRepo)
    {
        _specRepo = specRepo;
        _customRepo = customRepo;
    }

    public void ExportToWord(string filePath, DeviceDto device, IEnumerable<PartDto> parts, bool includeDeviceDrawing = true)
    {
        using var doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new W.Document(new W.Body());
        var body = main.Document.Body!;

        body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"Прибор: {device.Name}"))));
        if (!string.IsNullOrWhiteSpace(device.Code))
            body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"Код: {device.Code}"))));
        if (!string.IsNullOrWhiteSpace(device.Description))
            body.AppendChild(new W.Paragraph(new W.Run(new W.Text($"Описание: {device.Description}"))));

        if (includeDeviceDrawing)
            AddImageSection(doc, body, "Чертеж сборки", device.DrawingImage);

        var table = new W.Table();
        table.AppendChild(new W.TableProperties(new W.TableBorders(
            new W.TopBorder { Val = W.BorderValues.Single },
            new W.BottomBorder { Val = W.BorderValues.Single },
            new W.LeftBorder { Val = W.BorderValues.Single },
            new W.RightBorder { Val = W.BorderValues.Single },
            new W.InsideHorizontalBorder { Val = W.BorderValues.Single },
            new W.InsideVerticalBorder { Val = W.BorderValues.Single }
        )));

        table.AppendChild(MakeWordRow("Деталь", "Тип", "Параметры", "Путь NX"));

        foreach (var part in parts)
        {
            var parameters = GetParameters(part);

            table.AppendChild(MakeWordRow(part.Name, GetDisplayName(part), parameters, part.NxModelPath ?? ""));
        }

        body.AppendChild(table);

        foreach (var part in parts)
        {
            AddImageSection(doc, body, $"Чертеж детали: {part.Name}", part.DrawingImage);
        }

        main.Document.Save();
    }

    public void ExportToExcel(string filePath, DeviceDto device, IEnumerable<PartDto> parts)
    {
        using var doc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
        var wbPart = doc.AddWorkbookPart();
        wbPart.Workbook = new Workbook();

        var wsPart = wbPart.AddNewPart<WorksheetPart>();
        var sheetData = new SheetData();
        wsPart.Worksheet = new Worksheet(sheetData);

        var sheets = doc.WorkbookPart!.Workbook.AppendChild(new Sheets());
        var sheet = new Sheet { Id = doc.WorkbookPart.GetIdOfPart(wsPart), SheetId = 1, Name = "Прибор" };
        sheets.Append(sheet);

        sheetData.Append(MakeExcelRow("Прибор", device.Name));
        if (!string.IsNullOrWhiteSpace(device.Code))
            sheetData.Append(MakeExcelRow("Код", device.Code));
        if (!string.IsNullOrWhiteSpace(device.Description))
            sheetData.Append(MakeExcelRow("Описание", device.Description));

        sheetData.Append(MakeExcelRow("Деталь", "Тип", "Параметры", "Путь NX"));

        foreach (var part in parts)
        {
            var parameters = GetParameters(part);

            sheetData.Append(MakeExcelRow(part.Name, GetDisplayName(part), parameters, part.NxModelPath ?? ""));
        }

        wbPart.Workbook.Save();
    }

    public void ExportToXml(string filePath, DeviceDto device, IEnumerable<PartDto> parts)
    {
        var root = new XElement("Прибор",
            new XAttribute("Id", device.Id),
            new XElement("Наименование", device.Name),
            new XElement("Код", device.Code ?? string.Empty),
            new XElement("Описание", device.Description ?? string.Empty),
            new XElement("Детали",
                parts.Select(p =>
                {
                    var specFields = GetFields(p);

                    return new XElement("Деталь",
                        new XAttribute("Id", p.Id),
                        new XElement("Наименование", p.Name),
                        new XElement("Тип", p.PartType),
                        new XElement("ТипНаименование", GetDisplayName(p)),
                        new XElement("ПутьNX", p.NxModelPath ?? string.Empty),
                        new XElement("Примечание", p.Notes ?? string.Empty),
                        new XElement("Параметры",
                            specFields.Select(f =>
                                new XElement("Параметр",
                                    new XAttribute("Имя", f.Name),
                                    new XAttribute("Значение", f.Value))))
                    );
                })
            ));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
        doc.Save(filePath);
    }

    private string GetParameters(PartDto part)
    {
        var fields = GetFields(part);
        return string.Join("; ", fields.Select(f => $"{f.Name}={f.Value}"));
    }

    private IEnumerable<(string Name, string Value)> GetFields(PartDto part)
    {
        if (PartTypeProvider.IsCustom(part.PartType, out _))
            return _customRepo.GetFields(part.Id);

        var spec = _specRepo.GetSpec(part.PartType, part.Id);
        return spec == null ? Enumerable.Empty<(string Name, string Value)>() : SpecFormatter.GetFields(spec);
    }

    private string GetDisplayName(PartDto part)
    {
        if (PartTypeProvider.IsCustom(part.PartType, out var id))
        {
            var type = _customRepo.GetTypes().FirstOrDefault(t => t.Id == id);
            return type?.Name ?? part.PartType;
        }
        return PartTypeProvider.GetDisplayName(part.PartType);
    }

    private static void AddImageSection(WordprocessingDocument doc, W.Body body, string title, byte[]? imageBytes)
    {
        body.AppendChild(new W.Paragraph(new W.Run(new W.Text(title))));
        if (imageBytes == null || imageBytes.Length == 0)
        {
            body.AppendChild(new W.Paragraph(new W.Run(new W.Text("Чертеж отсутствует"))));
            return;
        }

        var imageContentType = GetImageContentType(imageBytes);
        var mainPart = doc.MainDocumentPart!;
        var imagePart = mainPart.AddImagePart(imageContentType);
        using (var stream = new MemoryStream(imageBytes))
            imagePart.FeedData(stream);

        var relId = mainPart.GetIdOfPart(imagePart);
        var (cx, cy) = GetImageSizeEmu(imageBytes, 600);

        var drawing = new W.Drawing(
            new DW.Inline(
                new DW.Extent { Cx = cx, Cy = cy },
                new DW.EffectExtent
                {
                    LeftEdge = 0,
                    TopEdge = 0,
                    RightEdge = 0,
                    BottomEdge = 0
                },
                new DW.DocProperties { Id = 1U, Name = "Picture" },
                new DW.NonVisualGraphicFrameDrawingProperties(new A.GraphicFrameLocks { NoChangeAspect = true }),
                new A.Graphic(
                    new A.GraphicData(
                        new PIC.Picture(
                            new PIC.NonVisualPictureProperties(
                                new PIC.NonVisualDrawingProperties { Id = 0U, Name = "Drawing" },
                                new PIC.NonVisualPictureDrawingProperties()),
                            new PIC.BlipFill(
                                new A.Blip { Embed = relId },
                                new A.Stretch(new A.FillRectangle())),
                            new PIC.ShapeProperties(
                                new A.Transform2D(
                                    new A.Offset { X = 0, Y = 0 },
                                    new A.Extents { Cx = cx, Cy = cy }),
                                new A.PresetGeometry(new A.AdjustValueList())
                                { Preset = A.ShapeTypeValues.Rectangle })))
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }))
        {
            DistanceFromTop = 0U,
            DistanceFromBottom = 0U,
            DistanceFromLeft = 0U,
            DistanceFromRight = 0U
        });

        body.AppendChild(new W.Paragraph(new W.Run(drawing)));
    }

    private static (long Cx, long Cy) GetImageSizeEmu(byte[] bytes, int maxWidthPx)
    {
        using var ms = new MemoryStream(bytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        var frame = decoder.Frames[0];
        var width = frame.PixelWidth;
        var height = frame.PixelHeight;

        if (width > maxWidthPx)
        {
            var scale = (double)maxWidthPx / width;
            width = maxWidthPx;
            height = (int)(height * scale);
        }

        const int emusPerPx = 9525;
        return (width * emusPerPx, height * emusPerPx);
    }

    private static string GetImageContentType(byte[] bytes)
    {
        if (bytes.Length >= 4 &&
            bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return "image/png";
        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xD8)
            return "image/jpeg";
        if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D)
            return "image/bmp";
        if (bytes.Length >= 4 &&
            ((bytes[0] == 0x49 && bytes[1] == 0x49 && bytes[2] == 0x2A && bytes[3] == 0x00) ||
             (bytes[0] == 0x4D && bytes[1] == 0x4D && bytes[2] == 0x00 && bytes[3] == 0x2A)))
            return "image/tiff";
        return "image/png";
    }

    private static W.TableRow MakeWordRow(params string[] values)
    {
        var row = new W.TableRow();
        foreach (var v in values)
        {
            row.AppendChild(new W.TableCell(new W.Paragraph(new W.Run(new W.Text(v ?? "")))));
        }
        return row;
    }

    private static Row MakeExcelRow(params string[] values)
    {
        var row = new Row();
        foreach (var v in values)
        {
            row.AppendChild(new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(v ?? "")
            });
        }
        return row;
    }
}
