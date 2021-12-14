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
    public class ProductsController : ControllerBase
    {

        private readonly RestApi_ExampleContext _context;
        private static IConfiguration _config;

        public ProductsController(RestApi_ExampleContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            return await _context.Product.ToListAsync();
        }

        [HttpGet("GetProducts/{CompanyID}")]
        [Authorize]
        public IActionResult GetProducts(string CompanyID)
        {
            dynamic jsonRes = new JObject();
            DataTable dtProducts = new DataTable();
            try
            {
                jsonRes.Success = true;
                jsonRes.Title = "COMPLETADO!";
                jsonRes.Description = "";
                jsonRes.Content = new JObject();
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_GetProducts", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", int.Parse(CompanyID));
                    cnn.Open();
                    cmd.CommandTimeout = 60;
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.HasRows)
                            dtProducts.Load(rdr);
                    }
                }
                JArray arrayProducts = new JArray();
                foreach (DataRow item in dtProducts.Rows)
                {
                    arrayProducts.Add(new JObject {
                        {"ProductID", int.Parse(item["ProductID"].ToString())},
                        {"Name", item["Name"].ToString()},
                        {"Brand", item["Brand"].ToString()},
                        {"Category", item["Category"].ToString()},
                        {"Price",  item["Price"].ToString()},
                        {"Sku", item["Sku"].ToString()},
                        {"Image", item["Image"].ToString()}
                    });

                }
                
                jsonRes.Content = arrayProducts;
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
        public IActionResult CreateProduct(Product objProduct)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO";
            jsonRes.Description = "Product Created";
            jsonRes.Content = 1;
            try
            {
                var Price = double.Parse(objProduct.Price.ToString("0.00"));
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_UpdateProduct", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objProduct.Name);
                    cmd.Parameters.AddWithValue("@Brand", int.Parse(objProduct.Brand.ToString()));
                    cmd.Parameters.AddWithValue("@Category", int.Parse(objProduct.Category.ToString()));
                    cmd.Parameters.AddWithValue("@CompanyID", int.Parse(objProduct.CompanyID.ToString()));
                    cmd.Parameters.AddWithValue("@Price", Price);
                    cmd.Parameters.AddWithValue("@Sku", objProduct.Sku);
                    cmd.Parameters.AddWithValue("@Image", objProduct.Image);
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
                jsonRes.Content = null;
                return StatusCode(500, jsonRes);
            }
        }

        [HttpPut("Update")]
        [Authorize]
        public IActionResult UpdateProduct(Product objProduct)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO";
            jsonRes.Description = "Product Updated";
            jsonRes.Content = 1;
            try
            {
                var Price = double.Parse(objProduct.Price.ToString("0.00"));
                using (SqlConnection cnn = new SqlConnection(_config["ConnectionStrings:ConnectionDB"]))
                using (SqlCommand cmd = new SqlCommand("API_UpdateProduct", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductID", int.Parse(objProduct.ProductID.ToString()));
                    cmd.Parameters.AddWithValue("@Name", objProduct.Name);
                    cmd.Parameters.AddWithValue("@Price", Price);
                    cmd.Parameters.AddWithValue("@Sku", objProduct.Sku);
                    cmd.Parameters.AddWithValue("@Image", objProduct.Image);
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
