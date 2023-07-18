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
        string name;
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
            name = config["TestName"];
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
        [TestCase()]
        [TestCase(true)]
        public void AuthorizationURLTest(bool signup = false)
        {
            IDictionary<string, string>? dic = null;
            try
            {
                dic = googleIdentityProvider.CreateAuthorizationRequest(signup);
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
        [Test]
        public void GetIdentityInfoTest()
        {
            IdentityObject? info = null;
            try
            {
                info = googleIdentityProvider.ExchangeCodeForIdentityInfo(code);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + "\n" + ex.StackTrace);
            }
            Assert.IsTrue(info != null && info.Name == name && info.Email == email, info?.Token);
        }
    }
}