using System.Collections.Generic;

public class HDLeagueLeaderboard {
    //---[Leaderboard Record]---------------------------------------------------
    public class Record {
        public uint position;

        public string name;
        public ulong score;

        public string dragonSku;
        public uint level;
        public List<string> pets;


        public Record() {
            position = 0;

            name = "";
            score = 0;

            dragonSku = "";
            level = 0;

            pets = new List<string>();
        }
    }
    //--------------------------------------------------------------------------

    public void RequestLeaderboard() {

    }

    private void OnLeaderboardResponse() {

    }

}
