using Microsoft.Data.SqlClient;
using PujcovnaUi.Data;

namespace PujcovnaUi.Data.Repositories;

public sealed class LoansRepository
{
    private readonly SqlConnectionFactory _factory;
    public LoansRepository(SqlConnectionFactory factory) => _factory = factory;

    public SqlConnection CreateConnection() => _factory.Create();

    public async Task<List<LoanRow>> GetAllAsync()
    {
        const string sql = """
            SELECT l.LoanId, u.FullName, l.Status, l.DueDate, l.CreatedAt, l.ReturnedAt
            FROM dbo.Loans l
            JOIN dbo.Users u ON u.UserId = l.UserId
            ORDER BY l.LoanId DESC;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<LoanRow>();
        while (await r.ReadAsync())
        {
            list.Add(new LoanRow(
                r.GetInt32(0),
                r.GetString(1),
                r.GetString(2),
                r.GetDateTime(3),
                r.GetDateTime(4),
                r.IsDBNull(5) ? null : r.GetDateTime(5)
            ));
        }

        return list;
    }
}

public sealed record LoanRow(int LoanId, string UserName, string Status, DateTime DueDate, DateTime CreatedAt, DateTime? ReturnedAt);
