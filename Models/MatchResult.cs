namespace WorldCup.Models
{
    public enum KnockoutStage
    {
        R32,
        R16,
        QF,
        SF,
        ThirdPlacePlayoff,
        Final
    }

    public enum MatchStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Postponed,
        Cancelled,
        Unknown
    }

    public class MatchResult
    {
        public int MatchId { get; set; }
        public KnockoutStage Stage
        {
            get
            {
                return MatchId switch
                {
                    >= 73 and <= 88 => KnockoutStage.R32,
                    >= 89 and <= 96 => KnockoutStage.R16,
                    >= 97 and <= 100 => KnockoutStage.QF,
                    >= 101 and <= 102 => KnockoutStage.SF,
                    103 => KnockoutStage.ThirdPlacePlayoff,
                    104 => KnockoutStage.Final,
                    _ => throw new InvalidOperationException($"Unknown MatchId {MatchId}")
                };
            }
        }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public string HomeTeamCode { get; set; }
        public string AwayTeamCode { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public MatchStatus Status { get; set; }

        public DateTime MatchDate { get; set; }
        public string HomeTeamLogoUrl { get; set; }
        public string AwayTeamLogoUrl { get; set; }

        public int HomeTeamFromMatchId { get; set; } // get HomeTeam from winner of this match
        public int AwayTeamFromMatchId { get; set; } // get AwayTeam from winner of this match
        public string Winner {  get; set; }

        public string? ResultNote { get; set; }

        public string Loser =>
            Winner == HomeTeamCode ? AwayTeamCode : HomeTeamCode;

        public void SetResult(string winnerCode, int homeGoals, int awayGoals)
        {
            Winner = winnerCode;
            HomeGoals = homeGoals;
            AwayGoals = awayGoals;
            Status = MatchStatus.Completed;
        }

    }
}
