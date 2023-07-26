using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityProvider
{
    public class IdentitityProviderMetadata
    {
        string Name { get; set; }
        string Message { get; set; }
        string Icon { get; set; }
    }
    public interface IIdentityProviderFactory
    {
        IIdentityProvider? GetIdentityProvider(IConfigurationSection config);
        IdentitityProviderMetadata GetMetadata(IConfigurationSection config);
    }
    public class IdentityProviderFactory: IIdentityProviderFactory
    {
        public IIdentityProvider? GetIdentityProvider(IConfigurationSection config)
        {
            throw new NotImplementedException("GetIdentityProvider method not implemented");
        }
        public IdentitityProviderMetadata GetMetadata(IConfigurationSection config)
        {
            throw new NotImplementedException("GetMetadata method not implemented");
        }
    }
}
