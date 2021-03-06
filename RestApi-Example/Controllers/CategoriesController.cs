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
using Microsoft.Extensions.Configuration;
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
        private static IConfiguration _config;

        public CategoriesController(RestApi_ExampleContext context, IConfiguration config)
        {
            _config = config;
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
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
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

        [HttpPost("GetCategory")]
        [Authorize]
        public IActionResult GetCategorysByNames([FromBody] dynamic objCategory)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "COMPLETADO!";
            jsonRes.Description = "";
            jsonRes.Content = new JObject();
            DataTable dtCategorys = new DataTable();
            try
            {
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_GetCategoryByNames", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", int.Parse(objCategory.CompanyID.ToString()));
                    cmd.Parameters.AddWithValue("@Name", objCategory.Name.ToString());
                    cnn.Open();
                    cmd.CommandTimeout = 60;
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                            dtCategorys.Load(rdr);
                        else
                        {
                            jsonRes.Success = false;
                            jsonRes.Title = "ERROR!";
                            jsonRes.Description = "No se encontró la categoria!";
                            jsonRes.Content = null;
                            return StatusCode(200, jsonRes);
                        }
                    }
                }
                JArray jsonCategorys = new JArray();
                foreach (DataRow item in dtCategorys.Rows)
                {
                    jsonCategorys.Add(new JObject
                    {
                        {"CategoryID", int.Parse(item["CategoryID"].ToString())},
                        {"Name", item["Name"].ToString() }
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
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
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
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
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
    }
}
