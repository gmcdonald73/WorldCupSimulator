using WorldCup.Models;

namespace WorldCup.Services
{
    public class TeamInfoService
    {
        private readonly Dictionary<string, TeamInfo> _teams = new();
        private readonly Dictionary<string, int> _eloRatings = new();

        public TeamInfoService()
        {
            // Populate eloRanking dictionary
            var eloRatingData = new[]
            {
                ("ARG",2144),
                ("ESP",2134),
                ("FRA",2090),
                ("ENG",2028),
                ("BRA",2009),
                ("USA",1781),
                ("MAR",1877),
                ("CAN",1748),
                ("GER",1916),
                ("AUS",1800),
                ("NED",1980),
                ("MEX",1912),
                ("SUI",1914),
                ("BIH",1622),
                ("JPN",1910),
                ("CIV",1743),
                ("RSA",1575),
                ("KOR",1723),
                ("SCO",1745),
                ("PAR",1815),
                ("ECU",1902),
                ("SWE",1742),
                ("EGY",1740),
                ("IRN",1766),
                ("BEL",1869),
                ("NZL",1549),
                ("URU",1851),
                ("CPV",1625),
                ("KSA",1593),
                ("NOR",1951),
                ("SEN",1817),
                ("AUT",1841),
                ("ALG",1780),
                ("JOR",1632),
                ("COL",2006),
                ("POR",1988),
                ("COD",1666),
                ("UZB",1677),
                ("GHA",1584),
                ("CRO",1896),
                ("PAN",1668)
            };

            foreach (var x in eloRatingData)
            {
                _eloRatings[x.Item1] = x.Item2;
            }
        }

        public void Add(string code, string name, string logoUrl)
        {
            if (!_teams.ContainsKey(code))
            {
                var team = new TeamInfo
                {
                    Code = code,
                    Name = name,
                    LogoUrl = logoUrl,
                    Elo = _eloRatings.GetValueOrDefault(code, 1500)
                };

                _teams[code] = team;
            }
        }

        public TeamInfo Get(string code)
        {
            return _teams.TryGetValue(code, out var team)
                ? team
                : null;
        }

        public IEnumerable<TeamInfo> GetAll()
        {
            return _teams.Values;
        }
    }
}
