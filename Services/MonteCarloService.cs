using WorldCup.Models;

namespace WorldCup.Services
{
    public class MonteCarloService
    {
        private readonly Random _rand = new();
        private readonly TeamInfoService _teamInfoService;

        public MonteCarloService(TeamInfoService teamInfoService)
        {
            _teamInfoService = teamInfoService;
        }

        public Dictionary<string, TeamStats> Run(
            SortedDictionary<int,MatchResult> matches,
            int simulations)
        {
            var simStats = new SimulationStats();

            for (int i = 0; i < simulations; i++)
            {
                foreach (var match in matches.Values)
                {
                    if(match.Status != MatchStatus.Completed)
                    {
                        if (match.HomeTeamFromMatchId > 0)
                        {
                            match.HomeTeamCode = matches[match.HomeTeamFromMatchId].Winner;
                        }

                        if (match.AwayTeamFromMatchId > 0)
                        {
                            match.AwayTeamCode = matches[match.AwayTeamFromMatchId].Winner;
                        }

                        var (homeGoals, awayGoals) =
                            SimulateMatch(match.HomeTeamCode, match.AwayTeamCode);

                        match.HomeGoals = homeGoals;
                        match.AwayGoals = awayGoals;

                        if (homeGoals > awayGoals)
                            match.Winner = match.HomeTeamCode;
                        else
                            match.Winner = match.AwayTeamCode;

                    }

                    // pass Stage, Winner to method to record results for calculating stats
                    simStats.AddWinner(match.Stage, match.Winner);

                    if (match.Stage == KnockoutStage.R32)
                    {
                        // Show teams on the board even if they lost the R32 match
                        simStats.AddTeam(match.Loser);
                    }
                }
            }

            return simStats.Teams;
        }

        private (int homeGoals, int awayGoals) SimulateMatch(
            string homeTeamCode,
            string awayTeamCode)
        {
            var homeElo = _teamInfoService.Get(homeTeamCode).Elo;
            var awayElo = _teamInfoService.Get(awayTeamCode).Elo;

            double homeProb = 1.0 / (1.0 + Math.Pow(10, (awayElo - homeElo) / 400.0));

            double r = _rand.NextDouble();

            if (r < homeProb)
                return (1, 0);
            else
                return (0, 1);
        }
    }
}
