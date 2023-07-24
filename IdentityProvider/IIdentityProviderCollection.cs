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
    public interface IIdentityProviderCollection
    {
        IIdentityProvider? this[string name] { get; }
        void Build(IConfigurationSection config);
        void Add(IIdentityProvider provider);
        void AddFromConfiguration(IConfigurationSection config);
        void Remove(IIdentityProvider provider);
        void RemoveKey(string key);
    }
    public class IdentityProviderCollection: IIdentityProviderCollection
    {
        private IDictionary<string, IIdentityProvider> _identityProviders;
        public IIdentityProvider? this[string name]
        {
            get {
                if (_identityProviders.ContainsKey(name)) return _identityProviders[name];
                else return null;
            }
        }
        public IdentityProviderCollection()
        {
            _identityProviders = new Dictionary<string, IIdentityProvider>();
        }
    }
}
