using Microsoft.Data.SqlClient;

namespace PujcovnaUi.Data;

public sealed class SqlConnectionFactory
{
    private readonly IConfiguration _config;

    public SqlConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public SqlConnection Create()
    {
        var cs = _config.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");

        return new SqlConnection(cs);
    }
}
