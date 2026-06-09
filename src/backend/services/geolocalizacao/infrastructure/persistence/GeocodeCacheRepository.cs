using Npgsql;
using Spotlight.Geolocalizacao.Application.Dtos;

namespace Spotlight.Geolocalizacao.Infrastructure.Persistence;

public interface IGeocodeCacheRepository
{
    Task SaveReverseAsync(GeocodingResponse response, CancellationToken cancellationToken);
}

public sealed class GeocodeCacheRepository(NpgsqlDataSource dataSource) : IGeocodeCacheRepository
{
    public async Task SaveReverseAsync(GeocodingResponse response, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO geocode_cache (lat, lng, address)
            VALUES (@lat, @lng, @address);
            """;
        command.Parameters.AddWithValue("lat", response.Latitude);
        command.Parameters.AddWithValue("lng", response.Longitude);
        command.Parameters.AddWithValue("address", response.FormattedAddress);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class NullGeocodeCacheRepository : IGeocodeCacheRepository
{
    public Task SaveReverseAsync(GeocodingResponse response, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
