using System.Collections.Generic;

public class HDLeagueLeaderboard {
    //---[Classes and Enums]----------------------------------------------------
    public enum State {
        NONE = 0,
        WAITING_RESPONSE,
        SUCCESS,
        ERROR
    }

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



    //---[Attributes]-----------------------------------------------------------
    private string m_leagueSku;



    //---[Methods]--------------------------------------------------------------
    public HDLeagueLeaderboard(string _sku) {
        m_leagueSku = _sku;
    }

    public void RequestLeaderboard() {
        //Right now, we don't need the league sku, because, in server, they are using the player id to retrieve the leaderboard.
        GameServerManager.SharedInstance.HDLeagues_GetLeaderboard(OnLeaderboardResponse);
    }

    private void OnLeaderboardResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {

    }
}
