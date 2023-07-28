using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityProvider
{
    /// <summary>
    /// Metadata for Identity Provider
    /// </summary>
    public class IdentitityProviderMetadata
    {
        /// <summary>
        /// The name associated with the IdentityProvider
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Message to be displayed with Identity Provider (e.g. "Sign in with Google")
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Icon to be displayed with Identity Provider (e.g. Logo of Microsoft)
        /// </summary>
        public string Icon { get; set; }
    }
    /// <summary>
    /// Interface for Identity Provider Factory
    /// </summary>
    public interface IIdentityProviderFactory
    {
        /// <summary>
        /// Instantiates and fetch the Identity Provider with the given configuration.
        /// </summary>
        /// <param name="config">The Configuration for Identity Provider</param>
        /// <returns>The requested Identity Provider.</returns>
        IIdentityProvider? GetIdentityProvider(IConfigurationSection config);
        /// <summary>
        /// Fetches the Identity Provider Metadata with the given configuration.
        /// </summary>
        /// <param name="config">The configuration from which metadata is built.</param>
        /// <returns>The Metadata</returns>
        IdentitityProviderMetadata GetMetadata(IConfigurationSection config);
    }
    /// <summary>
    /// Reference/Default implementation of IIdentityProviderFactory
    /// This will be used for instantiating Identity Providers.
    /// </summary>
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
