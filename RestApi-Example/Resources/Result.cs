using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi_Example.Resources
{
    public class Result
    {
        public bool Success { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public dynamic Content { get; set; }
    }
}
