using SimpleJSON;
using UnityEngine;
using System.Collections.Generic;
public class RequestNetworkOffline : RequestNetwork
{
	public bool loadInitValues = false;
	public delegate void Callback();

    public RequestNetworkOffline()
    {
    }
    
    public void Start()
    {
		// currentServerTime = 0;
    }

	public override void Update()
	{
		base.Update();
		if (timeToWait > 0f)
		{
			timeToWait -= Time.deltaTime;
			if (timeToWait <= 0f)
			{
				timeToWait = 0f;
				if (onTimeToWaitDone != null)
				{
					onTimeToWaitDone();
				}
			}
		}
	}
	
    public static JSONNode GetEmptyJSON()
    {
        return JSON.Parse("{}");
    }
    
    public static bool GetJSON(string _filename, out JSONNode node)
    {
		TextAsset _textAsset  = (TextAsset) Resources.Load(_filename, typeof(TextAsset));
		if ( _textAsset != null )
		{
			node = JSON.Parse( _textAsset.text );
			return true;
		}
		Debug.LogError("Could not load text asset " + _filename);     
		node = GetEmptyJSON();
		return false;
    }
    
    protected override void ExtendedLogin()
    {
        m_i_uid = 0;
        m_uid = "" + m_i_uid;
        ProcessGameCommandResponse(REQUEST_ACTION_LOGIN, GetEmptyJSON());
    }

    public override void RequestUniverse() 
    {
        string _cmd = REQUEST_ACTION_UNIVERSE;
        JSONNode _json;
		if ( GetJSON("userdata/universe", out _json) )
		{
			// if (InstanceManager.Config.UsePersistenceInitialValuesOffline())
			{
				loadInitValues = true;
			}
			
			if ( loadInitValues )
				LoadInitValues(_json);
			
			m_playerSince = _json["time"].AsLong / 1000;
			
			ProcessGameCommandResponse(_cmd, _json);
		}
		else
		{
			onGameResponseError(_cmd, _json);
		}
		
    }  
    
    
    public void LoadInitValues( JSONNode node )
    {

    }
    

	
	
	#region leaderboard
	public override void RequestGlobalLeaderboard()
	{
		if ( onLeaderboardResponse != null )
		{
			JSONNode _json;
			if ( GetJSON( "userdata/leaderboard_global",out _json ) )
			{
				onLeaderboardResponse( REQUEST_LEAGUE_GLOBAL, _json);
			}
			else
			{
				onLeaderboardResponseError(REQUEST_LEAGUE_GLOBAL, _json);
			}
		}
	}
	
	public override void RequestRegionLeaderboard(string region = "")
	{
		if ( onLeaderboardResponse != null )
		{
			JSONNode _json;
			if ( GetJSON( "userdata/leaderboard_region",out _json ) )
			{
				onLeaderboardResponse( REQUEST_LEAGUE_REGION, _json);
			}
			else
			{
				onLeaderboardResponseError(REQUEST_LEAGUE_REGION, _json);
			}
		}
	}
	
	public override void RequestFriendsLeadeboard( List<string> friend_ids)
	{
		if ( onLeaderboardResponse != null )
		{
			JSONNode _json;
			if ( GetJSON( "userdata/leaderboard_friends",out _json ) )
			{
				onLeaderboardResponse( REQUEST_LEAGUE_FRIENDS, _json);
			}
			else
			{
				onLeaderboardResponseError(REQUEST_LEAGUE_FRIENDS, _json);
			}
		}
	}
	#endregion
	
	#region time_to_wait
	private float timeToWait = 0f;
	private Callback onTimeToWaitDone;
	
	private void StartTimeToWait(float _timeToWait, Callback _onDone)
	{
		timeToWait = _timeToWait;
		onTimeToWaitDone = _onDone;
	}
    #endregion



    #region social
    public override void GetSocialMapping(JSONNode _platform_ids)
    {
        StartTimeToWait(1f, () => 
        { 
            JSONNode _response = null;

            if (_platform_ids != null)
            {
                _response = new JSONClass();

                JSONClass _dataObject = _platform_ids.AsObject;
                if (_dataObject != null)
                {
                    Dictionary<string, JSONNode> _dict = _dataObject.m_Dict;
                    if (_dict != null && _dict.Count > 0)
                    {
                        string _key;
                        foreach (KeyValuePair<string, JSONNode> _pair in _dict)
                        {
                            // _key = (_pair.Key == platform) ? LoginManager.GetSocialPlatformKeyFromPlatform(_pair.Key) : _pair.Key;
                            _key = "";
                            _response.Add(_key, _pair.Value);                            
                        }
                    }
                }            
            }

            if (onSocialResponse != null)
                onSocialResponse(REQUEST_SOCIAL_MAPPINGS, _response);
        }
        );
    }
    #endregion
}

