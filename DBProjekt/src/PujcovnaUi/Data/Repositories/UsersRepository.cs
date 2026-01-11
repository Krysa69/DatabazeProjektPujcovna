using Microsoft.Data.SqlClient;
using PujcovnaUi.Data;

namespace PujcovnaUi.Data.Repositories;

public sealed class UsersRepository
{
    private readonly SqlConnectionFactory _factory;

    public UsersRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<List<UserRow>> GetAllAsync()
    {
        const string sql = """
            SELECT UserId, FullName, Email, IsActive, CreatedAt
            FROM dbo.Users
            ORDER BY FullName;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        await using var r = await cmd.ExecuteReaderAsync();

        var list = new List<UserRow>();
        while (await r.ReadAsync())
        {
            list.Add(new UserRow(
                r.GetInt32(0),
                r.GetString(1),
                r.GetString(2),
                r.GetBoolean(3),
                r.GetDateTime(4)
            ));
        }
        return list;
    }

    public async Task<int> InsertAsync(string fullName, string email)
    {
        const string sql = """
            INSERT INTO dbo.Users(FullName, Email)
            OUTPUT INSERTED.UserId
            VALUES (@FullName, @Email);
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@FullName", fullName);
        cmd.Parameters.AddWithValue("@Email", email);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task UpdateAsync(int userId, string fullName, string email, bool isActive)
    {
        const string sql = """
            UPDATE dbo.Users
            SET FullName=@FullName, Email=@Email, IsActive=@IsActive
            WHERE UserId=@UserId;
        """;

        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FullName", fullName);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@IsActive", isActive);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0) throw new InvalidOperationException("User not found.");
    }

    public async Task DeleteAsync(int userId)
    {
        const string sql = "DELETE FROM dbo.Users WHERE UserId=@UserId;";
        await using var con = _factory.Create();
        await con.OpenAsync();

        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@UserId", userId);

        await cmd.ExecuteNonQueryAsync();
    }
}

public sealed record UserRow(int UserId, string FullName, string Email, bool IsActive, DateTime CreatedAt);
