using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.Data;

public sealed class PartRepository
{
    private readonly string _cs;
    public PartRepository(string connectionString) => _cs = connectionString;

    public List<PartDto> GetByDevice(int deviceId)
    {
        var list = new List<PartDto>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "SELECT Id, DeviceId, Name, PartType, NxModelPath, DrawingImage, Notes FROM Parts WHERE DeviceId=@Id ORDER BY Name",
            cn);
        cmd.Parameters.AddWithValue("@Id", deviceId);
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new PartDto
            {
                Id = r.GetInt32(0),
                DeviceId = r.GetInt32(1),
                Name = r.GetString(2),
                PartType = r.GetString(3),
                NxModelPath = r.IsDBNull(4) ? null : r.GetString(4),
                DrawingImage = r.IsDBNull(5) ? null : (byte[])r[5],
                Notes = r.IsDBNull(6) ? null : r.GetString(6)
            });
        }
        return list;
    }

    public int Insert(PartDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"INSERT INTO Parts(DeviceId,Name,PartType,NxModelPath,DrawingImage,Notes)
              VALUES(@DeviceId,@Name,@PartType,@Nx,@Img,@Notes);
              SELECT CAST(SCOPE_IDENTITY() AS int);", cn);
        cmd.Parameters.AddWithValue("@DeviceId", dto.DeviceId);
        cmd.Parameters.AddWithValue("@Name", dto.Name);
        cmd.Parameters.AddWithValue("@PartType", dto.PartType);
        cmd.Parameters.AddWithValue("@Nx", (object?)dto.NxModelPath ?? DBNull.Value);
        cmd.Parameters.Add("@Img", SqlDbType.VarBinary).Value = (object?)dto.DrawingImage ?? DBNull.Value;
        cmd.Parameters.AddWithValue("@Notes", (object?)dto.Notes ?? DBNull.Value);
        cn.Open();
        return (int)cmd.ExecuteScalar();
    }

    public void Update(PartDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            @"UPDATE Parts SET Name=@Name, PartType=@PartType, NxModelPath=@Nx, DrawingImage=@Img, Notes=@Notes WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", dto.Id);
        cmd.Parameters.AddWithValue("@Name", dto.Name);
        cmd.Parameters.AddWithValue("@PartType", dto.PartType);
        cmd.Parameters.AddWithValue("@Nx", (object?)dto.NxModelPath ?? DBNull.Value);
        cmd.Parameters.Add("@Img", SqlDbType.VarBinary).Value = (object?)dto.DrawingImage ?? DBNull.Value;
        cmd.Parameters.AddWithValue("@Notes", (object?)dto.Notes ?? DBNull.Value);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM Parts WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public List<int> GetDeviceIdsByPartSearch(string text, IReadOnlyList<string> partTypes)
    {
        var list = new List<int>();
        var likeText = "%" + text + "%";

        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand();
        cmd.Connection = cn;

        var sql = "SELECT DISTINCT DeviceId FROM Parts WHERE Name LIKE @Text OR PartType LIKE @Text";
        cmd.Parameters.AddWithValue("@Text", likeText);

        if (partTypes.Count > 0)
        {
            var inParams = new List<string>();
            for (var i = 0; i < partTypes.Count; i++)
            {
                var name = "@P" + i;
                inParams.Add(name);
                cmd.Parameters.AddWithValue(name, partTypes[i]);
            }
            sql += " OR PartType IN (" + string.Join(",", inParams) + ")";
        }

        cmd.CommandText = sql;
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(r.GetInt32(0));
        }
        return list;
    }
}
