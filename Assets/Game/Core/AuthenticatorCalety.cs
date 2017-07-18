/// <summary>
/// This class is responsible for implementing the <c>Authenticator</c>interface by using Calety.
/// </summary>

using FGOL.Authentication;
using FGOL.Server;
using System;
using System.Collections.Generic;
public class AuthenticatorCalety : Authenticator
{
	protected override void ExtendedCheckConnection(Action<Error> callback)
    {
		GameServerManager.SharedInstance.Ping(
			(Error _error, GameServerManager.ServerResponse _response) => { 
				callback(_error); 
			}
		);
    }

    public override void Authenticate(string fgolID, User.LoginCredentials credentials, User.LoginType network, Action<Error, AuthResult> callback)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        parameters["authMethod"] = network.ToString();
        parameters["deviceToken"] = Token.ToString();

        if (!string.IsNullOrEmpty(fgolID))
        {
            parameters["fgolID"] = fgolID;
        }

        // [DGR] SERVER: Not supported yet
        //parameters["credentials"] = Json.Serialize(credentials.ToDictionary());

        Log("Authenticate " + network.ToString() + " socialID = " + credentials.socialID);
        GameServerManager.SharedInstance.LogInToServerThruPlatform(User.LoginTypeToCaletySocialPlatform(network), credentials.socialID, credentials.accessToken, 
			(Error commandError, GameServerManager.ServerResponse response) => 
            {
                Log("OnLoginToServerThruPlatform " + commandError);
                if (commandError == null)
                {
                    //string[] requiredParams = new string[] { "cloudCredentials", "cloudCredentialsExpiry", "fgolID", "savePath", "bucket", "sessionToken", "sessionExpiry", "socialExpiry", "authState", "cloudSaveAvailable" };

                    // [DGR] SERVER: Not needed yet
                    //if (Commander.IsValidResponse(response, requiredParams))
                    {
                        AuthResult result = new AuthResult();
                        result.fgolID = response["fgolID"] as string;
                        /*
                        result.cloudSaveLocation = response["savePath"] as string;
                        result.cloudSaveBucket = response["bucket"] as string;
                        */
                        result.sessionToken = response["sessionToken"] as string;
                        /*result.sessionExpiry = Convert.ToInt32(response["sessionExpiry"]);
                        result.cloudCredentials = response["cloudCredentials"] as Dictionary<string, object>;
                        result.cloudCredentialsExpiry = Convert.ToInt32(response["cloudCredentialsExpiry"]);
                        result.socialExpiry = Convert.ToInt32(response["socialExpiry"]);
                        result.cloudSaveAvailable = Convert.ToBoolean(response["cloudSaveAvailable"]);
                        */
                        result.authState = (AuthState)Enum.Parse(typeof(AuthState), response["authState"] as string);
                        result.upgradeAvailable = response.ContainsKey("upgradeAvailable") && Convert.ToBoolean(response["upgradeAvailable"]);

                        // TO UNCOMMENT to force flow https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/26%29Game+update+available
                        //result.upgradeAvailable = true;                        

                        // [DGR] cloud save is always available as long as the user is logged in
                        result.cloudSaveAvailable = true;

                        // TO UNCOMMENT to force flow https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/13%29Recommend+cloud+save
                        //result.cloudSaveAvailable = false;                        

                        callback(null, result);

                        /*if (OnLoggedIn != null)
                        {
                            OnLoggedIn();
                        }*/
                    }
                    /*else
                    {
                        callback(new InvalidServerResponseError("Missing response params: " + string.Join(",", requiredParams)), null);
                    }*/
                }
                else
                {
                    Log("OnLoginToServerThruPlatform error = " + commandError.ToString());
                    callback(commandError, null);
                }
            }
        );        
    }

	public override void Logout(Action<Error> callback)
    {
		GameServerManager.SharedInstance.LogOut(
			(Error _error, GameServerManager.ServerResponse _response) => { 
				callback(_error); 
			}
		);
    }

    public override void GetServerTime(Action<Error, string, int> onGetServerTime)
    {
        GameServerManager.SharedInstance.GetServerTime(
			(Error commandError, GameServerManager.ServerResponse response) =>
            {
                string dateTimeNow = null;
                int unixTimestamp = -1;

                if (commandError == null)
                {
                    if (response != null && response.ContainsKey("dateTime") && response.ContainsKey("unixTimestamp"))
                    {
                        dateTimeNow = response["dateTime"] as string;

                        try
                        {
                            unixTimestamp = Convert.ToInt32(response["unixTimestamp"]);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        commandError = new InvalidServerResponseError("Response not as expected");
                    }
                }

                onGetServerTime(commandError, dateTimeNow, unixTimestamp);
            }
        );
    }

    public override void UpdateSaveVersion(bool preliminary, Action<Error, int> onUpdate)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "fgolID", User.ID }
            };

        if (preliminary)
        {
            parameters["prelimUpdate"] = preliminary.ToString();
        }

        GameServerManager.SharedInstance.UpdateSaveVersion(preliminary,
            (Error commandError, GameServerManager.ServerResponse response) =>
            {
                //string dateTimeNow = null;
                int unixTimestamp = -1;

                if (commandError == null)
                {
                    if (response != null && response.ContainsKey("dateTime") && response.ContainsKey("unixTimestamp"))
                    {
                        //dateTimeNow = response["dateTime"] as string;

                        try
                        {
                            unixTimestamp = Convert.ToInt32(response["unixTimestamp"]);
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        commandError = new InvalidServerResponseError("Response not as expected");
                    }

                }

                onUpdate(commandError, unixTimestamp);
            }
        );      
    }

    #region log
    private const string PREFIX = "AuthenticatorCalety:";

    private void Log(string message)
    {
        Debug.Log(PREFIX + message);        
    }

    private void LogWarning(string message)
    {        
        Debug.LogWarning(PREFIX + message);
    }

    private void LogError(string message)
    {     
        Debug.LogError(PREFIX + message);
    }
    #endregion
}
