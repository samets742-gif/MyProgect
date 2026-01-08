using System.Collections.Generic;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.Services;

public static class SpecFormatter
{
    public static IReadOnlyList<(string Name, string Value)> GetFields(object spec)
    {
        var list = new List<(string, string)>();
        switch (spec)
        {
            case Spec07_02_BracketDto s:
                Add(list, "Длина", s.Length);
                Add(list, "Ширина", s.Width);
                Add(list, "Толщина", s.Thickness);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_05_RollerDto s:
                Add(list, "Диаметр", s.Diameter);
                Add(list, "Ширина", s.Width);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_06_FrameDto s:
                Add(list, "Длина", s.Length);
                Add(list, "Ширина", s.Width);
                Add(list, "Высота", s.Height);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_07_RollerDto s:
                Add(list, "Диаметр", s.Diameter);
                Add(list, "Ширина", s.Width);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_08_AdapterDto s:
                Add(list, "Длина", s.Length);
                Add(list, "Наружный диаметр", s.OuterDiameter);
                Add(list, "Внутренний диаметр", s.InnerDiameter);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_10_SpringDto s:
                Add(list, "Диаметр проволоки", s.WireDiameter);
                Add(list, "Наружный диаметр", s.OuterDiameter);
                Add(list, "Свободная длина", s.FreeLength);
                Add(list, "Число витков", s.CoilsCount);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_11_StopDto s:
                Add(list, "Длина", s.Length);
                Add(list, "Ширина", s.Width);
                Add(list, "Высота", s.Height);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_14_RingDto s:
                Add(list, "Наружный диаметр", s.OuterDiameter);
                Add(list, "Внутренний диаметр", s.InnerDiameter);
                Add(list, "Толщина", s.Thickness);
                Add(list, "Материал", s.Material);
                break;
            case Spec07_15_ScrewDto s:
                Add(list, "Диаметр", s.Diameter);
                Add(list, "Длина", s.Length);
                Add(list, "Шаг резьбы", s.ThreadPitch);
                Add(list, "Тип головки", s.HeadType);
                Add(list, "Материал", s.Material);
                break;
        }
        return list;
    }

    public static void SetPartId(object spec, int partId)
    {
        switch (spec)
        {
            case Spec07_02_BracketDto s: s.PartId = partId; break;
            case Spec07_05_RollerDto s: s.PartId = partId; break;
            case Spec07_06_FrameDto s: s.PartId = partId; break;
            case Spec07_07_RollerDto s: s.PartId = partId; break;
            case Spec07_08_AdapterDto s: s.PartId = partId; break;
            case Spec07_10_SpringDto s: s.PartId = partId; break;
            case Spec07_11_StopDto s: s.PartId = partId; break;
            case Spec07_14_RingDto s: s.PartId = partId; break;
            case Spec07_15_ScrewDto s: s.PartId = partId; break;
        }
    }

    private static void Add(List<(string, string)> list, string name, object? value)
    {
        if (value == null) return;
        list.Add((name, value.ToString() ?? ""));
    }
}
