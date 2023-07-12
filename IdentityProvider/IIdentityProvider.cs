using Microsoft.Extensions.Configuration;
namespace IdentityProvider
{
    /// <summary>
    /// Interface for all Identity Providers
    /// </summary>
    public interface IIdentityProvider
    {
        /// <summary>
        /// Name of the IdentityProvider as per configuration.
        /// It will help in identification of the provider.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Creates a new authorization request with the given parameters and as per configuration.
        /// </summary>
        /// <param name="signup"></param>
        /// <returns>A collection of Key-Value pairs representing different parameters of the auth request.</returns>
        IDictionary<string, string>? CreateAuthorizationRequest(bool signup);
        /// <summary>
        /// This method exchanges the authorization code for the identity information.
        /// To be used for sign-up when we need additional information to pre-fill user profile.
        /// </summary>
        /// <param name="code">The Authorization Code</param>
        /// <returns>An IdentityObject object which contains all the necessary identity information.</returns>
        IdentityObject? ExchangeCodeForIdentityInfo(string? code);
        /// <summary>
        /// This method exchanges the authorization code for the identity.
        /// It does not return additional information, just the parameters that can authenticate the user.
        /// </summary>
        /// <param name="code">The Authorization Code</param>
        /// <returns>The string representation of chosen identity parameter.</returns>
        string ExchangeCodeForIdentity(string? code);
        /// <summary>
        /// This method configures the IdentityProvider with the configuration provided in the configuration file through <paramref name="config"/>.
        /// </summary>
        /// <param name="name">Name of the Identity Provider.</param>
        /// <param name="config">Parameters from the config file.</param>
        /// <seealso cref="IConfigurationSection" href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfigurationsection?view=dotnet-plat-ext-6.0"/>
        void Configure(string name, IConfigurationSection config);
    }
}