using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityProvider
{
    /// <summary>
    /// Interface for collection of Identity Providers. 
    /// This interface will be used by controllers to get the correct Identity Provider.
    /// </summary>
    interface IIdentityProviderCollection
    {
        IIdentityProvider Get(string name);
        void BuildCollection(IConfigurationSection config);
    }
}
