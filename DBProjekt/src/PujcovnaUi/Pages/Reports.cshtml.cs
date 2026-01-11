using Microsoft.AspNetCore.Mvc.RazorPages;
using PujcovnaUi.Data.Repositories;

namespace PujcovnaUi.Pages;

public class ReportsModel : PageModel
{
    private readonly ReportsRepository _repo;
    public ReportsModel(ReportsRepository repo) => _repo = repo;

    public List<ReportRow> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Rows = await _repo.GetTopAssetsAsync();
    }
}
