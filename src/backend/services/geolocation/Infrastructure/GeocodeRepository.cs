using Npgsql;

namespace Spotlight.Geolocation.Infrastructure;

public class GeocodeRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public GeocodeRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task SaveAsync(decimal lat, decimal lng, string address)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO geocode_cache (lat, lng, address) VALUES (@lat, @lng, @addr);";
        cmd.Parameters.AddWithValue("lat", lat);
        cmd.Parameters.AddWithValue("lng", lng);
        cmd.Parameters.AddWithValue("addr", address);
        await cmd.ExecuteNonQueryAsync();
    }
}
