using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi_Example.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        public string Name { get; set; }
        public string CompanyCode { get; set; }
        [Email]
        public string Email { get; set; }

        public string AccessToken { get; set; }
    }
}
