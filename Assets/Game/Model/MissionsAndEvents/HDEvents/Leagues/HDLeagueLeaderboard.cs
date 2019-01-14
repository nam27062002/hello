using System.Collections.Generic;

public class HDLeagueLeaderboard {
    //---[Attributes]-----------------------------------------------------------

    private string m_leagueSku;

    private long m_playerScore;
    public long playerScore { get { return m_playerScore; } set { if (m_playerScore < value) m_playerScore = value;  } }

    private int m_playerRank;
    public int playerRank { get { return m_playerRank; } }

    private List<HDLiveData.Leaderboard.Record> m_records;
    public List<HDLiveData.Leaderboard.Record> records { get { return m_records; } }

    public HDLiveData.State liveDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes liveDataError { get; private set; }



    //---[Methods]--------------------------------------------------------------

    public HDLeagueLeaderboard(string _sku) {
        m_leagueSku = _sku;
        m_records = new List<HDLiveData.Leaderboard.Record>();
        Clean();
    }

    public void Clean() {
        m_records.Clear();
        m_playerScore = 0;
        m_playerRank = 0;

        liveDataState = HDLiveData.State.EMPTY;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    public void RequestData() {
        __RequestData();

        liveDataState = HDLiveData.State.WAITING_RESPONSE;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    private void OnDataResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
        HDLiveDataManager.ResponseLog("[Leagues] Leaderboard", _error, _response);

        HDLiveDataManager.ComunicationErrorCodes outErr = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        SimpleJSON.JSONNode responseJson = HDLiveDataManager.ResponseErrorCheck(_error, _response, out outErr);

        if (outErr == HDLiveDataManager.ComunicationErrorCodes.NO_ERROR) {
            // clear Data
            m_records.Clear();
            m_playerRank = 0;

            // parse Json
            LoadData(responseJson);
        } else {

            liveDataState = HDLiveData.State.ERROR;
        }

        liveDataError = outErr;
    }

    public void LoadData(SimpleJSON.JSONNode _data) {
        if (_data != null) {
            SimpleJSON.JSONClass player = _data["u"].AsObject;
            m_playerScore = player["score"].AsLong;
            m_playerRank = player["rank"].AsInt;

            SimpleJSON.JSONArray array = _data["l"].AsArray;
            for (int i = 0; i < array.Count; ++i) {
                SimpleJSON.JSONClass d = array[i].AsObject;

                HDLiveData.Leaderboard.Record record = new HDLiveData.Leaderboard.Record();
                record.LoadData(d);
                record.rank = i;

                m_records.Add(record);
            }
        }
        liveDataState = HDLiveData.State.VALID;
    }



    //---[Server Calls]---------------------------------------------------------

    private void __RequestData() {
        if (HDLiveDataManager.TEST_CALLS) {
            ApplicationManager.instance.StartCoroutine(HDLiveDataManager.DelayedCall("league_leaderboard_data.json", OnDataResponse));
        } else {
            //Right now, we don't need the league sku, because, in server, they are using the player id to retrieve the leaderboard.
            GameServerManager.SharedInstance.HDLeagues_GetLeaderboard(OnDataResponse);
        }
    }
}
