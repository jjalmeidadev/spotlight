using Xunit;

namespace Spotlight.Recomendacao.Tests.Integration;

/// <summary>
/// Agrupa os testes de integração do fluxo <c>GET /recomendacao/me</c> para que
/// compartilhem uma única instância de <see cref="IntegrationFixture"/> — e,
/// com ela, o MESMO container Postgres e o MESMO servidor WireMock. Subir um
/// container por teste seria caro e desnecessário; o xUnit garante que os
/// testes da coleção não rodam em paralelo entre si, então não há disputa pelo
/// estado compartilhado.
/// </summary>
[CollectionDefinition(nameof(IntegrationCollection))]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationFixture>;
