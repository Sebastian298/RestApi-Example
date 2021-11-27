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
using RestApi_Example.Resources;

namespace RestApi_Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private static readonly string connection_string_db_local = GetSecretKey("connection-string-db-local");

        private readonly RestApi_ExampleContext _context;

        public ProductsController(RestApi_ExampleContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct()
        {
            return await _context.Product.ToListAsync();
        }

        [HttpGet("GetProducts")]
        [Authorize]
        public IActionResult GetProducts()
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
                using (SqlCommand cmd = new SqlCommand("API_GetProducts", cnn))
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
                var jsonProducts = new List<Product>();
                foreach (DataRow item in dtBrands.Rows)
                {
                    jsonProducts.Add(new Product
                    {
                        ProductID = int.Parse(item["ProductID"].ToString()),
                        Name = item["Name"].ToString(),
                        Brand = int.Parse(item["Brand"].ToString()),
                        Category = int.Parse(item["Category"].ToString()),
                        Price = double.Parse(item["Price"].ToString()),
                        Sku = item["Sku"].ToString(),
                        Image = item["Image"].ToString()
                    });
                }
                result.Content = jsonProducts;
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
        [Authorize]
        public IActionResult CreateProduct(Product objProduct)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Product Created";
            result.Content = 1;
            try
            {
                var Price = double.Parse(objProduct.Price.ToString("0.00"));
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateProduct", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objProduct.Name);
                    cmd.Parameters.AddWithValue("@Brand", int.Parse(objProduct.Brand.ToString()));
                    cmd.Parameters.AddWithValue("@Category", int.Parse(objProduct.Category.ToString()));
                    cmd.Parameters.AddWithValue("@Price", Price);
                    cmd.Parameters.AddWithValue("@Sku", objProduct.Sku);
                    cmd.Parameters.AddWithValue("@Image", objProduct.Image);
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
        [Authorize]
        public IActionResult UpdateProduct(Product objProduct)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Product Updated";
            result.Content = 1;
            try
            {
                var Price = double.Parse(objProduct.Price.ToString("0.00"));
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_UpdateProduct", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProductID", int.Parse(objProduct.ProductID.ToString()));
                    cmd.Parameters.AddWithValue("@Name", objProduct.Name);
                    cmd.Parameters.AddWithValue("@Brand", int.Parse(objProduct.Brand.ToString()));
                    cmd.Parameters.AddWithValue("@Category", int.Parse(objProduct.Category.ToString()));
                    cmd.Parameters.AddWithValue("@Price", Price);
                    cmd.Parameters.AddWithValue("@Sku", objProduct.Sku);
                    cmd.Parameters.AddWithValue("@Image", objProduct.Image);
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
