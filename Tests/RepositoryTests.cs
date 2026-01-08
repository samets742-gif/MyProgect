using System;
using System.Linq;
using CRUDSamsonovCAD.Data;
using CRUDSamsonovCAD.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CRUDSamsonovCAD.Tests;

[TestClass]
public sealed class RepositoryTests
{
    [TestMethod]
    public void Devices_CRUD_Works()
    {
        TestConfig.EnsureDatabaseAvailable();
        var repo = new DeviceRepository(TestConfig.ConnectionString);
        var name = "Тестовый прибор " + Guid.NewGuid().ToString("N");

        var dto = new DeviceDto
        {
            Name = name,
            Code = "T-001",
            Description = "Проверка CRUD"
        };

        var id = repo.Insert(dto);
        try
        {
            var all = repo.GetAll();
            var created = all.FirstOrDefault(d => d.Id == id);
            Assert.IsNotNull(created);
            Assert.AreEqual(name, created!.Name);

            created.Name = name + "_upd";
            repo.Update(created);

            var updated = repo.GetAll().FirstOrDefault(d => d.Id == id);
            Assert.IsNotNull(updated);
            Assert.AreEqual(name + "_upd", updated!.Name);
        }
        finally
        {
            repo.Delete(id);
        }

        var deleted = repo.GetAll().FirstOrDefault(d => d.Id == id);
        Assert.IsNull(deleted);
    }

    [TestMethod]
    public void Parts_And_Spec_CRUD_Works()
    {
        TestConfig.EnsureDatabaseAvailable();
        var deviceRepo = new DeviceRepository(TestConfig.ConnectionString);
        var partRepo = new PartRepository(TestConfig.ConnectionString);
        var specRepo = new SpecRepository(TestConfig.ConnectionString);

        var device = new DeviceDto { Name = "Тестовый прибор " + Guid.NewGuid().ToString("N") };
        device.Id = deviceRepo.Insert(device);

        var part = new PartDto
        {
            DeviceId = device.Id,
            Name = "Кронштейн тест",
            PartType = "Spec_07_02_Bracket",
            NxModelPath = "C:\\Temp\\model.prt",
            Notes = "Тест"
        };

        part.Id = partRepo.Insert(part);

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
            var loaded = (Spec07_02_BracketDto?)specRepo.GetSpec(part.PartType, part.Id);

            Assert.IsNotNull(loaded);
            Assert.AreEqual(10, loaded!.Length);
            Assert.AreEqual("Сталь", loaded.Material);

            part.Name = "Кронштейн тест upd";
            partRepo.Update(part);
            var reloaded = partRepo.GetByDevice(device.Id).FirstOrDefault(p => p.Id == part.Id);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual("Кронштейн тест upd", reloaded!.Name);
        }
        finally
        {
            specRepo.DeleteSpec(part.PartType, part.Id);
            partRepo.Delete(part.Id);
            deviceRepo.Delete(device.Id);
        }
    }
}
