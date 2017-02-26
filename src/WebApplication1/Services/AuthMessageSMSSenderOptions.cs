using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class AuthMessageSMSSenderOptions
    {
        public string SID { get; set; }
        public string AuthToken { get; set; }
        public string SendNumber { get; set; }
    }
}
