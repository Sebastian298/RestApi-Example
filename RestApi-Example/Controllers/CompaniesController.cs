using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using EASendMail;
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

namespace RestApi_Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private static readonly string connection_string_db_local = GetSecretKey("connection-string-db-local");
        private readonly RestApi_ExampleContext _context;
        private static IConfiguration _config;
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
        [HttpPost("Validate")]
        public IActionResult ValidateCompany([FromBody] Company objCompany)
        
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO!";
            jsonRes.Description = "Authenticated";
            jsonRes.Content = new JObject();
            var CompanyID = 0;
            try
            {
                using (SqlConnection cnn = new SqlConnection(connection_string_db_local))
                using (SqlCommand cmd = new SqlCommand("API_ValidateLoginCompany", cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", objCompany.Name);
                    cmd.Parameters.AddWithValue("@Email", objCompany.Email);
                    cmd.Parameters.AddWithValue("@Password", objCompany.Password);
                    cnn.Open();
                    CompanyID = (int)cmd.ExecuteScalar();
                }
                if (CompanyID == 0)
                {
                    jsonRes.Success = false;
                    jsonRes.Title = "Error";
                    jsonRes.Description = "Company Does Not Exists!";
                    jsonRes.Content = null;
                    return StatusCode(404, jsonRes);
                }
                else
                {
                    jsonRes.Content.CompanyID = CompanyID;
                    return StatusCode(202, jsonRes);
                }
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
        [AllowAnonymous]
        [HttpPost("Create")]
        public IActionResult CreateCompany([FromBody] Company objCompany)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "Company Created";
            jsonRes.Description = "Revise su correo para obtener su CompanyID";
            jsonRes.Content = 1;
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
                        cmd.Parameters.AddWithValue("@Password", objCompany.Password);
                        cnn.Open();
                        CompanyID = (int)cmd.ExecuteScalar();
                    }
                    SendEmail(objCompany.Email, CompanyID);
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
            else
            {
                jsonRes.Success = false;
                jsonRes.Title = "Error";
                jsonRes.Description = "Company Already Exists!";
                jsonRes.Content = 0;
                return StatusCode(500, jsonRes);
            }
        }

        private static void SendEmail(string EmailTo,int CompanyID)
        {
            var email = new SmtpMail("TryIt");
            var from = _config["Credentials:Mail"];
            var pass = _config["Credentials:Pass"];
            email.From = from;
            email.To = EmailTo;
            email.Subject = "Registro de compa­ñia";
            email.TextBody = $"Su CompanyID es: {CompanyID}";
            var server = new SmtpServer("smtp.live.com");
            server.User = from;
            server.Password = pass;
            server.Port = 587;
            server.ConnectType = SmtpConnectType.ConnectSSLAuto;
            var client = new EASendMail.SmtpClient();
            client.SendMailAsync(server, email);
            return;
        }

        [AllowAnonymous]
        [HttpPost("CreateToken")]
        public IActionResult CreateToken([FromBody] Company objCompany)
        {
            dynamic jsonRes = new JObject();
            jsonRes.Success = true;
            jsonRes.Title = "LISTO!";
            jsonRes.Description = "Access Token Created";
            jsonRes.Content = new JObject();
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
                    jsonRes.Success = false;
                    jsonRes.Title = "Error";
                    jsonRes.Description = "Company Does Not Exists!";
                    jsonRes.Content = 0;
                    return StatusCode(404, jsonRes);

                }
                else if (!resultValidate.Contains("Currently"))
                {
                    var expirationAux = new DateTime();
                    var expirationToken = new DateTime();
                    var Token = GetToken(ref expirationToken);
                    expirationAux = expirationToken;
                    var expires = expirationToken.ToString("hh:mm tt");
                    dynamic jsonToken = new JObject();
                    jsonToken.Token = Token;
                    jsonToken.ExpirationTime = expires;
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
                    jsonRes.Content = jsonToken;
                    return StatusCode(201, jsonRes);
                }
                else
                {
                    jsonRes.Success = false;
                    jsonRes.Title = "Error!";
                    jsonRes.Description = "Currently Active Access Token";
                    jsonRes.Content = null;
                    return StatusCode(500, jsonRes);
                }
            }
            catch (Exception ex)
            {
                jsonRes.Success = false;
                jsonRes.Title = "Error!";
                jsonRes.Description = "Error al generar el access token. Intente de nuevo o contacte a soporte";
                jsonRes.Content = 0;
                return StatusCode(500, jsonRes);
            }
        }
        private string GetToken(ref DateTime ExpirationHour)
        {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var time = DateTime.Now.AddMinutes(50);
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
