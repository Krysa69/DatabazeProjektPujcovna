using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PujcovnaUi.Services;

namespace PujcovnaUi.Pages;

public class ImportModel : PageModel
{
    private readonly ImportService _import;
    public ImportModel(ImportService import) => _import = import;

    public string? Message { get; private set; }

    public async Task<IActionResult> OnPostAsync(
        IFormFile? usersCsv, IFormFile? assetsJson)
    {
        var result = await _import.ImportAsync(usersCsv, assetsJson);
        Message = result;
        return Page();
    }
}
