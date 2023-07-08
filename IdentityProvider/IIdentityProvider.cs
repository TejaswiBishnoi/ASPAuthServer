using Microsoft.Extensions.Configuration;
namespace IdentityProvider
{
    public interface IIdentityProvider
    {
        string Name { get; }
        AuthorizationRequestObject? CreateAuthorizationRequest();
        IdentityObject? ExchangeCodeForIdentity(string? code);
        void Configure(IConfigurationSection config);
    }
}