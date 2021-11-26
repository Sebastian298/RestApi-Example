using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestApi_Example.Data;
using RestApi_Example.Models;
using RestApi_Example.Resources;

namespace RestApi_Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private static readonly string connection_string_db_local = GetSecretKey("connection-string-db-local");
        private readonly RestApi_ExampleContext _context;

        public BrandsController(RestApi_ExampleContext context)
        {
            _context = context;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateBrand(Brand objBrand)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Brand Created";
            result.Content = 1;
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateBrand", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objBrand.Name);
                    cmd.Parameters.AddWithValue("@Image", objBrand.Image);
                    cnn.Open();
                    cmd.ExecuteReader();
                }
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Title = "Error";
                result.Description = $"Error inesperado {ex.Message}";
                result.Content = 0;
                return StatusCode(500, result);
            }
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateBrand(Brand objBrand)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Brand Updated";
            result.Content = 1;
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateBrand", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BrandID", objBrand.BrandID);
                    cmd.Parameters.AddWithValue("@Name", objBrand.Name);
                    cmd.Parameters.AddWithValue("@Image", objBrand.Image);
                    cnn.Open();
                    cmd.ExecuteReader();
                }
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Title = "Error";
                result.Description = $"Error inesperado {ex.Message}";
                result.Content = 0;
                return StatusCode(500, result);
            }
        }

        [HttpGet("GetBrands")]
        public IActionResult GetBrands()
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "";
            result.Content = new JObject();
            DataTable dtBrands = new DataTable();
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_GetBrands", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cnn.Open();
                    cmd.CommandTimeout = 60;
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                            dtBrands.Load(rdr);
                    }
                }
                var jsonBrands = new List<Brand>();
                foreach (DataRow item in dtBrands.Rows)
                {
                    jsonBrands.Add(new Brand
                    {
                        BrandID = int.Parse(item["BrandID"].ToString()),
                        Name = item["Name"].ToString(),
                        Image = item["Image"].ToString()
                    });
                }
                result.Content = jsonBrands;
                return StatusCode(200, result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Title = "Error";
                result.Description = $"Error inesperado {ex.Message}";
                result.Content = null;
                return StatusCode(500, result);
            }
        }
        static string GetSecretKey(string file_name) => System.IO.File.ReadAllText(@"C:\applications\.secret-keys\" + file_name + ".txt");
    }
}
