using Microsoft.Extensions.Configuration;
namespace IdentityProvider
{
    /// <summary>
    /// Interface for all Identity Providers
    /// </summary>
    public interface IIdentityProvider
    {
        string Name { get; }
        IDictionary<string, string>? CreateAuthorizationRequest();
        IdentityObject? ExchangeCodeForIdentityInfo(string? code);
        string ExchangeCodeForIdentity(string? code);
        void Configure(string name, IConfigurationSection config);
    }
}