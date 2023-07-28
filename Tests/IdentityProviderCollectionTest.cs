using IdentityProvider;

namespace Tests
{
    internal class IdentityProviderCollectionMockTest
    {
        [Test]
        public void ConfigTest()
        {
            var mockConfig = new Mock<IConfigurationSection>();
            mockConfig.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection>() { mockConfig.Object});
            var mockFactory = new Mock<IIdentityProviderFactory>();
            var mockIdentityProvider = new Mock<IIdentityProvider>();
            mockIdentityProvider.Setup(mockIdentityProvider => mockIdentityProvider.Name).Returns("mockIdentityProvider");
            mockFactory.Setup(x => x.GetIdentityProvider(mockConfig.Object)).Returns(mockIdentityProvider.Object);
            mockFactory.Setup(x => x.GetMetadata(mockConfig.Object)).Returns(new IdentitityProviderMetadata() { 
                Name = "mockIdentityProvider",
                Message = "MockTest",
                Icon = "MockIcon"
            });
            try
            {
                var identityProviderCollection = new IdentityProviderCollection(mockFactory.Object);
                identityProviderCollection.Build(mockConfig.Object);
                Assert.Multiple(() =>
                {
                    Assert.That(identityProviderCollection.Metadata, Is.Not.EqualTo(""));
                    Assert.That(identityProviderCollection["mockIdentityProvider"], Is.Not.Null);
                    Assert.That(identityProviderCollection["mockIdentityProvider"]?.Name, Is.EqualTo("mockIdentityProvider"));
                });
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message, ex.StackTrace);
            }
        }
        [Test]
        public void AddTest()
        {
            var mockIdentityProvider = new Mock<IIdentityProvider>();
            mockIdentityProvider.Setup(mockIdentityProvider => mockIdentityProvider.Name).Returns("mockIdentityProvider");
            var metadata = new IdentitityProviderMetadata()
            {
                Name = "mockIdentityProvider",
                Message = "MockTest",
                Icon = "MockIcon"
            };
            var mockFactory = new Mock<IIdentityProviderFactory>();
            try
            {
                var identityProviderCollection = new IdentityProviderCollection(mockFactory.Object);
                identityProviderCollection.Add(mockIdentityProvider.Object, metadata);
                Assert.Multiple(() =>
                {
                    Assert.That(identityProviderCollection.Metadata, Is.Not.EqualTo(""));
                    Assert.That(identityProviderCollection["mockIdentityProvider"], Is.Not.Null);
                    Assert.That(identityProviderCollection["mockIdentityProvider"]?.Name, Is.EqualTo("mockIdentityProvider"));
                });
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message, ex.StackTrace);
            }
        }
        [Test]
        public void IndexerTest()
        {
            var mockIdentityProvider = new Mock<IIdentityProvider>();
            mockIdentityProvider.Setup(mockIdentityProvider => mockIdentityProvider.Name).Returns("mockIdentityProvider");
            var metadata = new IdentitityProviderMetadata()
            {
                Name = "mockIdentityProvider",
                Message = "MockTest",
                Icon = "MockIcon"
            };
            var mockFactory = new Mock<IIdentityProviderFactory>();
            try
            {
                var identityProviderCollection = new IdentityProviderCollection(mockFactory.Object);
                identityProviderCollection.Add(mockIdentityProvider.Object, metadata);
                Assert.Multiple(() =>
                {
                    Assert.That(identityProviderCollection["mockIdentityProvider"], Is.Not.Null);
                    Assert.That(identityProviderCollection["mockIdentityProvider"]?.Name, Is.EqualTo("mockIdentityProvider"));
                    Assert.That(identityProviderCollection["_mockIdentityProvider"], Is.Null);
                });
            }
            catch(Exception ex)
            {
                Assert.Fail(ex.Message, ex.StackTrace);
            }
        }
        [Test]
        public void MetadataTest()
        {
            var mockIdentityProvider = new Mock<IIdentityProvider>();
            mockIdentityProvider.Setup(mockIdentityProvider => mockIdentityProvider.Name).Returns("mockIdentityProvider");
            var metadata = new IdentitityProviderMetadata()
            {
                Name = "mockIdentityProvider",
                Message = "MockTest",
                Icon = "MockIcon"
            };
            var mockFactory = new Mock<IIdentityProviderFactory>();
            try
            {
                var identityProviderCollection = new IdentityProviderCollection(mockFactory.Object);
                identityProviderCollection.Add(mockIdentityProvider.Object, metadata);
                var deserializedMetadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, IdentitityProviderMetadata>>(identityProviderCollection.Metadata);
                Assert.Multiple(() =>
                {
                    Assert.That(deserializedMetadata?.ContainsKey("mockIdentityProvider"), Is.True);
                    Assert.That(deserializedMetadata?["mockIdentityProvider"], Is.Not.Null);
                    Assert.That(deserializedMetadata?["mockIdentityProvider"].Name, Is.EqualTo("mockIdentityProvider"));
                    Assert.That(deserializedMetadata?["mockIdentityProvider"].Message, Is.EqualTo("MockTest"));
                    Assert.That(deserializedMetadata?["mockIdentityProvider"].Icon, Is.EqualTo("MockIcon"));
                });
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message, ex.StackTrace);
            }
        }
    }
}
