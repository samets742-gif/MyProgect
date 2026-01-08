using System.Collections.Generic;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.Services;

public static class PartTypeProvider
{
    public static readonly IReadOnlyList<PartTypeItem> Items = new List<PartTypeItem>
    {
        new("Spec_07_02_Bracket", "07.02 Кронштейн"),
        new("Spec_07_05_Roller", "07.05 Ролик"),
        new("Spec_07_06_Frame", "07.06 Рама"),
        new("Spec_07_07_Roller", "07.07 Ролик"),
        new("Spec_07_08_Adapter", "07.08 Адаптер"),
        new("Spec_07_10_Spring", "07.10 Пружина"),
        new("Spec_07_11_Stop", "07.11 Упор"),
        new("Spec_07_14_Ring", "07.14 Кольцо"),
        new("Spec_07_15_Screw", "07.15 Винт")
    };

    public static string GetDisplayName(string partType)
    {
        foreach (var item in Items)
        {
            if (item.Key == partType)
                return item.Name;
        }
        return partType;
    }

    public static bool IsCustom(string partType, out int typeId)
    {
        typeId = 0;
        if (!partType.StartsWith("Custom:"))
            return false;
        var suffix = partType.Substring("Custom:".Length);
        return int.TryParse(suffix, out typeId);
    }

    public static string GetCustomKey(int typeId) => $"Custom:{typeId}";
}
