using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace CRUDSamsonovCAD.Services;

public sealed class ImportService
{
    private readonly SpecRepository _specRepo;
    private readonly CustomPartRepository _customRepo;

    public ImportService(SpecRepository specRepo, CustomPartRepository customRepo)
    {
        _specRepo = specRepo;
        _customRepo = customRepo;
    }

    public (DeviceDto Device, List<PartImportItem> Parts) ImportFromXml(string filePath)
    {
        var doc = XDocument.Load(filePath);
        var root = doc.Root ?? throw new InvalidOperationException("XML не содержит корневой элемент.");

        var device = new DeviceDto
        {
            Name = root.Element("Наименование")?.Value ?? "",
            Code = root.Element("Код")?.Value,
            Description = root.Element("Описание")?.Value
        };

        var parts = new List<PartImportItem>();
        var partsNode = root.Element("Детали");
        if (partsNode != null)
        {
            foreach (var p in partsNode.Elements("Деталь"))
            {
                var partType = p.Element("Тип")?.Value ?? "";
                var typeName = p.Element("ТипНаименование")?.Value;
                var part = new PartDto
                {
                    Name = p.Element("Наименование")?.Value ?? "",
                    PartType = ResolvePartType(partType, typeName),
                    NxModelPath = p.Element("ПутьNX")?.Value,
                    Notes = p.Element("Примечание")?.Value
                };
                var paramDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var paramRoot = p.Element("Параметры");
                if (paramRoot != null)
                {
                    foreach (var prm in paramRoot.Elements("Параметр"))
                    {
                        var name = prm.Attribute("Имя")?.Value ?? "";
                        var value = prm.Attribute("Значение")?.Value ?? "";
                        if (!string.IsNullOrWhiteSpace(name))
                            paramDict[name] = value;
                    }
                }
                parts.Add(new PartImportItem(part, paramDict));
            }
        }

        return (device, parts);
    }

    public (DeviceDto Device, List<PartImportItem> Parts) ImportFromExcel(string filePath)
    {
        using var doc = SpreadsheetDocument.Open(filePath, false);
        var wb = doc.WorkbookPart?.Workbook ?? throw new InvalidOperationException("Excel пустой.");
        var sheet = wb.Sheets?.Elements<Sheet>().FirstOrDefault()
                    ?? throw new InvalidOperationException("Лист не найден.");
        var wsPart = (WorksheetPart)doc.WorkbookPart!.GetPartById(sheet.Id!);
        var rows = wsPart.Worksheet.GetFirstChild<SheetData>()?.Elements<Row>().ToList() ?? new List<Row>();

        var device = new DeviceDto();
        var parts = new List<PartImportItem>();

        foreach (var row in rows)
        {
            var cells = row.Elements<Cell>().Select(c => GetCellValue(doc, c)).ToList();
            if (cells.Count == 0) continue;

            if (cells[0] == "Прибор" && cells.Count > 1)
                device.Name = cells[1];
            else if (cells[0] == "Код" && cells.Count > 1)
                device.Code = cells[1];
            else if (cells[0] == "Описание" && cells.Count > 1)
                device.Description = cells[1];
            else if (cells[0] == "Деталь")
            {
                // header row, skip
            }
            else if (!string.IsNullOrWhiteSpace(cells[0]) && cells.Count >= 4)
            {
                var part = new PartDto
                {
                    Name = cells[0],
                    PartType = ResolvePartType("", cells[1]),
                    NxModelPath = cells[3]
                };
                var paramDict = ParseParameters(cells[2]);
                parts.Add(new PartImportItem(part, paramDict));
            }
        }

        return (device, parts);
    }

    public (DeviceDto Device, List<PartImportItem> Parts) ImportFromWord(string filePath)
    {
        using var doc = WordprocessingDocument.Open(filePath, false);
        var body = doc.MainDocumentPart?.Document.Body ?? throw new InvalidOperationException("Word пустой.");

        var device = new DeviceDto();
        var parts = new List<PartImportItem>();

        foreach (var p in body.Elements<W.Paragraph>())
        {
            var text = p.InnerText ?? "";
            if (text.StartsWith("Прибор:", StringComparison.OrdinalIgnoreCase))
                device.Name = text.Replace("Прибор:", "").Trim();
            else if (text.StartsWith("Код:", StringComparison.OrdinalIgnoreCase))
                device.Code = text.Replace("Код:", "").Trim();
            else if (text.StartsWith("Описание:", StringComparison.OrdinalIgnoreCase))
                device.Description = text.Replace("Описание:", "").Trim();
        }

        var table = body.Elements<W.Table>().FirstOrDefault();
        if (table != null)
        {
            var rows = table.Elements<W.TableRow>().Skip(1);
            foreach (var row in rows)
            {
                var cells = row.Elements<W.TableCell>().Select(c => c.InnerText ?? "").ToList();
                if (cells.Count < 4) continue;
                var part = new PartDto
                {
                    Name = cells[0],
                    PartType = ResolvePartType("", cells[1]),
                    NxModelPath = cells[3]
                };
                var paramDict = ParseParameters(cells[2]);
                parts.Add(new PartImportItem(part, paramDict));
            }
        }

        return (device, parts);
    }

    public object CreateSpecFromParameters(string partType, Dictionary<string, string> parameters)
    {
        if (PartTypeProvider.IsCustom(partType, out _))
            throw new InvalidOperationException("Для пользовательского типа создаются CustomPartValues.");

        switch (partType)
        {
            case "Spec_07_02_Bracket":
                return new Spec07_02_BracketDto
                {
                    Length = ToDecimal(parameters, "Длина"),
                    Width = ToDecimal(parameters, "Ширина"),
                    Thickness = ToDecimal(parameters, "Толщина"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_05_Roller":
                return new Spec07_05_RollerDto
                {
                    Diameter = ToDecimal(parameters, "Диаметр"),
                    Width = ToDecimal(parameters, "Ширина"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_06_Frame":
                return new Spec07_06_FrameDto
                {
                    Length = ToDecimal(parameters, "Длина"),
                    Width = ToDecimal(parameters, "Ширина"),
                    Height = ToDecimal(parameters, "Высота"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_07_Roller":
                return new Spec07_07_RollerDto
                {
                    Diameter = ToDecimal(parameters, "Диаметр"),
                    Width = ToDecimal(parameters, "Ширина"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_08_Adapter":
                return new Spec07_08_AdapterDto
                {
                    Length = ToDecimal(parameters, "Длина"),
                    OuterDiameter = ToDecimal(parameters, "Наружный диаметр"),
                    InnerDiameter = ToDecimal(parameters, "Внутренний диаметр"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_10_Spring":
                return new Spec07_10_SpringDto
                {
                    WireDiameter = ToDecimal(parameters, "Диаметр проволоки"),
                    OuterDiameter = ToDecimal(parameters, "Наружный диаметр"),
                    FreeLength = ToDecimal(parameters, "Свободная длина"),
                    CoilsCount = ToInt(parameters, "Число витков"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_11_Stop":
                return new Spec07_11_StopDto
                {
                    Length = ToDecimal(parameters, "Длина"),
                    Width = ToDecimal(parameters, "Ширина"),
                    Height = ToDecimal(parameters, "Высота"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_14_Ring":
                return new Spec07_14_RingDto
                {
                    OuterDiameter = ToDecimal(parameters, "Наружный диаметр"),
                    InnerDiameter = ToDecimal(parameters, "Внутренний диаметр"),
                    Thickness = ToDecimal(parameters, "Толщина"),
                    Material = Get(parameters, "Материал")
                };
            case "Spec_07_15_Screw":
                return new Spec07_15_ScrewDto
                {
                    Diameter = ToDecimal(parameters, "Диаметр"),
                    Length = ToDecimal(parameters, "Длина"),
                    ThreadPitch = ToDecimal(parameters, "Шаг резьбы"),
                    HeadType = Get(parameters, "Тип головки"),
                    Material = Get(parameters, "Материал")
                };
        }

        return _specRepo.CreateEmptySpec(partType, 0);
    }

    public List<CustomPartValueDto> CreateCustomValues(int typeId, Dictionary<string, string> parameters)
    {
        var values = new List<CustomPartValueDto>();
        foreach (var p in _customRepo.GetParams(typeId))
        {
            if (parameters.TryGetValue(p.ParamName, out var val))
            {
                values.Add(new CustomPartValueDto
                {
                    ParamId = p.Id,
                    ValueText = val
                });
            }
        }
        return values;
    }

    private string ResolvePartType(string rawType, string? typeName)
    {
        if (!string.IsNullOrWhiteSpace(rawType) && PartTypeProvider.Items.Any(i => i.Key == rawType))
            return rawType;

        if (!string.IsNullOrWhiteSpace(rawType) && PartTypeProvider.IsCustom(rawType, out _))
            return rawType;

        var standard = PartTypeProvider.Items.FirstOrDefault(i => i.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        if (standard != null) return standard.Key;

        if (!string.IsNullOrWhiteSpace(typeName))
        {
            var custom = _customRepo.GetTypes().FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
            if (custom != null) return PartTypeProvider.GetCustomKey(custom.Id);
        }

        return rawType;
    }

    private static Dictionary<string, string> ParseParameters(string raw)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw)) return dict;
        var parts = raw.Split(';');
        foreach (var p in parts)
        {
            var kv = p.Split('=');
            if (kv.Length < 2) continue;
            var key = kv[0].Trim();
            var value = string.Join("=", kv.Skip(1)).Trim();
            if (key.Length > 0)
                dict[key] = value;
        }
        return dict;
    }

    private static string? Get(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out var v) ? v : null;

    private static decimal? ToDecimal(Dictionary<string, string> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v)) return null;
        if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.CurrentCulture, out var c)) return c;
        if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
        return null;
    }

    private static int? ToInt(Dictionary<string, string> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v)) return null;
        if (int.TryParse(v, NumberStyles.Any, CultureInfo.CurrentCulture, out var c)) return c;
        if (int.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) return i;
        return null;
    }

    private static string GetCellValue(SpreadsheetDocument doc, Cell cell)
    {
        var value = cell.CellValue?.InnerText ?? "";
        if (cell.DataType == null) return value;
        if (cell.DataType == CellValues.SharedString)
        {
            var sst = doc.WorkbookPart?.SharedStringTablePart?.SharedStringTable;
            if (sst == null) return value;
            return sst.ElementAt(int.Parse(value)).InnerText;
        }
        return value;
    }
}

public sealed class PartImportItem
{
    public PartDto Part { get; }
    public Dictionary<string, string> Parameters { get; }

    public PartImportItem(PartDto part, Dictionary<string, string> parameters)
    {
        Part = part;
        Parameters = parameters;
    }
}
