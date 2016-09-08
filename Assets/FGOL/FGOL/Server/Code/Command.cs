using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using FGOL.Configuration;
using BestHTTP;
using FGOL.ThirdParty.MiniJSON;

namespace FGOL.Server
{
    public class Command : Request
    {
        public enum Type
        {
            Authenticated,
            Normal
        };

        public delegate void OnCommandCompleteDG(Error error, Dictionary<string, object> result = null);

		public static Dictionary<string, string> BaseUrls
        {
			get { return ms_baseUrls; }
        }

        private static Dictionary<string, string> ms_baseUrls = new Dictionary<string, string>();

        private string m_name = null;
        private string m_url = null;
		private string m_server = null;
        private Type m_type = Type.Normal;
        private Method m_method;

        public Command(string name, string url, string server, Type type, Method method = Method.POST, int connectionTimeout = 30, int timeout = 30)
        {
			m_name = name;
			m_url = url;
			m_server = server;
            m_type = type;
            m_method = method;
            m_connectionTimeout = connectionTimeout;
            m_timeout = timeout;
        }

        public Type CommandType
        {
            get { return m_type; }
        }

        public string Name
        {
            get { return m_name; }
        }

        public void Run(Dictionary<string, string> parameters, Hashtable headers, OnCommandCompleteDG callback, string sslValidationDomain = null)
        {
            bool rawOutput = Convert.ToBoolean(Config.Instance["request.rawOutput"]);

            parameters["_"] = ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();

			string url = SanitizeURL(m_url);

            if(rawOutput)
            {
                Debug.Log(string.Format("FGOL.Server.Command :: Parameters for url: {0}\nParams: {1}", url, Json.Serialize(parameters)));
            }

			Run(url, m_method, headers, parameters, null, delegate(Error error, HTTPResponse response)
            {
                if(rawOutput)
                {
                    if (error != null)
                    {
                        Debug.LogError(string.Format("FGOL.Server.Command :: Output for url: {0}\nError: {1}\nRaw output: {2}", url, error, response != null ? response.DataAsText : null));
                    }
                    else if (response != null)
                    {
                        Debug.Log(string.Format("FGOL.Server.Command :: Output for url: {0}\nRaw output: {1}", url, response.DataAsText));
                    }
                    else
                    {
                        Debug.Log(string.Format("FGOL.Server.Command :: Output for url: {0}\nNO RESPONSE", url));
                    }
                }

                if(callback != null)
                {
                    if(error == null)
                    {
                        if(response != null)
                        {
                            int status = (int)Math.Floor(response.StatusCode / 100.0);

                            switch(status)
                            {
                                case 2:
                                    {
                                        if(response.DataAsText != null && response.DataAsText != "")
                                        {
                                            Dictionary<string, object> result = Json.Deserialize(response.DataAsText) as Dictionary<string, object>;
                                            if(result != null)
                                            {
                                                if(result.ContainsKey("response"))
                                                {
                                                    Dictionary<string, object> expectedResult = result["response"] as Dictionary<string, object>;

                                                    if(expectedResult != null)
                                                    {
                                                        callback(null, expectedResult);
                                                    }
                                                    else
                                                    {
                                                        error = new InvalidServerResponseError("(WRONG RESPONSE FORMAT) " + response.DataAsText);
                                                        LogError(url, error);
                                                        callback(error, null);
                                                    }
                                                }
                                                else if(result.ContainsKey("error"))
                                                {
                                                    Dictionary<string, object> errorJson = result["error"] as Dictionary<string, object>;

                                                    string errorMessage = errorJson["message"] as string;
                                                    string errorName = errorJson["name"] as string;

                                                    //TODO do we still need status?
                                                    //string errorStatus = errorJson.ContainsKey("status") ? errorJson["status"] as string : string.Empty;

                                                    ErrorCodes errorCode = ErrorCodes.UnknownError;

                                                    try
                                                    {
                                                        int errorCodeRaw = errorJson.ContainsKey("code") ? Convert.ToInt32(errorJson["code"]) : -1;

                                                        object parsedCode = Enum.ToObject(typeof(ErrorCodes), errorCodeRaw);

                                                        if (Enum.IsDefined(typeof(ErrorCodes), parsedCode))
                                                        {
                                                            errorCode = (ErrorCodes)parsedCode;
                                                        }
                                                    }
                                                    catch(Exception) {}

                                                    Dictionary<string, object> errorData = null;

                                                    if(errorJson.ContainsKey("data"))
                                                    {
                                                        errorData = errorJson["data"] as Dictionary<string, object>;
                                                    }

                                                    error = new ServerInternalError(errorMessage, errorName, errorCode);

                                                    bool logError = true;

                                                    if(errorName != null)
                                                    {
                                                        switch(errorName)
                                                        {
                                                            case "AuthError":
                                                                error = new AuthenticationError(errorMessage, errorCode);
                                                                break;
                                                            case "CompatibilityError":
                                                                error = new CompatibilityError(errorMessage, errorCode, errorData);
                                                                break;
                                                            case "UploadDisallowedError":
                                                                error = new UploadDisallowedError(errorMessage, errorCode);
                                                                logError = false;
                                                                break;
                                                            case "UserError":
                                                                error = new UserAuthError(errorMessage, errorCode);
                                                                break;
                                                        }
                                                    }

                                                    if(logError)
                                                    {
                                                        LogError(url, error);
                                                    }

                                                    callback(error, null);
                                                }
                                                else if(result.ContainsKey("maintenance"))
                                                {
                                                    error = new MaintenanceError();

                                                    LogError(url, error);
                                                    callback(error, null);
                                                }
                                                else
                                                {
                                                    error = new InvalidServerResponseError("(WRONG FORMAT) " + response.DataAsText);
                                                    LogError(url, error);
                                                    callback(error, null);
                                                }
                                            }
                                            else
                                            {
                                                error = new InvalidServerResponseError("(NOT JSON) " + response.DataAsText);
                                                LogError(url, error);
                                                callback(error, null);
                                            }
                                        }
                                        else
                                        {
                                            error = new InvalidServerResponseError("(EMPTY RESPONSE)");

                                            LogError(url, error);
                                            callback(error, null);
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        error = new ClientConnectionError("Status code: " + response.StatusCode);
                                        LogError(url, error);
                                        callback(error, null);
                                    }
                                    break;
                                case 5:
                                    {
                                        error = new ServerConnectionError("Status code: " + response.StatusCode);
                                        LogError(url, error);
                                        callback(error, null);
                                    }
                                    break;
                                default:
                                    {
                                        error = new UnknownError("Status code: " + response.StatusCode);
                                        LogError(url, error);
                                        callback(error, null);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            error = new ServerConnectionError("Null response received");

                            LogError(url, error);
                            callback(error, null);
                        }
                    }
                    else
                    {
                        callback(error);
                    }
                }
            }, sslValidationDomain);
        }

        private void LogError(string url, Error error, Exception e = null)
        {
            Debug.LogError(String.Format("[FGOL.Server.Command] {0}: URL: {1}", error.GetType().Name, url));
            Debug.LogError(String.Format("[FGOL.Server.Command] {0}: {1} ({2})", error.GetType().Name, error.message, error.code));

            if(e != null)
            {
                Debug.LogError(e);
            }
        }

        private string SanitizeURL(string url)
        {
			string fullUrl = "";

			//If a relative url
            if(url != null && url.Length > 0 && url[0] == '/')
            {
				if (ms_baseUrls.ContainsKey(m_server))
				{
					fullUrl = ms_baseUrls[m_server];
				}
            }
				
			fullUrl += url;

			return fullUrl;
        }
    }
}
