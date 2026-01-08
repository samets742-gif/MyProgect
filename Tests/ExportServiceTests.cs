using System;
using System.IO;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using CRUDSamsonovCAD.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CRUDSamsonovCAD.Tests;

[TestClass]
public sealed class ExportServiceTests
{
    [TestMethod]
    public void Export_Word_And_Excel_Creates_Files()
    {
        TestConfig.EnsureDatabaseAvailable();
        var deviceRepo = new DeviceRepository(TestConfig.ConnectionString);
        var partRepo = new PartRepository(TestConfig.ConnectionString);
        var specRepo = new SpecRepository(TestConfig.ConnectionString);
        var customRepo = new CustomPartRepository(TestConfig.ConnectionString);
        var export = new ExportService(specRepo, customRepo);

        var device = new DeviceDto { Name = "Экспорт прибор " + Guid.NewGuid().ToString("N") };
        device.Id = deviceRepo.Insert(device);

        var part = new PartDto
        {
            DeviceId = device.Id,
            Name = "Ролик экспорт",
            PartType = "Spec_07_05_Roller",
            Notes = "Экспорт"
        };

        part.Id = partRepo.Insert(part);

        var wordPath = Path.Combine(Path.GetTempPath(), $"cad_export_{Guid.NewGuid():N}.docx");
        var excelPath = Path.Combine(Path.GetTempPath(), $"cad_export_{Guid.NewGuid():N}.xlsx");

        try
        {
            var spec = new Spec07_05_RollerDto
            {
                PartId = part.Id,
                Diameter = 12,
                Width = 5,
                Material = "Сталь"
            };
            specRepo.UpsertSpec(part.PartType, spec);

            export.ExportToWord(wordPath, device, new[] { part }, includeDeviceDrawing: true);
            export.ExportToExcel(excelPath, device, new[] { part });

            Assert.IsTrue(File.Exists(wordPath));
            Assert.IsTrue(File.Exists(excelPath));
            Assert.IsTrue(new FileInfo(wordPath).Length > 0);
            Assert.IsTrue(new FileInfo(excelPath).Length > 0);
        }
        finally
        {
            if (File.Exists(wordPath)) File.Delete(wordPath);
            if (File.Exists(excelPath)) File.Delete(excelPath);
            specRepo.DeleteSpec(part.PartType, part.Id);
            partRepo.Delete(part.Id);
            deviceRepo.Delete(device.Id);
        }
    }

    [TestMethod]
    public void Export_Xml_Creates_File()
    {
        TestConfig.EnsureDatabaseAvailable();
        var deviceRepo = new DeviceRepository(TestConfig.ConnectionString);
        var partRepo = new PartRepository(TestConfig.ConnectionString);
        var specRepo = new SpecRepository(TestConfig.ConnectionString);
        var customRepo = new CustomPartRepository(TestConfig.ConnectionString);
        var export = new ExportService(specRepo, customRepo);

        var device = new DeviceDto { Name = "Экспорт XML " + Guid.NewGuid().ToString("N") };
        device.Id = deviceRepo.Insert(device);

        var part = new PartDto
        {
            DeviceId = device.Id,
            Name = "Кронштейн экспорт",
            PartType = "Spec_07_02_Bracket"
        };
        part.Id = partRepo.Insert(part);

        var xmlPath = Path.Combine(Path.GetTempPath(), $"cad_export_{Guid.NewGuid():N}.xml");

        try
        {
            var spec = new Spec07_02_BracketDto
            {
                PartId = part.Id,
                Length = 10,
                Width = 20,
                Thickness = 2,
                Material = "Сталь"
            };
            specRepo.UpsertSpec(part.PartType, spec);

            export.ExportToXml(xmlPath, device, new[] { part });
            Assert.IsTrue(File.Exists(xmlPath));
            Assert.IsTrue(new FileInfo(xmlPath).Length > 0);
        }
        finally
        {
            if (File.Exists(xmlPath)) File.Delete(xmlPath);
            specRepo.DeleteSpec(part.PartType, part.Id);
            partRepo.Delete(part.Id);
            deviceRepo.Delete(device.Id);
        }
    }
}
