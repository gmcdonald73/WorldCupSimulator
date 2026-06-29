using WorldCup.Services;
using WorldCup.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpClient<EspnWorldCupService>();
builder.Services.AddScoped<MonteCarloService>();
builder.Services.AddSingleton<TeamInfoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapPost("/api/simulations/worldcup",
async (
    SimulationRequest request,
    EspnWorldCupService footballApi,
    MonteCarloService simulator,
    TeamInfoService teamInfoService) =>
{
    int numSims = request.NumSimulations;

    if(numSims < 1) numSims = 1;
    if (numSims > 100_000) numSims = 100_000;

    var matches = await footballApi.GetMatches();

    var result = simulator.Run(
        matches,
        numSims);

    foreach (var team in result.Values)
    {
        var teamInfo = teamInfoService.Get(team.Code);
        team.Name = teamInfo.Name;
        team.LogoUrl = teamInfo.LogoUrl;
        team.Elo = teamInfo.Elo;
    }

    return Results.Ok(result);
});

app.Run();
