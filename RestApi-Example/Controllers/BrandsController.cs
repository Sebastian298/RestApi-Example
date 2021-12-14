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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestApi_Example.Data;
using RestApi_Example.Models;

namespace RestApi_Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly RestApi_ExampleContext _context;
        private static IConfiguration _config;

        public BrandsController(RestApi_ExampleContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("Create")]
        [Authorize]
        public IActionResult CreateBrand(Brand objBrand)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO!";
            jsonRes.Description = "Brand Created";
            jsonRes.Content = 1;
            try
            {
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_UpdateBrand", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objBrand.Name);
                    cmd.Parameters.AddWithValue("@Image", objBrand.Image);
                    cmd.Parameters.AddWithValue("@CompanyID", objBrand.CompanyID);
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
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
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

        [HttpGet("GetBrands/{CompanyID}")]
        [Authorize]
        public IActionResult GetBrands(string CompanyID)
        {
            dynamic jsonRes = new JObject();
            DataTable dtBrands = new DataTable();
            jsonRes.Success = true;
            jsonRes.Title = "COMPLETADO!";
            jsonRes.Description = "";
            jsonRes.Content = new JObject();
            try
            {
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_GetBrands", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID",int.Parse(CompanyID));
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

        [HttpPost("GetBrand")]
        [Authorize]
        public IActionResult API_GetBrandByNames([FromBody] dynamic objBrand)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "COMPLETADO!";
            jsonRes.Description = "";
            jsonRes.Content = new JObject();
            DataTable dtBrands = new DataTable();
            try
            {
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_GetBrandByNames", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", int.Parse(objBrand.CompanyID.ToString()));
                    cmd.Parameters.AddWithValue("@Name", objBrand.Name.ToString());
                    cnn.Open();
                    cmd.CommandTimeout = 60;
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                            dtBrands.Load(rdr);
                        else
                        {
                            jsonRes.Success = false;
                            jsonRes.Title = "ERROR!";
                            jsonRes.Description = "No se encontró la marca!";
                            jsonRes.Content = null;
                            return StatusCode(200, jsonRes);
                        }
                    }
                }
                JArray jsonBrands = new JArray();
                foreach (DataRow item in dtBrands.Rows)
                {
                    jsonBrands.Add(new JObject
                    {
                        {"BrandID", int.Parse(item["BrandID"].ToString())},
                        {"Name", item["Name"].ToString() }
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
    }
}
