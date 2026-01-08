using System;
using System.IO;
using System.Linq;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CRUDSamsonovCAD.Tests;

[TestClass]
public sealed class ImportServiceTests
{
    [TestMethod]
    public void Import_Xml_Roundtrip_Creates_Parts()
    {
        TestConfig.EnsureDatabaseAvailable();
        var specRepo = new SpecRepository(TestConfig.ConnectionString);
        var customRepo = new CustomPartRepository(TestConfig.ConnectionString);
        var export = new ExportService(specRepo, customRepo);
        var import = new ImportService(specRepo, customRepo);

        var device = new DeviceDto { Name = "Импорт XML " + Guid.NewGuid().ToString("N") };
        var part = new PartDto
        {
            Name = "Ролик импорт",
            PartType = "Spec_07_05_Roller"
        };

        var xmlPath = Path.Combine(Path.GetTempPath(), $"cad_import_{Guid.NewGuid():N}.xml");

        try
        {
            export.ExportToXml(xmlPath, device, new[] { part });
            var (importedDevice, importedParts) = import.ImportFromXml(xmlPath);

            Assert.IsFalse(string.IsNullOrWhiteSpace(importedDevice.Name));
            Assert.AreEqual(1, importedParts.Count);
            Assert.AreEqual("Spec_07_05_Roller", importedParts[0].Part.PartType);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
        }
    }

    [TestMethod]
    public void Custom_Types_Crud_Works()
    {
        TestConfig.EnsureDatabaseAvailable();
        var repo = new CustomPartRepository(TestConfig.ConnectionString);

        var type = new CustomPartTypeDto { Name = "Тип " + Guid.NewGuid().ToString("N") };
        type.Id = repo.InsertType(type);

        try
        {
            var param = new CustomPartParamDto
            {
                TypeId = type.Id,
                ParamName = "Параметр1",
                ParamKind = "Text",
                SortOrder = 1
            };
            param.Id = repo.InsertParam(param);

            var types = repo.GetTypes();
            Assert.IsTrue(types.Any(t => t.Id == type.Id));

            var paramsList = repo.GetParams(type.Id);
            Assert.AreEqual(1, paramsList.Count);
        }
        finally
        {
            repo.DeleteType(type.Id);
        }
    }
}
