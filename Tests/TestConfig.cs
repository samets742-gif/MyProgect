using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CRUDSamsonovCAD.Tests;

internal static class TestConfig
{
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("CAD_TEST_CS")
        ?? "Server=localhost\\SQLEXPRESS;Database=CADDevicesDb;Trusted_Connection=True;Encrypt=False;";

    public static void EnsureDatabaseAvailable()
    {
        try
        {
            using var cn = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
            cn.Open();
        }
        catch (Exception ex)
        {
            Assert.Inconclusive("База данных недоступна: " + ex.Message);
        }
    }
}
