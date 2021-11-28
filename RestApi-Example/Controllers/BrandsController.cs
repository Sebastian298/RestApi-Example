using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public IActionResult CreateBrand(Brand objBrand)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO";
            jsonRes.Description = "Product Updated";
            jsonRes.Content = 1;
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
                return StatusCode(201, jsonRes);
            }
            catch (Exception ex)
            {
                jsonRes.Success = false;
                jsonRes.Title = "Error";
                jsonRes.Description = $"Error inesperado {ex.Message}";
                jsonRes.Content = 0;
                return StatusCode(500, jsonRes);
            }
        }

        [HttpPut("Update")]
        [Authorize]
        public IActionResult UpdateBrand(Brand objBrand)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO!";
            jsonRes.Description = "Brand Updated";
            jsonRes.Content = 1;
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
                return StatusCode(201, jsonRes);
            }
            catch (Exception ex)
            {
                jsonRes.Success = false;
                jsonRes.Title = "Error";
                jsonRes.Description = $"Error inesperado {ex.Message}";
                jsonRes.Content = 0;
                return StatusCode(500, jsonRes);
            }
        }

        [HttpGet("GetBrands")]
        [Authorize]
        public IActionResult GetBrands()
        {
            dynamic jsonRes = new JObject();
            DataTable dtBrands = new DataTable();
            jsonRes.Success = true;
            jsonRes.Title = "COMPLETADO!";
            jsonRes.Description = "";
            jsonRes.Content = new JObject();
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
                JArray jsonBrands = new JArray();
                foreach (DataRow item in dtBrands.Rows)
                {
                    jsonBrands.Add(new JObject
                    {
                        {"BrandID",int.Parse(item["BrandID"].ToString())},
                        {"Name",item["Name"].ToString()},
                        {"Image",item["Image"].ToString()}
                    });
                }
                jsonRes.Content = jsonBrands;
                return StatusCode(200, jsonRes);
            }
            catch (Exception ex)
            {
                jsonRes.Success = false;
                jsonRes.Title = "Error";
                jsonRes.Description = $"Error inesperado {ex.Message}";
                jsonRes.Content = null;
                return StatusCode(500, jsonRes);
            }
        }
        static string GetSecretKey(string file_name) => System.IO.File.ReadAllText(@"C:\applications\.secret-keys\" + file_name + ".txt");
    }
}
