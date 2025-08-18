namespace Core
{
    class Game
    {
        public string GameId { get; set; }

        public enum GameType
        {
            nap,
            hkrpg,
            hk4e,
            bh3,
        }

        static readonly Dictionary<(Region, GameType), string> gameMap = new()
        {
            {(Region.OSREL, GameType.nap), "U5hbdsT9W7"},
            {(Region.CNREL, GameType.nap), "x6znKlJ0xK"},
            {(Region.OSREL, GameType.hkrpg), "4ziysqXOQ8"},
            {(Region.CNREL, GameType.hkrpg), "64kMb5iAWu"},
            {(Region.OSREL, GameType.hk4e), "gopR6Cufr3"},
            {(Region.CNREL, GameType.hk4e), "1Z8W5NHUQb"},
            {(Region.OSREL, GameType.bh3), "5TIVvvcwtM"},
            {(Region.CNREL, GameType.bh3), "osvnlOc0S8"},
        };

        public Game(Region region, string id)
        {
            bool isRel = !Enum.TryParse(id, out GameType game);
            if (isRel)
            {
                this.GameId = id;
            } else
            {
                this.GameId = gameMap[(region, game)];
            }
        }

        public string GetGameId()
        {
            return this.GameId;
        }
    }
}
