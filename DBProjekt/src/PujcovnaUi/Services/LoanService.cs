using Microsoft.Data.SqlClient;
using PujcovnaUi.Data.Repositories;

namespace PujcovnaUi.Services;

public sealed class LoanService
{
    private readonly LoansRepository _loansRepo;

    public LoanService(LoansRepository loansRepo) => _loansRepo = loansRepo;

    public async Task<int> CreateLoanAsync(int userId, DateTime dueDate, int[] assetIds)
    {
        if (userId <= 0) throw new ArgumentException("Invalid user.");
        if (assetIds == null || assetIds.Length == 0) throw new ArgumentException("Select at least one asset.");
        if (dueDate.Date < DateTime.Today) throw new ArgumentException("Due date cannot be in the past.");

        var distinctAssetIds = assetIds.Distinct().ToArray();

        await using var con = _loansRepo.CreateConnection();
        await con.OpenAsync();

        await using var tx = (SqlTransaction)await con.BeginTransactionAsync();

        try
        {
            // (optional) prevent lending already borrowed assets by checking view
            const string checkSql = """
                SELECT COUNT(*)
                FROM dbo.vw_AssetAvailability
                WHERE AssetId = @AssetId AND IsBorrowed = 1;
            """;

            foreach (var assetId in distinctAssetIds)
            {
                await using var checkCmd = new SqlCommand(checkSql, con, tx);
                checkCmd.Parameters.AddWithValue("@AssetId", assetId);

                var count = (int)await checkCmd.ExecuteScalarAsync();
                if (count > 0)
                    throw new InvalidOperationException($"Asset {assetId} is already borrowed.");
            }

            // 1) loan header
            const string insertLoan = """
                INSERT INTO dbo.Loans(UserId, Status, DueDate)
                OUTPUT INSERTED.LoanId
                VALUES (@UserId, 'BORROWED', @DueDate);
            """;

            await using var cmdLoan = new SqlCommand(insertLoan, con, tx);
            cmdLoan.Parameters.AddWithValue("@UserId", userId);
            cmdLoan.Parameters.AddWithValue("@DueDate", dueDate.Date);

            var loanId = (int)await cmdLoan.ExecuteScalarAsync();

            // 2) loan items
            const string insertItem = """
                INSERT INTO dbo.LoanItems(LoanId, AssetId, Note)
                VALUES (@LoanId, @AssetId, NULL);
            """;

            foreach (var assetId in distinctAssetIds)
            {
                await using var cmdItem = new SqlCommand(insertItem, con, tx);
                cmdItem.Parameters.AddWithValue("@LoanId", loanId);
                cmdItem.Parameters.AddWithValue("@AssetId", assetId);
                await cmdItem.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return loanId;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
