using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RestApi_Example.Data;
using RestApi_Example.Models;
using RestApi_Example.Resources;

namespace RestApi_Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private static readonly string connection_string_db_local = GetSecretKey("connection-string-db-local");
        private readonly RestApi_ExampleContext _context;
        private IConfiguration _config;

        public CompaniesController(RestApi_ExampleContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompany()
        {
            return await _context.Company.ToListAsync();
        }

        [AllowAnonymous]
        [HttpPost("Create")]
        public IActionResult CreateCompany([FromBody] Company objCompany)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Company Created";
            result.Content = 1;
            var CompanyID = 0;
            using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
            using (SqlCommand cmd = new SqlCommand("API_ValidateCompany", cnn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompanyName", objCompany.Name);
                cmd.Parameters.AddWithValue("@Operation", 1);
                cnn.Open();
                CompanyID = (int)cmd.ExecuteScalar();
            }
            if (CompanyID == 0)
            {
                try
                {
                    using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                    using (SqlCommand cmd = new SqlCommand("API_UpdateCompany", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", objCompany.Name);
                        cmd.Parameters.AddWithValue("@CompanyCode", objCompany.CompanyCode);
                        cmd.Parameters.AddWithValue("@Email", objCompany.Email);
                        cnn.Open();
                        cmd.ExecuteReader();
                    }
                    return StatusCode(201, result);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Title = "Error!";
                    result.Description = $"Error inesperado {ex.Message}";
                    result.Content = 0;
                    return StatusCode(500, result);
                }
            }
            else
            {
                result.Success = false;
                result.Title = "Error!";
                result.Description = "Company Already Exists!";
                result.Content = 0;
                return StatusCode(400, result);
            }
        }

        [AllowAnonymous]
        [HttpPost("CreateToken")]
        public IActionResult CreateToken([FromBody] Company objCompany)
        {
            var result = new Result();
            result.Success = true;
            result.Title = "Listo!";
            result.Description = "Token Created";
            result.Content = new JObject();
            var resultValidate = "";
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_ValidateCompany", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CompanyID", objCompany.CompanyID);
                    cmd.Parameters.AddWithValue("@Hour", DateTime.Now);
                    cnn.Open();
                    resultValidate = (String)cmd.ExecuteScalar();
                }
                if (resultValidate.Contains("Company"))
                {
                    result.Success = false;
                    result.Title = "Error!";
                    result.Description = "Company Does Not Exists";
                    result.Content = new JObject();
                    return StatusCode(404, result);

                }
                else if (!resultValidate.Contains("Currently"))
                {
                    var expirationAux = new DateTime();
                    var expirationToken = new DateTime();
                    var Token = GetToken(ref expirationToken);
                    expirationAux = expirationToken;
                    var expires = expirationToken.ToString("hh:mm tt");
                    var objToken = new List<Token>();
                    objToken.Add(new Token{ 
                       AccessToken = Token,
                       ExpiresHour = expires.ToString()
                    });
                    using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                    using (SqlCommand cmd = new SqlCommand("API_UpdateToken", cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CompanyID", objCompany.CompanyID);
                        cmd.Parameters.AddWithValue("@Token", Token.ToString());
                        cmd.Parameters.AddWithValue("@ExpirationHour", expirationAux);
                        cnn.Open();
                        cmd.ExecuteReader();
                    }
                    result.Content = objToken;
                    return StatusCode(201, result);
                }
                else
                {
                    result.Success = false;
                    result.Title = "Error!";
                    result.Description = "Currently Active Access Token";
                    result.Content = new JObject();
                    return StatusCode(400, result);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Title = "Error";
                result.Description = $"Error al generar el access token. Intente de nuevo o contacte a soporte";
                result.Content = 0;
                return StatusCode(500, result);
            }
        }

        private string GetToken(ref DateTime ExpirationHour)
        {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var time = DateTime.Now.AddMinutes(15);
                var configToken = new JwtSecurityToken(_config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    expires: time,
                    signingCredentials: credentials);
                ExpirationHour = time;
                var Token = new JwtSecurityTokenHandler().WriteToken(configToken);
               return Token;
        }
        static string GetSecretKey(string file_name) => System.IO.File.ReadAllText(@"C:\applications\.secret-keys\" + file_name + ".txt");

    }
}
