using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi_Example.Models
{
    public class Brand
    {
        public int BrandID { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int CompanyID { get; set; }
    }
}
