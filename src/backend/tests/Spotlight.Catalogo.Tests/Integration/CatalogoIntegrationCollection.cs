using Xunit;

namespace Spotlight.Catalogo.Tests.Integration;

[CollectionDefinition(Name)]
public class CatalogoIntegrationCollection : ICollectionFixture<CatalogoIntegrationFixture>
{
    public const string Name = "CatalogoIntegration";
}
