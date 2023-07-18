using Microsoft.Extensions.Configuration;
using IdentityProvider;
using System.ComponentModel.DataAnnotations;

namespace Tests
{
    public class Tests
    {
        IConfigurationSection config;
        GoogleIdentityProvider googleIdentityProvider;
        string code;
        string email;
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);
            IConfiguration config = builder.Build();
            IConfigurationSection section = config.GetSection("IdentityPlugins").GetSection("OAuth").GetSection("Google");            
            this.config = section;
            googleIdentityProvider = new GoogleIdentityProvider();
            googleIdentityProvider.Configure("Google", section);
            code = config["code"];
            email = config["TestEmail"];
        }
        [SetUp]
        public void Setup()
        {
            
        }

        [Test]
        public void ConfigTest()
        {
            try
            {
                var x = new GoogleIdentityProvider();
                x.Configure("Google", config);                
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }        
        [Test]
        public void AuthorizationURLTest()
        {
            IDictionary<string, string>? dic = null;
            try
            {
                dic = googleIdentityProvider.CreateAuthorizationRequest();
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            if (dic == null || !dic.ContainsKey("URL"))
            {
                Assert.Fail("Request not complete!");
            }
            Assert.Pass(dic["URL"]);
        }
        [Test]
        public void GetIdentityTest()
        {
            string? iden = null;
            try
            {
                iden = googleIdentityProvider.ExchangeCodeForIdentity(code);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + "\n" + ex.StackTrace);
            }
            Assert.IsTrue(!string.IsNullOrEmpty(iden) && iden == email);
        }        
    }
}