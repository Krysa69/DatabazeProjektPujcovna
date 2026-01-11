using PujcovnaUi.Data.Repositories;

namespace PujcovnaUi.Services;

public sealed class PointsService
{
    private readonly PointsRepository _repo;

    public PointsService(PointsRepository repo) => _repo = repo;

    public async Task TransferAsync(int fromUserId, int toUserId, int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be > 0.");
        if (fromUserId <= 0 || toUserId <= 0) throw new ArgumentException("Invalid user.");
        if (fromUserId == toUserId) throw new ArgumentException("Users must be different.");

        // stored procedure throws clean errors too
        await _repo.TransferViaStoredProcedureAsync(fromUserId, toUserId, amount);
    }
}
