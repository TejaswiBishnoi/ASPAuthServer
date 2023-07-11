using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityProvider
{
    public class GoogleIdentityProvider: IIdentityProvider
    {
        public string Name { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        RestClient Client { get; set; }
        
    }
}
