using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RestApi_Example.Data;
using RestApi_Example.Models;

namespace RestApi_Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RestApi_ExampleContext _context;
        private static readonly string connection_string_db_local = GetSecretKey("connection-string-db-local");
        public CategoriesController(RestApi_ExampleContext context)
        {
            _context = context;
        }

        [HttpGet("GetCategorys/{CompanyID}")]
        [Authorize]
        public IActionResult GetCategorys(string CompanyID)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "COMPLETADO!";
            jsonRes.Description = "";
            jsonRes.Content = new JObject();
            DataTable dtCategorys = new DataTable();
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_GetCategorys", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", int.Parse(CompanyID));
                    cnn.Open();
                    cmd.CommandTimeout = 60;
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                            dtCategorys.Load(rdr);
                    }
                }
                JArray jsonCategorys = new JArray();
                foreach (DataRow item in dtCategorys.Rows)
                {
                    jsonCategorys.Add(new JObject
                    {
                        {"CategoryID", int.Parse(item["CategoryID"].ToString())},
                        {"Name", item["Name"].ToString() },
                        {"Image", item["Image"].ToString()}
                    });
                }
                jsonRes.Content = jsonCategorys;
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
        
        [HttpPost("Create")]
        [Authorize]
        public IActionResult CreateCategory(Category objCategory)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO";
            jsonRes.Description = "Category Created";
            jsonRes.Content = 1;
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateCategory", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objCategory.Name);
                    cmd.Parameters.AddWithValue("@Image", objCategory.Image);
                    cmd.Parameters.AddWithValue("@CompanyID", objCategory.CompanyID);
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
        public IActionResult UpdateCategory(Category objCategory)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO";
            jsonRes.Description = "Category Updated";
            jsonRes.Content = 1;
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateCategory", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CategoryID", int.Parse(objCategory.CategoryID.ToString()));
                    cmd.Parameters.AddWithValue("@Name", objCategory.Name);
                    cmd.Parameters.AddWithValue("@Image", objCategory.Image);
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
        static string GetSecretKey(string file_name) => System.IO.File.ReadAllText(@"C:\applications\.secret-keys\" + file_name + ".txt");
    }
}
