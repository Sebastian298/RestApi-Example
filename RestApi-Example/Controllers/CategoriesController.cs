using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using RestApi_Example.Data;
using RestApi_Example.Models;
using RestApi_Example.Resources;

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

        [HttpGet("GetCategorys")]
        public IActionResult GetCategorys()
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "";
            result.Content = new JObject();
            DataTable dtCategorys = new DataTable();
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_GetCategorys", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cnn.Open();
                    cmd.CommandTimeout = 60;
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                            dtCategorys.Load(rdr);
                    }
                }
                var jsonCategorys = new List<Category>();
                foreach (DataRow item in dtCategorys.Rows)
                {
                    jsonCategorys.Add(new Category
                    {
                        CategoryID = int.Parse(item["CategoryID"].ToString()),
                        Name = item["Name"].ToString(),
                        Image = item["Image"].ToString()
                    });
                }
                result.Content = jsonCategorys;
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
        [HttpPost("Create")]
        public IActionResult CreateCategory(Category objCategory)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Category Created";
            result.Content = 1;
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateCategory", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objCategory.Name);
                    cmd.Parameters.AddWithValue("@Image", objCategory.Image);
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
        public IActionResult UpdateCategory(Category objCategory)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Category Updated";
            result.Content = 1;
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
        static string GetSecretKey(string file_name) => System.IO.File.ReadAllText(@"C:\applications\.secret-keys\" + file_name + ".txt");
    }
}
