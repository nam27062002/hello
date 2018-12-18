using System.Collections.Generic;

public class HDLeagueLeaderboard {
    //---[Attributes]-----------------------------------------------------------

    private string m_leagueSku;

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

        liveDataState = HDLiveData.State.EMPTY;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    public void RequestData() {
        //Right now, we don't need the league sku, because, in server, they are using the player id to retrieve the leaderboard.
        GameServerManager.SharedInstance.HDLeagues_GetLeaderboard(OnDataResponse);

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

            // parse Json
            LoadData(responseJson.AsArray);
        } else {

            liveDataState = HDLiveData.State.ERROR;
        }

        liveDataError = outErr;
    }

    public void LoadData(SimpleJSON.JSONArray _data) {
        if (_data != null) {
            for (int i = 0; i < _data.Count; ++i) {
                SimpleJSON.JSONClass d = _data[i].AsObject;

                HDLiveData.Leaderboard.Record record = new HDLiveData.Leaderboard.Record();
                record.LoadData(d);
                record.rank = i;

                m_records.Add(record);
            }
        }
        liveDataState = HDLiveData.State.VALID;
    }
}
