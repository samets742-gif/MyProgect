using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.Data;

public sealed class SpecRepository
{
    private readonly string _cs;
    public SpecRepository(string connectionString) => _cs = connectionString;

    public object? GetSpec(string partType, int partId)
    {
        return partType switch
        {
            "Spec_07_02_Bracket" => GetSpec07_02_Bracket(partId),
            "Spec_07_05_Roller" => GetSpec07_05_Roller(partId),
            "Spec_07_06_Frame" => GetSpec07_06_Frame(partId),
            "Spec_07_07_Roller" => GetSpec07_07_Roller(partId),
            "Spec_07_08_Adapter" => GetSpec07_08_Adapter(partId),
            "Spec_07_10_Spring" => GetSpec07_10_Spring(partId),
            "Spec_07_11_Stop" => GetSpec07_11_Stop(partId),
            "Spec_07_14_Ring" => GetSpec07_14_Ring(partId),
            "Spec_07_15_Screw" => GetSpec07_15_Screw(partId),
            _ => null
        };
    }

    public object CreateEmptySpec(string partType, int partId)
    {
        return partType switch
        {
            "Spec_07_02_Bracket" => new Spec07_02_BracketDto { PartId = partId },
            "Spec_07_05_Roller" => new Spec07_05_RollerDto { PartId = partId },
            "Spec_07_06_Frame" => new Spec07_06_FrameDto { PartId = partId },
            "Spec_07_07_Roller" => new Spec07_07_RollerDto { PartId = partId },
            "Spec_07_08_Adapter" => new Spec07_08_AdapterDto { PartId = partId },
            "Spec_07_10_Spring" => new Spec07_10_SpringDto { PartId = partId },
            "Spec_07_11_Stop" => new Spec07_11_StopDto { PartId = partId },
            "Spec_07_14_Ring" => new Spec07_14_RingDto { PartId = partId },
            "Spec_07_15_Screw" => new Spec07_15_ScrewDto { PartId = partId },
            _ => new Spec07_02_BracketDto { PartId = partId }
        };
    }

    public void UpsertSpec(string partType, object spec)
    {
        switch (partType)
        {
            case "Spec_07_02_Bracket": UpsertSpec07_02_Bracket((Spec07_02_BracketDto)spec); break;
            case "Spec_07_05_Roller": UpsertSpec07_05_Roller((Spec07_05_RollerDto)spec); break;
            case "Spec_07_06_Frame": UpsertSpec07_06_Frame((Spec07_06_FrameDto)spec); break;
            case "Spec_07_07_Roller": UpsertSpec07_07_Roller((Spec07_07_RollerDto)spec); break;
            case "Spec_07_08_Adapter": UpsertSpec07_08_Adapter((Spec07_08_AdapterDto)spec); break;
            case "Spec_07_10_Spring": UpsertSpec07_10_Spring((Spec07_10_SpringDto)spec); break;
            case "Spec_07_11_Stop": UpsertSpec07_11_Stop((Spec07_11_StopDto)spec); break;
            case "Spec_07_14_Ring": UpsertSpec07_14_Ring((Spec07_14_RingDto)spec); break;
            case "Spec_07_15_Screw": UpsertSpec07_15_Screw((Spec07_15_ScrewDto)spec); break;
        }
    }

    public void DeleteSpec(string partType, int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand($"DELETE FROM {partType} WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_02_BracketDto? GetSpec07_02_Bracket(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Length, Width, Thickness, Material FROM Spec_07_02_Bracket WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_02_BracketDto
        {
            PartId = r.GetInt32(0),
            Length = r.IsDBNull(1) ? null : r.GetDecimal(1),
            Width = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Thickness = r.IsDBNull(3) ? null : r.GetDecimal(3),
            Material = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }

    private void UpsertSpec07_02_Bracket(Spec07_02_BracketDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_02_Bracket WHERE PartId=@Id)
                UPDATE Spec_07_02_Bracket SET Length=@L, Width=@W, Thickness=@T, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_02_Bracket(PartId, Length, Width, Thickness, Material)
                VALUES(@Id,@L,@W,@T,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@L", (object?)dto.Length ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@W", (object?)dto.Width ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@T", (object?)dto.Thickness ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_05_RollerDto? GetSpec07_05_Roller(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Diameter, Width, Material FROM Spec_07_05_Roller WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_05_RollerDto
        {
            PartId = r.GetInt32(0),
            Diameter = r.IsDBNull(1) ? null : r.GetDecimal(1),
            Width = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Material = r.IsDBNull(3) ? null : r.GetString(3)
        };
    }

    private void UpsertSpec07_05_Roller(Spec07_05_RollerDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_05_Roller WHERE PartId=@Id)
                UPDATE Spec_07_05_Roller SET Diameter=@D, Width=@W, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_05_Roller(PartId, Diameter, Width, Material)
                VALUES(@Id,@D,@W,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@D", (object?)dto.Diameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@W", (object?)dto.Width ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_06_FrameDto? GetSpec07_06_Frame(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Length, Width, Height, Material FROM Spec_07_06_Frame WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_06_FrameDto
        {
            PartId = r.GetInt32(0),
            Length = r.IsDBNull(1) ? null : r.GetDecimal(1),
            Width = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Height = r.IsDBNull(3) ? null : r.GetDecimal(3),
            Material = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }

    private void UpsertSpec07_06_Frame(Spec07_06_FrameDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_06_Frame WHERE PartId=@Id)
                UPDATE Spec_07_06_Frame SET Length=@L, Width=@W, Height=@H, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_06_Frame(PartId, Length, Width, Height, Material)
                VALUES(@Id,@L,@W,@H,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@L", (object?)dto.Length ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@W", (object?)dto.Width ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@H", (object?)dto.Height ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_07_RollerDto? GetSpec07_07_Roller(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Diameter, Width, Material FROM Spec_07_07_Roller WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_07_RollerDto
        {
            PartId = r.GetInt32(0),
            Diameter = r.IsDBNull(1) ? null : r.GetDecimal(1),
            Width = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Material = r.IsDBNull(3) ? null : r.GetString(3)
        };
    }

    private void UpsertSpec07_07_Roller(Spec07_07_RollerDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_07_Roller WHERE PartId=@Id)
                UPDATE Spec_07_07_Roller SET Diameter=@D, Width=@W, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_07_Roller(PartId, Diameter, Width, Material)
                VALUES(@Id,@D,@W,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@D", (object?)dto.Diameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@W", (object?)dto.Width ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_08_AdapterDto? GetSpec07_08_Adapter(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Length, OuterDiameter, InnerDiameter, Material FROM Spec_07_08_Adapter WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_08_AdapterDto
        {
            PartId = r.GetInt32(0),
            Length = r.IsDBNull(1) ? null : r.GetDecimal(1),
            OuterDiameter = r.IsDBNull(2) ? null : r.GetDecimal(2),
            InnerDiameter = r.IsDBNull(3) ? null : r.GetDecimal(3),
            Material = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }

    private void UpsertSpec07_08_Adapter(Spec07_08_AdapterDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_08_Adapter WHERE PartId=@PartId)
                UPDATE Spec_07_08_Adapter SET Length=@L, OuterDiameter=@OD, InnerDiameter=@InnerD, Material=@M WHERE PartId=@PartId
              ELSE
                INSERT INTO Spec_07_08_Adapter(PartId, Length, OuterDiameter, InnerDiameter, Material)
                VALUES(@PartId,@L,@OD,@InnerD,@M)", cn);
        cmd.Parameters.AddWithValue("@PartId", dto.PartId);
        cmd.Parameters.AddWithValue("@L", (object?)dto.Length ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OD", (object?)dto.OuterDiameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@InnerD", (object?)dto.InnerDiameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_10_SpringDto? GetSpec07_10_Spring(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, WireDiameter, OuterDiameter, FreeLength, CoilsCount, Material FROM Spec_07_10_Spring WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_10_SpringDto
        {
            PartId = r.GetInt32(0),
            WireDiameter = r.IsDBNull(1) ? null : r.GetDecimal(1),
            OuterDiameter = r.IsDBNull(2) ? null : r.GetDecimal(2),
            FreeLength = r.IsDBNull(3) ? null : r.GetDecimal(3),
            CoilsCount = r.IsDBNull(4) ? null : r.GetInt32(4),
            Material = r.IsDBNull(5) ? null : r.GetString(5)
        };
    }

    private void UpsertSpec07_10_Spring(Spec07_10_SpringDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_10_Spring WHERE PartId=@Id)
                UPDATE Spec_07_10_Spring SET WireDiameter=@WD, OuterDiameter=@OD, FreeLength=@FL, CoilsCount=@CC, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_10_Spring(PartId, WireDiameter, OuterDiameter, FreeLength, CoilsCount, Material)
                VALUES(@Id,@WD,@OD,@FL,@CC,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@WD", (object?)dto.WireDiameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OD", (object?)dto.OuterDiameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@FL", (object?)dto.FreeLength ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CC", (object?)dto.CoilsCount ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_11_StopDto? GetSpec07_11_Stop(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Length, Width, Height, Material FROM Spec_07_11_Stop WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_11_StopDto
        {
            PartId = r.GetInt32(0),
            Length = r.IsDBNull(1) ? null : r.GetDecimal(1),
            Width = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Height = r.IsDBNull(3) ? null : r.GetDecimal(3),
            Material = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }

    private void UpsertSpec07_11_Stop(Spec07_11_StopDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_11_Stop WHERE PartId=@Id)
                UPDATE Spec_07_11_Stop SET Length=@L, Width=@W, Height=@H, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_11_Stop(PartId, Length, Width, Height, Material)
                VALUES(@Id,@L,@W,@H,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@L", (object?)dto.Length ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@W", (object?)dto.Width ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@H", (object?)dto.Height ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_14_RingDto? GetSpec07_14_Ring(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, OuterDiameter, InnerDiameter, Thickness, Material FROM Spec_07_14_Ring WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_14_RingDto
        {
            PartId = r.GetInt32(0),
            OuterDiameter = r.IsDBNull(1) ? null : r.GetDecimal(1),
            InnerDiameter = r.IsDBNull(2) ? null : r.GetDecimal(2),
            Thickness = r.IsDBNull(3) ? null : r.GetDecimal(3),
            Material = r.IsDBNull(4) ? null : r.GetString(4)
        };
    }

    private void UpsertSpec07_14_Ring(Spec07_14_RingDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_14_Ring WHERE PartId=@PartId)
                UPDATE Spec_07_14_Ring SET OuterDiameter=@OD, InnerDiameter=@InnerD, Thickness=@T, Material=@M WHERE PartId=@PartId
              ELSE
                INSERT INTO Spec_07_14_Ring(PartId, OuterDiameter, InnerDiameter, Thickness, Material)
                VALUES(@PartId,@OD,@InnerD,@T,@M)", cn);
        cmd.Parameters.AddWithValue("@PartId", dto.PartId);
        cmd.Parameters.AddWithValue("@OD", (object?)dto.OuterDiameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@InnerD", (object?)dto.InnerDiameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@T", (object?)dto.Thickness ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    private Spec07_15_ScrewDto? GetSpec07_15_Screw(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT PartId, Diameter, Length, ThreadPitch, HeadType, Material FROM Spec_07_15_Screw WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return new Spec07_15_ScrewDto
        {
            PartId = r.GetInt32(0),
            Diameter = r.IsDBNull(1) ? null : r.GetDecimal(1),
            Length = r.IsDBNull(2) ? null : r.GetDecimal(2),
            ThreadPitch = r.IsDBNull(3) ? null : r.GetDecimal(3),
            HeadType = r.IsDBNull(4) ? null : r.GetString(4),
            Material = r.IsDBNull(5) ? null : r.GetString(5)
        };
    }

    private void UpsertSpec07_15_Screw(Spec07_15_ScrewDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"IF EXISTS (SELECT 1 FROM Spec_07_15_Screw WHERE PartId=@Id)
                UPDATE Spec_07_15_Screw SET Diameter=@D, Length=@L, ThreadPitch=@TP, HeadType=@HT, Material=@M WHERE PartId=@Id
              ELSE
                INSERT INTO Spec_07_15_Screw(PartId, Diameter, Length, ThreadPitch, HeadType, Material)
                VALUES(@Id,@D,@L,@TP,@HT,@M)", cn);
        cmd.Parameters.AddWithValue("@Id", dto.PartId);
        cmd.Parameters.AddWithValue("@D", (object?)dto.Diameter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@L", (object?)dto.Length ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TP", (object?)dto.ThreadPitch ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HT", (object?)dto.HeadType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@M", (object?)dto.Material ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }
}
