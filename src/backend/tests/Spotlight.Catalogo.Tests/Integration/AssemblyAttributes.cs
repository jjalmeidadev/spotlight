using Xunit;

// Evita corrida de variáveis de ambiente entre testes de integração e smoke tests.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
