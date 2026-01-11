using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PujcovnaUi.Data.Repositories;

namespace PujcovnaUi.Pages;

public class UsersModel : PageModel
{
    private readonly UsersRepository _repo;

    public UsersModel(UsersRepository repo) => _repo = repo;

    public List<UserRow> Users { get; private set; } = new();
    public string? Error { get; private set; }
    public string? Success { get; private set; }

    public async Task OnGetAsync()
    {
        Users = await _repo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(string fullName, string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length < 3)
                throw new ArgumentException("Full name must be at least 3 characters.");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Invalid email.");

            await _repo.InsertAsync(fullName.Trim(), email.Trim());
            Success = "User added.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int userId, string fullName, string email, bool? isActive)
    {
        try
        {
            await _repo.UpdateAsync(userId, fullName.Trim(), email.Trim(), isActive == true);
            Success = "User updated.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int userId)
    {
        try
        {
            await _repo.DeleteAsync(userId);
            Success = "User deleted.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        await OnGetAsync();
        return Page();
    }
}
