using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi_Example.Resources
{
    public class Token
    {
        public string AccessToken { get; set; }
        public string ExpiresHour { get; set; }
    }
}
