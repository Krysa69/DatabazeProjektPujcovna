using Microsoft.Data.SqlClient;
using PujcovnaUi.Data;

namespace PujcovnaUi.Data.Repositories;

public sealed class PointsRepository
{
    private readonly SqlConnectionFactory _factory;
    public PointsRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task TransferViaStoredProcedureAsync(int fromUserId, int toUserId, int amount)
    {
        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand("dbo.sp_TransferPoints", con);
        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@FromUserId", fromUserId);
        cmd.Parameters.AddWithValue("@ToUserId", toUserId);
        cmd.Parameters.AddWithValue("@Amount", amount);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<PointsBalanceRow>> GetBalancesAsync()
    {
        const string sql = """
            SELECT u.UserId, u.FullName, pa.Balance
            FROM dbo.PointsAccounts pa
            JOIN dbo.Users u ON u.UserId = pa.UserId
            ORDER BY u.FullName;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<PointsBalanceRow>();
        while (await r.ReadAsync())
            list.Add(new PointsBalanceRow(r.GetInt32(0), r.GetString(1), r.GetInt32(2)));

        return list;
    }
}

public sealed record PointsBalanceRow(int UserId, string FullName, int Balance);
