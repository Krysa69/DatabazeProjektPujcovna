using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PujcovnaUi.Data.Repositories;

namespace PujcovnaUi.Pages;

public class AssetsModel : PageModel
{
    private readonly AssetsRepository _repo;

    public AssetsModel(AssetsRepository repo) => _repo = repo;

    public List<CategoryRow> Categories { get; private set; } = new();
    public List<AssetRow> Assets { get; private set; } = new();

    public string? Error { get; private set; }
    public string? Success { get; private set; }

    public async Task OnGetAsync()
    {
        Categories = await _repo.GetCategoriesAsync();
        Assets = await _repo.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(
        int categoryId, string name, string serialNumber, double purchasePrice)
    {
        try
        {
            await _repo.InsertAsync(categoryId, name, serialNumber, purchasePrice);
            Success = "Asset added.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        await OnGetAsync();
        return Page();
    }
}
