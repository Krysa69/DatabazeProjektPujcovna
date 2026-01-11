using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PujcovnaUi.Data.Repositories;
using PujcovnaUi.Services;

namespace PujcovnaUi.Pages;

public class CreateLoanModel : PageModel
{
    private readonly UsersRepository _users;
    private readonly AssetsRepository _assets;
    private readonly LoanService _loanService;

    public CreateLoanModel(
        UsersRepository users,
        AssetsRepository assets,
        LoanService loanService)
    {
        _users = users;
        _assets = assets;
        _loanService = loanService;
    }

    public List<UserRow> Users { get; private set; } = new();
    public List<AssetPickRow> Assets { get; private set; } = new();

    public string? Error { get; private set; }
    public string? Success { get; private set; }

    public async Task OnGetAsync()
    {
        Users = await _users.GetAllAsync();
        Assets = await _assets.GetAvailableForLoanAsync();
    }

    public async Task<IActionResult> OnPostAsync(
        int userId, DateTime dueDate, int[] assetIds)
    {
        try
        {
            var loanId = await _loanService.CreateLoanAsync(userId, dueDate, assetIds);
            Success = $"Loan {loanId} created.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        await OnGetAsync();
        return Page();
    }
}
