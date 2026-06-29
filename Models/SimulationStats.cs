using System.Reflection;

namespace WorldCup.Models
{
    public class SimulationStats
    {
        public Dictionary<string, TeamStats> Teams { get; } = new();

        public void AddTeam(string teamCode)
        {
            Teams.TryAdd(teamCode, new TeamStats
            {
                Code = teamCode
            });
        }

        public void AddWinner(KnockoutStage currentStage, string teamCode)
        {
            if (!Teams.ContainsKey(teamCode))
            {
                Teams[teamCode] = new TeamStats { Code = teamCode };
            }

            var currTeam = Teams[teamCode];

            switch (currentStage)
            {
                case KnockoutStage.R32:
                    currTeam.R16++;
                    break;
                case KnockoutStage.R16:
                    currTeam.QF++;
                    break;
                case KnockoutStage.QF:
                    currTeam.SF++;
                    break;
                case KnockoutStage.SF:
                    currTeam.Final++;
                    break;
                case KnockoutStage.Final:
                    currTeam.Wins++;
                    break;
            }
        }
    }
}
