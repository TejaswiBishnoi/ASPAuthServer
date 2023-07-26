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
        /// <summary>
        /// Returns the Identity Provider with the given name.
        /// </summary>
        /// <param name="name">Indexer for IIdentityProvider</param>
        /// <returns>Identity Provider with given name</returns>
        IIdentityProvider? this[string name] { get; }
        /// <summary>
        /// Builds the collection of Identity Providers from the given configuration.
        /// </summary>
        /// <param name="config">The configuration using which the collection has to be created.</param>
        void Build(IConfigurationSection config);
        /// <summary>
        /// Add a new Identity Provider to the collection.
        /// </summary>
        /// <param name="provider">The Identity Provider which is to be added.</param>
        void Add(IIdentityProvider provider);
        /// <summary>
        /// Add a new Identity Provider to the collection from the given configuration.
        /// </summary>
        /// <param name="config">Configuration from which the Identity Provider will be created.</param>
        void AddFromConfiguration(IConfigurationSection config);
        /// <summary>
        /// Remove the given Identity Provider from the collection.
        /// </summary>
        /// <param name="provider">The Identity Provider which has to be removed.</param>
        void Remove(IIdentityProvider provider);
        /// <summary>
        /// Remove the Identity Provider with the given key from the collection.
        /// </summary>
        /// <param name="key">Name/Key of the Identity Provider that has to be removed.</param>
        void RemoveKey(string key);
    }
    /// <summary>
    /// Collection of Identity Providers. Referenced by controllers to get the correct Identity Provider.
    /// </summary>
    /// <remarks>A reference/default implementation of IIdentityProviderCollection</remarks>
    public class IdentityProviderCollection: IIdentityProviderCollection
    {
        private readonly IDictionary<string, IIdentityProvider> _identityProviders;
        private readonly IIdentityProviderFactory _identityProviderFactory;
        public IIdentityProvider? this[string name]
        {
            get {
                if (_identityProviders.ContainsKey(name)) return _identityProviders[name];
                else return null;
            }
        }
        /// <summary>
        /// The constructor for IdentityProviderCollection.
        /// </summary>
        /// <param name="identityProviderFactory">The Identity Provider Factory that is to be used. For DI purpose.</param>
        public IdentityProviderCollection(IIdentityProviderFactory identityProviderFactory)
        {
            _identityProviders = new Dictionary<string, IIdentityProvider>();
            _identityProviderFactory = identityProviderFactory;
        }
        public void Build(IConfigurationSection config)
        {
            //throw new NotImplementedException("Build method not implemented");
            foreach (var section in config.GetChildren())
            {
                foreach(var subsection in section.GetChildren())
                {
                    var identityProvider = _identityProviderFactory.GetIdentityProvider(subsection);
                    if (identityProvider != null)
                    {
                        _identityProviders.Add(identityProvider.Name, identityProvider);
                    }
                }
            }
        }
        public void Add(IIdentityProvider identityProvider)
        {
            //throw new NotImplementedException("Add method not implemented");
            if (!_identityProviders.ContainsKey(identityProvider.Name))
            {
                _identityProviders.Add(identityProvider.Name, identityProvider);
            }
            else throw new Exception("Identity Provider already exists");
        }
        public void AddFromConfiguration(IConfigurationSection config)
        {
            //throw new NotImplementedException("AddFromConfiguration method not implemented");
            var identityProvider = _identityProviderFactory.GetIdentityProvider(config);
            if (identityProvider != null && !_identityProviders.ContainsKey(identityProvider.Name))
            {
                _identityProviders.Add(identityProvider.Name, identityProvider);
            }
            else if (identityProvider != null) throw new Exception("Identity Provider already exists");
        }
        public void Remove(IIdentityProvider provider)
        {
            //throw new NotImplementedException("Remove method not implemented");
            if (_identityProviders.ContainsKey(identityProvider.Name))
            {
                _identityProviders.Remove(identityProvider.Name);
            }
            else throw new Exception("Identity Provider does not exist");
        }
        public void RemoveKey(string key)
        {
            //throw new NotImplementedException("RemoveKey method not implemented");
            if (_identityProviders.ContainsKey(key))
            {
                _identityProviders.Remove(key);
            }
            else throw new Exception("Identity Provider does not exist");
        }
    }
}
