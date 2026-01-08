using System.Diagnostics;

namespace CRUDSamsonovCAD.Services;

public static class NxService
{
    public static void OpenNxModel(string? nxPath)
    {
        if (string.IsNullOrWhiteSpace(nxPath)) return;
        Process.Start(new ProcessStartInfo(nxPath) { UseShellExecute = true });
    }
}
