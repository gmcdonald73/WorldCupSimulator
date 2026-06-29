using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WorldCup.Services;
using WorldCup.Models;

namespace WorldCup.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly EspnWorldCupService _api;

        public IndexModel(ILogger<IndexModel> logger, EspnWorldCupService api)
        {
            _logger = logger;
            _api = api;
        }

        [BindProperty]
        public int NumSimulations { get; set; } = 1000;
        public SortedDictionary<int,MatchResult> Matches { get; set; }

        public async Task OnGetAsync()
        {
            Matches = await _api.GetMatches();
        }
    }
}
