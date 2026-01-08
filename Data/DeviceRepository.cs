using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using CRUDSamsonovCAD.Models;

namespace CRUDSamsonovCAD.Data;

public sealed class DeviceRepository
{
    private readonly string _cs;
    public DeviceRepository(string connectionString) => _cs = connectionString;

    public List<DeviceDto> GetAll()
    {
        var list = new List<DeviceDto>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("SELECT Id, Name, Code, Description, NxModelPath, DrawingImage FROM Devices ORDER BY Name", cn);
        cn.Open();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new DeviceDto
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                Code = r.IsDBNull(2) ? null : r.GetString(2),
                Description = r.IsDBNull(3) ? null : r.GetString(3),
                NxModelPath = r.IsDBNull(4) ? null : r.GetString(4),
                DrawingImage = r.IsDBNull(5) ? null : (byte[])r[5]
            });
        }
        return list;
    }

    public int Insert(DeviceDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "INSERT INTO Devices(Name, Code, Description, NxModelPath, DrawingImage) VALUES (@Name,@Code,@Desc,@Nx,@Img); SELECT CAST(SCOPE_IDENTITY() AS int);",
            cn);
        cmd.Parameters.AddWithValue("@Name", dto.Name);
        cmd.Parameters.AddWithValue("@Code", (object?)dto.Code ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Desc", (object?)dto.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Nx", (object?)dto.NxModelPath ?? DBNull.Value);
        cmd.Parameters.Add("@Img", SqlDbType.VarBinary).Value = (object?)dto.DrawingImage ?? DBNull.Value;
        cn.Open();
        return (int)cmd.ExecuteScalar();
    }

    public void Update(DeviceDto dto)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "UPDATE Devices SET Name=@Name, Code=@Code, Description=@Desc, NxModelPath=@Nx, DrawingImage=@Img WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", dto.Id);
        cmd.Parameters.AddWithValue("@Name", dto.Name);
        cmd.Parameters.AddWithValue("@Code", (object?)dto.Code ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Desc", (object?)dto.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Nx", (object?)dto.NxModelPath ?? DBNull.Value);
        cmd.Parameters.Add("@Img", SqlDbType.VarBinary).Value = (object?)dto.DrawingImage ?? DBNull.Value;
        cn.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM Devices WHERE Id=@Id", cn);
        cmd.Parameters.AddWithValue("@Id", id);
        cn.Open();
        cmd.ExecuteNonQuery();
    }
}

