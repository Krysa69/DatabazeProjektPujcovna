using Microsoft.Data.SqlClient;
using PujcovnaUi.Data;

namespace PujcovnaUi.Data.Repositories;

public sealed class ReportsRepository
{
    private readonly SqlConnectionFactory _factory;
    public ReportsRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<List<ReportRow>> GetTopAssetsAsync(int top = 10)
    {
        const string sql = """
            SELECT TOP (@Top)
                a.Name AS AssetName,
                c.Name AS CategoryName,
                COUNT(*) AS TimesLoaned,
                MAX(l.CreatedAt) AS LastLoan
            FROM dbo.LoanItems li
            JOIN dbo.Assets a ON a.AssetId = li.AssetId
            JOIN dbo.Categories c ON c.CategoryId = a.CategoryId
            JOIN dbo.Loans l ON l.LoanId = li.LoanId
            GROUP BY a.Name, c.Name
            ORDER BY TimesLoaned DESC, LastLoan DESC;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Top", top);

        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<ReportRow>();
        while (await r.ReadAsync())
        {
            list.Add(new ReportRow(
                r.GetString(0),
                r.GetString(1),
                r.GetInt32(2),
                r.GetDateTime(3)
            ));
        }

        return list;
    }
}

public sealed record ReportRow(string AssetName, string CategoryName, int TimesLoaned, DateTime LastLoan);
