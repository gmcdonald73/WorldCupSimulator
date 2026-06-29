namespace WorldCup.Models
{
    public class TeamStats
    {
        public string Code { get; set; }
        public int R16 { get; set; }
        public int QF { get; set; }
        public int SF { get; set; }
        public int Final { get; set; }
        public int Wins { get; set; }

        // Populated later by API presentation layer
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public int Elo { get; set; }
    }
}
