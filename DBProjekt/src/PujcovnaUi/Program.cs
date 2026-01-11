using PujcovnaUi.Data;
using PujcovnaUi.Data.Repositories;
using PujcovnaUi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// config
builder.Services.AddSingleton<SqlConnectionFactory>();

// repositories (D1)
builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<AssetsRepository>();
builder.Services.AddScoped<LoansRepository>();
builder.Services.AddScoped<PointsRepository>();
builder.Services.AddScoped<ReportsRepository>();

// services (use cases)
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<PointsService>();
builder.Services.AddScoped<ImportService>();

var app = builder.Build();

// Global error handling – tester nesmí vidět stacktrace jako první věc v životě
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // i v dev je fajn mít rozumné chyby
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

app.Run();
