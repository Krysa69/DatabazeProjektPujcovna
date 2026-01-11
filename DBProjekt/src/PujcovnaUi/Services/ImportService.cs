using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PujcovnaUi.Data.Repositories;

namespace PujcovnaUi.Services;

public sealed class ImportService
{
    private readonly UsersRepository _users;
    private readonly AssetsRepository _assets;

    public ImportService(UsersRepository users, AssetsRepository assets)
    {
        _users = users;
        _assets = assets;
    }

    public async Task<string> ImportAsync(IFormFile? usersCsv, IFormFile? assetsJson)
    {
        var sb = new StringBuilder();

        if (usersCsv != null && usersCsv.Length > 0)
        {
            var (ok, fail) = await ImportUsersCsvAsync(usersCsv);
            sb.AppendLine($"Users CSV: OK={ok}, FAIL={fail}");
        }
        else
        {
            sb.AppendLine("Users CSV: not provided.");
        }

        if (assetsJson != null && assetsJson.Length > 0)
        {
            var (ok, fail) = await ImportAssetsJsonAsync(assetsJson);
            sb.AppendLine($"Assets JSON: OK={ok}, FAIL={fail}");
        }
        else
        {
            sb.AppendLine("Assets JSON: not provided.");
        }

        return sb.ToString().Trim();
    }

    private async Task<(int ok, int fail)> ImportUsersCsvAsync(IFormFile file)
    {
        int ok = 0, fail = 0;

        using var sr = new StreamReader(file.OpenReadStream(), Encoding.UTF8);
        var lines = new List<string>();

        while (!sr.EndOfStream)
        {
            var line = (await sr.ReadLineAsync()) ?? "";
            if (!string.IsNullOrWhiteSpace(line))
                lines.Add(line);
        }

        // allow header
        var start = 0;
        if (lines.Count > 0 && lines[0].ToLowerInvariant().Contains("email"))
            start = 1;

        for (int i = start; i < lines.Count; i++)
        {
            try
            {
                // supports delimiter ; or ,
                var parts = lines[i].Split(';');
                if (parts.Length < 2) parts = lines[i].Split(',');

                if (parts.Length < 2)
                    throw new FormatException("Expected: FullName;Email");

                var fullName = parts[0].Trim();
                var email = parts[1].Trim();

                if (fullName.Length < 3) throw new ArgumentException("FullName too short.");
                if (email.Length < 5 || !email.Contains("@")) throw new ArgumentException("Invalid email.");

                await _users.InsertAsync(fullName, email);
                ok++;
            }
            catch
            {
                fail++;
            }
        }

        return (ok, fail);
    }

    private async Task<(int ok, int fail)> ImportAssetsJsonAsync(IFormFile file)
    {
        int ok = 0, fail = 0;

        using var stream = file.OpenReadStream();
        var items = await JsonSerializer.DeserializeAsync<List<AssetImportDto>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (items == null) return (0, 0);

        foreach (var it in items)
        {
            try
            {
                var categoryName = (it.CategoryName ?? "").Trim();
                var name = (it.Name ?? "").Trim();
                var serial = (it.SerialNumber ?? "").Trim();

                if (categoryName.Length == 0) throw new ArgumentException("CategoryName missing.");
                if (name.Length == 0) throw new ArgumentException("Name missing.");
                if (serial.Length == 0) throw new ArgumentException("SerialNumber missing.");
                if (it.PurchasePrice <= 0) throw new ArgumentException("PurchasePrice must be > 0.");

                var categoryId = await _assets.GetOrCreateCategoryIdAsync(categoryName);
                await _assets.InsertAsync(categoryId, name, serial, it.PurchasePrice);

                ok++;
            }
            catch
            {
                fail++;
            }
        }

        return (ok, fail);
    }

    private sealed class AssetImportDto
    {
        public string? CategoryName { get; set; }
        public string? Name { get; set; }
        public string? SerialNumber { get; set; }
        public double PurchasePrice { get; set; }
    }
}
