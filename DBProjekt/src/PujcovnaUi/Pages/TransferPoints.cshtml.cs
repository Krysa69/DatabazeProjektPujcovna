using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PujcovnaUi.Data.Repositories;
using PujcovnaUi.Services;

namespace PujcovnaUi.Pages;

public class TransferPointsModel : PageModel
{
    private readonly UsersRepository _users;
    private readonly PointsService _points;

    public TransferPointsModel(UsersRepository users, PointsService points)
    {
        _users = users;
        _points = points;
    }

    public List<UserRow> Users { get; private set; } = new();
    public string? Error { get; private set; }
    public string? Success { get; private set; }

    public async Task OnGetAsync()
    {
        Users = await _users.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAsync(int fromUserId, int toUserId, int amount)
    {
        try
        {
            await _points.TransferAsync(fromUserId, toUserId, amount);
            Success = "Transfer completed.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        await OnGetAsync();
        return Page();
    }
}
