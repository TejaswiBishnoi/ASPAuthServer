using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        /// The metadata about the available identity providers that can be used by front-end to display the available options.
        /// </summary>
        /// <remarks>Metadata is a JSON string.</remarks>
        public string Metadata { get; }
        /// <summary>
        /// Builds the collection of Identity Providers from the given configuration.
        /// Will clear the existing collection and build a new one.
        /// </summary>
        /// <param name="config">The configuration using which the collection has to be created.</param>
        /// <remarks>If you need to rebuild the whole collection use this method else use other methods.</remarks>
        void Build(IConfigurationSection config);
        /// <summary>
        /// Add a new Identity Provider to the collection.
        /// </summary>
        /// <param name="provider">The Identity Provider which is to be added.</param>
        /// <param name="metadata">The metadata associated with the Identity Provider.</param>
        void Add(IIdentityProvider provider, IdentitityProviderMetadata metadata);
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
        private readonly IDictionary<string, IdentitityProviderMetadata> _identityProvidersMetadata;
        public string Metadata { get; private set; }
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
            _identityProvidersMetadata = new Dictionary<string, IdentitityProviderMetadata>();
            Metadata = System.Text.Json.JsonSerializer.Serialize(_identityProvidersMetadata);
        }
        private void UpdateMetadata() => Metadata = System.Text.Json.JsonSerializer.Serialize(_identityProvidersMetadata);
        public void Build(IConfigurationSection config)
        {
            //throw new NotImplementedException("Build method not implemented");
            _identityProvidersMetadata.Clear();
            _identityProviders.Clear();
            foreach (var section in config.GetChildren())
            {
                foreach(var subsection in section.GetChildren())
                {
                    var identityProvider = _identityProviderFactory.GetIdentityProvider(subsection);
                    var metadata = _identityProviderFactory.GetMetadata(subsection);
                    if (identityProvider != null && metadata != null)
                    {
                        _identityProviders.Add(identityProvider.Name, identityProvider);
                        _identityProvidersMetadata.Add(identityProvider.Name, metadata);
                    }
                }
            }
            UpdateMetadata();
        }
        public void Add(IIdentityProvider identityProvider, IdentitityProviderMetadata metadata)
        {
            //throw new NotImplementedException("Add method not implemented");
            if (!_identityProviders.ContainsKey(identityProvider.Name))
            {
                _identityProviders.Add(identityProvider.Name, identityProvider);
                _identityProvidersMetadata.Add(identityProvider.Name, metadata);
            }
            else throw new Exception("Identity Provider already exists");
            UpdateMetadata();
        }
        public void AddFromConfiguration(IConfigurationSection config)
        {
            //throw new NotImplementedException("AddFromConfiguration method not implemented");
            var identityProvider = _identityProviderFactory.GetIdentityProvider(config);
            var metadata = _identityProviderFactory.GetMetadata(config);
            if (identityProvider is not null && metadata is not null && !_identityProviders.ContainsKey(identityProvider.Name))
            {
                _identityProviders.Add(identityProvider.Name, identityProvider);
                _identityProvidersMetadata.Add(identityProvider.Name, metadata);
            }
            else if (identityProvider != null) throw new Exception("Identity Provider already exists");
            UpdateMetadata();
        }
        public void Remove(IIdentityProvider provider)
        {
            //throw new NotImplementedException("Remove method not implemented");
            if (_identityProviders.ContainsKey(provider.Name))
            {
                _identityProviders.Remove(provider.Name);
                _identityProvidersMetadata.Remove(provider.Name);
            }
            else throw new Exception("Identity Provider does not exist");
        }
        public void RemoveKey(string key)
        {
            //throw new NotImplementedException("RemoveKey method not implemented");
            if (_identityProviders.ContainsKey(key))
            {
                _identityProviders.Remove(key);
                _identityProvidersMetadata.Remove(key);
            }
            else throw new Exception("Identity Provider does not exist");
        }
    }
}
