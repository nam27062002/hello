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

    private State m_state;
    public State state { get { return m_state; } }


    //---[Methods]--------------------------------------------------------------
    public HDLeagueLeaderboard(string _sku) {
        m_leagueSku = _sku;
        m_state = State.NONE;
    }

    public void RequestLeaderboard() {
        //Right now, we don't need the league sku, because, in server, they are using the player id to retrieve the leaderboard.
        GameServerManager.SharedInstance.HDLeagues_GetLeaderboard(OnLeaderboardResponse);

        m_state = State.WAITING_RESPONSE;
    }

    private void OnLeaderboardResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Leaderboard", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // parse Json
            LoadData(responseJson);

            m_state = State.SUCCESS;
        } else {

            m_state = State.ERROR;
        }
    }

    private void LoadData(SimpleJSON.JSONNode _data) {

    }
}
