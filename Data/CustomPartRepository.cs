using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.Data;

public sealed class CustomPartRepository
{
    private readonly string _cs;
    public CustomPartRepository(string connectionString) => _cs = connectionString;

    public List<CustomPartTypeDto> GetTypes()
    {
        var list = new List<CustomPartTypeDto>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("SELECT Id, Name FROM CustomPartTypes ORDER BY Name", cn);
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new CustomPartTypeDto
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1)
            });
        }
        return list;
    }

    public int InsertType(CustomPartTypeDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("INSERT INTO CustomPartTypes(Name) VALUES(@Name); SELECT CAST(SCOPE_IDENTITY() AS int);", cn);
        cmd.Parameters.AddWithValue("@Name", dto.Name);
        cn.Open();
        return (int)cmd.ExecuteScalar();
    }

    public void UpdateType(CustomPartTypeDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("UPDATE CustomPartTypes SET Name=@Name WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", dto.Id);
        cmd.Parameters.AddWithValue("@Name", dto.Name);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public void DeleteType(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM CustomPartTypes WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<CustomPartParamDto> GetParams(int typeId)
    {
        var list = new List<CustomPartParamDto>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT Id, TypeId, ParamName, ParamKind, ListValues, SortOrder FROM CustomPartParams WHERE TypeId=@Id ORDER BY SortOrder, ParamName",
            cn);
        cmd.Parameters.AddWithValue("@Id", typeId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new CustomPartParamDto
            {
                Id = r.GetInt32(0),
                TypeId = r.GetInt32(1),
                ParamName = r.GetString(2),
                ParamKind = r.GetString(3),
                ListValues = r.IsDBNull(4) ? null : r.GetString(4),
                SortOrder = r.IsDBNull(5) ? 0 : r.GetInt32(5)
            });
        }
        return list;
    }

    public int InsertParam(CustomPartParamDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "INSERT INTO CustomPartParams(TypeId, ParamName, ParamKind, ListValues, SortOrder) VALUES(@TypeId,@Name,@Kind,@List,@Sort); SELECT CAST(SCOPE_IDENTITY() AS int);",
            cn);
        cmd.Parameters.AddWithValue("@TypeId", dto.TypeId);
        cmd.Parameters.AddWithValue("@Name", dto.ParamName);
        cmd.Parameters.AddWithValue("@Kind", dto.ParamKind);
        cmd.Parameters.AddWithValue("@List", (object?)dto.ListValues ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Sort", dto.SortOrder);
        cn.Open();
        return (int)cmd.ExecuteScalar();
    }

    public void UpdateParam(CustomPartParamDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "UPDATE CustomPartParams SET ParamName=@Name, ParamKind=@Kind, ListValues=@List, SortOrder=@Sort WHERE Id=@Id",
            cn);
        cmd.Parameters.AddWithValue("@Id", dto.Id);
        cmd.Parameters.AddWithValue("@Name", dto.ParamName);
        cmd.Parameters.AddWithValue("@Kind", dto.ParamKind);
        cmd.Parameters.AddWithValue("@List", (object?)dto.ListValues ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Sort", dto.SortOrder);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public void DeleteParam(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM CustomPartParams WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public Dictionary<int, string?> GetValues(int partId)
    {
        var map = new Dictionary<int, string?>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT ParamId, ValueText FROM CustomPartValues WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            map[r.GetInt32(0)] = r.IsDBNull(1) ? null : r.GetString(1);
        }
        return map;
    }

    public void UpsertValues(int partId, IEnumerable<CustomPartValueDto> values)
    {
        using var cn = new SqlConnection(_cs);
        cn.Open();
        using var tx = cn.BeginTransaction();

        using (var del = new SqlCommand("DELETE FROM CustomPartValues WHERE PartId=@Id", cn, tx))
        {
            del.Parameters.AddWithValue("@Id", partId);
            del.ExecuteNonQuery();
        }

        foreach (var v in values)
        {
            using var ins = new SqlCommand(
                "INSERT INTO CustomPartValues(PartId, ParamId, ValueText) VALUES(@PartId,@ParamId,@Value)",
                cn, tx);
            ins.Parameters.AddWithValue("@PartId", partId);
            ins.Parameters.AddWithValue("@ParamId", v.ParamId);
            ins.Parameters.AddWithValue("@Value", (object?)v.ValueText ?? DBNull.Value);
            ins.ExecuteNonQuery();
        }

        tx.Commit();
    }

    public void DeleteValues(int partId)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM CustomPartValues WHERE PartId=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<(string Name, string Value)> GetFields(int partId)
    {
        var list = new List<(string, string)>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"SELECT p.ParamName, v.ValueText
              FROM CustomPartValues v
              JOIN CustomPartParams p ON p.Id = v.ParamId
              WHERE v.PartId=@Id
              ORDER BY p.SortOrder, p.ParamName", cn);
        cmd.Parameters.AddWithValue("@Id", partId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var name = r.GetString(0);
            var value = r.IsDBNull(1) ? "" : r.GetString(1);
            list.Add((name, value));
        }
        return list;
    }
}
