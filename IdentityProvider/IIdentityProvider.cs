namespace IdentityProvider
{
    public interface IIdentityProvider
    {
        string Name { get; }
        IdentityObject? ProvideIdentity();
        void Configure(string? config);
    }
}