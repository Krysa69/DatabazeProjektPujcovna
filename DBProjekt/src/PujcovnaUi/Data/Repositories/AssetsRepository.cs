using Microsoft.Data.SqlClient;
using PujcovnaUi.Data;

namespace PujcovnaUi.Data.Repositories;

public sealed class AssetsRepository
{
    private readonly SqlConnectionFactory _factory;
    public AssetsRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<List<CategoryRow>> GetCategoriesAsync()
    {
        const string sql = "SELECT CategoryId, Name FROM dbo.Categories ORDER BY Name;";

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<CategoryRow>();
        while (await r.ReadAsync())
            list.Add(new CategoryRow(r.GetInt32(0), r.GetString(1)));

        return list;
    }

    public async Task<int> GetOrCreateCategoryIdAsync(string categoryName)
    {
        categoryName = (categoryName ?? "").Trim();
        if (categoryName.Length == 0) throw new ArgumentException("Category name is empty.");

        await using var con = _factory.Create();
        await con.OpenAsync();

        // 1) Try get
        const string getSql = "SELECT CategoryId FROM dbo.Categories WHERE Name=@Name;";
        await using (var getCmd = new SqlCommand(getSql, con))
        {
            getCmd.Parameters.AddWithValue("@Name", categoryName);
            var found = await getCmd.ExecuteScalarAsync();
            if (found != null && found != DBNull.Value)
                return (int)found;
        }

        // 2) Create
        const string insSql = """
            INSERT INTO dbo.Categories(Name)
            OUTPUT INSERTED.CategoryId
            VALUES (@Name);
        """;
        await using (var insCmd = new SqlCommand(insSql, con))
        {
            insCmd.Parameters.AddWithValue("@Name", categoryName);
            return (int)await insCmd.ExecuteScalarAsync();
        }
    }

    public async Task<List<AssetRow>> GetAllAsync()
    {
        const string sql = """
            SELECT a.AssetId, a.Name, a.SerialNumber, a.PurchasePrice, a.IsActive,
                   c.Name AS CategoryName
            FROM dbo.Assets a
            JOIN dbo.Categories c ON c.CategoryId = a.CategoryId
            ORDER BY a.AssetId DESC;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<AssetRow>();
        while (await r.ReadAsync())
        {
            list.Add(new AssetRow(
                r.GetInt32(0),
                r.GetString(1),
                r.GetString(2),
                r.GetDouble(3),
                r.GetBoolean(4),
                r.GetString(5)
            ));
        }
        return list;
    }

    public async Task<List<AssetPickRow>> GetAvailableForLoanAsync()
    {
        // používá view dbo.vw_AssetAvailability
        const string sql = """
            SELECT AssetId, Name, SerialNumber
            FROM dbo.vw_AssetAvailability
            WHERE IsActive=1 AND IsBorrowed=0
            ORDER BY Name;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<AssetPickRow>();
        while (await r.ReadAsync())
            list.Add(new AssetPickRow(r.GetInt32(0), r.GetString(1), r.GetString(2)));

        return list;
    }

    public async Task<int> InsertAsync(int categoryId, string name, string serialNumber, double purchasePrice)
    {
        const string sql = """
            INSERT INTO dbo.Assets(CategoryId, Name, SerialNumber, PurchasePrice)
            OUTPUT INSERTED.AssetId
            VALUES (@CategoryId, @Name, @SerialNumber, @PurchasePrice);
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@CategoryId", categoryId);
        cmd.Parameters.AddWithValue("@Name", (name ?? "").Trim());
        cmd.Parameters.AddWithValue("@SerialNumber", (serialNumber ?? "").Trim());
        cmd.Parameters.AddWithValue("@PurchasePrice", purchasePrice);

        return (int)await cmd.ExecuteScalarAsync();
    }
}

public sealed record CategoryRow(int CategoryId, string Name);
public sealed record AssetRow(int AssetId, string Name, string SerialNumber, double PurchasePrice, bool IsActive, string CategoryName);
public sealed record AssetPickRow(int AssetId, string Name, string SerialNumber);
