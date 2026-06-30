using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WorldCup.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorldCup.Services
{
    public class EspnWorldCupService
    {
        private readonly HttpClient _http;
        private readonly TeamInfoService _teamInfoService;

        public EspnWorldCupService(HttpClient http, TeamInfoService teamInfoService)
        {
            _http = http;
            _teamInfoService = teamInfoService;
        }

        public async Task<SortedDictionary<int, MatchResult>> GetMatches()
        {
            var matches = new SortedDictionary<int, MatchResult>();

            await CreateMatchStructure(matches);

            await LoadMatchDetails(matches);

            // await SetupTestData(matches);

            return matches;
        }

        private async Task CreateMatchStructure(SortedDictionary<int, MatchResult> matches)
        {
            // Create matches and their dependencies
            // matchId, HomeTeamFromMatchId, AwayTeamFromMatchId
            var data = new[]
            {
                // R32
                (73,0,0),
                (74,0,0),
                (75,0,0),
                (76,0,0),
                (77,0,0),
                (78,0,0),
                (79,0,0),
                (80,0,0),
                (81,0,0),
                (82,0,0),
                (83,0,0),
                (84,0,0),
                (85,0,0),
                (86,0,0),
                (87,0,0),
                (88,0,0),

                // R16
                (89,74,77),
                (90,73,75),
                (91,76,78),
                (92,79,80),
                (93,83,84),
                (94,81,82),
                (95,86,88),
                (96,85,87),
                
                // QF
                (97,89,90),
                (98,93,94),
                (99,91,92),
                (100,95,96),
                
                // Semi finals
                (101,97,98),
                (102,99,100),

                // Final
                (104,101,102)
            };

            foreach (var x in data)
            {
                matches[x.Item1] = new MatchResult
                {
                    MatchId = x.Item1,
                    HomeTeamFromMatchId = x.Item2,
                    AwayTeamFromMatchId = x.Item3
                };
            }
        }

        private async Task LoadMatchDetails(SortedDictionary<int, MatchResult> matches)
        {
            var espnEventToMatchIdData = new[]
            {
                (760486,73),
                (760487,76),
                (760489,74),
                (760488,75),
                (760490,78),
                (760492,77),
                (760491,79),
                (760495,80),
                (760493,82),
                (760494,81),
                (760497,84),
                (760496,83),
                (760498,85),
                (760499,88),
                (760500,86),
                (760501,87),
                (760502,90),
                (760503,89),
                (760504,91),
                (760505,92),
                (760506,93),
                (760507,94),
                (760509,95),
                (760508,96),
                (760510,97),
                (760511,98),
                (760512,99),
                (760513,100),
                (760514,101),
                (760515,102),
                (760516,103),
                (760517,104)
            };

            var espnIdToMatchIdMap = new Dictionary<int, int>();

            foreach (var x in espnEventToMatchIdData)
            {
                espnIdToMatchIdMap[x.Item1] = x.Item2;
            }

            var url =
                $"https://site.api.espn.com/apis/site/v2/sports/soccer/fifa.world/scoreboard" +
                "?dates=20260628-20260731" +
                $"&_={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

            var json = await _http.GetStringAsync(url);

            using var doc = JsonDocument.Parse(json);

            var events = doc.RootElement
                .GetProperty("events")
                .EnumerateArray();

            foreach (var e in events)
            {
                var eventIdString = e.GetProperty("id").GetString();
                int eventId = int.Parse(eventIdString);
                var competitions = e.GetProperty("competitions")[0];
                var dateString = e.GetProperty("date").GetString();
                DateTime matchDate = DateTime.Parse(dateString);

                var venue = competitions.GetProperty("venue");
                var stadium = venue.GetProperty("fullName").GetString();

                string ResultNote = "";

                if (competitions.TryGetProperty("notes", out var notes) &&
                    notes.GetArrayLength() > 0)
                {
                    ResultNote = notes[0].GetProperty("headline").GetString();
                }

                var competitors = competitions.GetProperty("competitors");

                var home = competitors[0];
                var away = competitors[1];

                string homeTeamName = home.GetProperty("team").GetProperty("displayName").GetString();
                string awayTeamName = away.GetProperty("team").GetProperty("displayName").GetString();

                string homeTeamCode = home.GetProperty("team").GetProperty("abbreviation").GetString();
                string awayTeamCode = away.GetProperty("team").GetProperty("abbreviation").GetString();

                int homeScore = int.Parse(home.GetProperty("score").GetString());
                int awayScore = int.Parse(away.GetProperty("score").GetString());

                string homeTeamLogoUrl = home.GetProperty("team").GetProperty("logo").GetString();
                string awayTeamLogoUrl = away.GetProperty("team").GetProperty("logo").GetString();

                string status = e.GetProperty("status")
                    .GetProperty("type")
                    .GetProperty("name")
                    .GetString();


                if (espnIdToMatchIdMap.ContainsKey(eventId))
                {
                    int matchId = espnIdToMatchIdMap[eventId];

                    if (matches.ContainsKey(matchId))
                    {
                        var match = matches[matchId];

                        // Make these unique (not an issue once all pairs resolved)
                        if (homeTeamCode == "3RD") homeTeamCode = matchId.ToString() + "H";
                        if (awayTeamCode == "3RD") awayTeamCode = matchId.ToString() + "A";

                        match.MatchDate = matchDate;
                        match.HomeTeamName = homeTeamName;
                        match.AwayTeamName = awayTeamName;
                        match.HomeTeamCode = homeTeamCode;
                        match.AwayTeamCode = awayTeamCode;
                        match.HomeTeamLogoUrl = homeTeamLogoUrl;
                        match.AwayTeamLogoUrl = awayTeamLogoUrl;
                        match.HomeGoals = homeScore;
                        match.AwayGoals = awayScore;

                        if (IsCompleted(e))
                        {
                            match.Status = MatchStatus.Completed;

                        }
                        else
                        {
                            match.Status = MapEspnStatus(status);
                        }

                        if (match.Status == MatchStatus.Completed)
                        {
                            string winnerCode = "";

                            if (IsWinner(home))
                            {
                                winnerCode = homeTeamCode;
                            }
                            else if (IsWinner(away))
                            {
                                winnerCode = awayTeamCode;
                            }

                            match.Winner = winnerCode;
                            match.ResultNote = ResultNote;
                        }

                        _teamInfoService.Add(homeTeamCode, homeTeamName, homeTeamLogoUrl);
                        _teamInfoService.Add(awayTeamCode, awayTeamName, awayTeamLogoUrl);

                    }
                }
            }
        }

        private static MatchStatus MapEspnStatus(string? espnStatus)
        {
            return espnStatus switch
            {
                "STATUS_SCHEDULED" => MatchStatus.Scheduled,

                "STATUS_IN_PROGRESS" => MatchStatus.InProgress,
                "STATUS_FIRST_HALF" => MatchStatus.InProgress,
                "STATUS_HALFTIME" => MatchStatus.InProgress,
                "STATUS_SECOND_HALF" => MatchStatus.InProgress,
                "STATUS_OVERTIME" => MatchStatus.InProgress,
                "STATUS_EXTRA_TIME" => MatchStatus.InProgress,
                "STATUS_END_OF_EXTRATIME" => MatchStatus.InProgress,
                "STATUS_SHOOTOUT" => MatchStatus.InProgress,
                "STATUS_PENALTY_SHOOTOUT" => MatchStatus.InProgress,

                "STATUS_FINAL" => MatchStatus.Completed,
                "STATUS_FULL_TIME" => MatchStatus.Completed,
                "STATUS_FINAL_AET" => MatchStatus.Completed,
                "STATUS_FINAL_PEN" => MatchStatus.Completed,

                "STATUS_POSTPONED" => MatchStatus.Postponed,
                "STATUS_DELAYED" => MatchStatus.Postponed,
                "STATUS_SUSPENDED" => MatchStatus.Postponed,

                "STATUS_CANCELED" => MatchStatus.Cancelled,
                "STATUS_CANCELLED" => MatchStatus.Cancelled,
                "STATUS_ABANDONED" => MatchStatus.Cancelled,

                _ => MatchStatus.InProgress  // If status not recognised, assume InProgress
            };
        }

        private static bool IsCompleted(JsonElement match)
        {
            return match.GetProperty("status")
                        .GetProperty("type")
                        .GetProperty("completed")
                        .GetBoolean();
        }

        private static bool IsWinner(JsonElement competitor)
        {
            return competitor.TryGetProperty("winner", out var prop) &&
                   prop.ValueKind == JsonValueKind.True;
        }

        private async Task SetupTestData(SortedDictionary<int, MatchResult> matches)
        {
            matches[73].SetResult("CAN", 0, 1);
        }
    }
}