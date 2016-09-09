using FGOL.Save;
using FGOL.ThirdParty.MiniJSON;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace FGOL.Server
{
    public class Logging
    {
        private const string EndPoint = "data.logentries.com";
        private const int Port = 80;

        private string m_logToken = null;

        public Logging(string logToken)
        {
            m_logToken = logToken;

#if !UNITY_EDITOR
            LogCallbackHandler.RegisterLogCallbackThreaded(UnityLogHandler);
#endif
        }

        public void SendLog(string message, string stackTrace, LogType type)
        {
            if(m_logToken != null)
            {
                DateTime now = DateTime.UtcNow;

                Dictionary<string, string> messageDic = new Dictionary<string, string>();

                messageDic.Add("time", now.ToString("yyyy-MM-dd HH:mm:ss"));
                messageDic.Add("type", type.ToString());
                messageDic.Add("message", message);
#if UNITY_IOS
				messageDic.Add("platform", "iOS");
#elif UNITY_ANDROID && AMAZON
				messageDic.Add("platform", "Amazon");
#elif UNITY_ANDROID
				messageDic.Add("platform", "Android");
#endif
				try
				{
					string version = Globals.GetApplicationVersion();
					messageDic.Add("version", version);
				}
				catch (Exception)
				{
					//do nothing
				}
				messageDic.Add("stackTrace", stackTrace);

				string logMessage = m_logToken + " " + Json.Serialize(messageDic);

                SendMessage(logMessage);
            }
        }

        private void UnityLogHandler(string message, string stackTrace, LogType type)
        {
            //We only handle errors and above
            if((type == LogType.Error || type == LogType.Exception))
            {
                SendLog(message, stackTrace, type);
            }
        }

        private void SendMessage(string message)
        {
            TcpClient socket = null;

            try
            {
                socket = new TcpClient();
                socket.SendTimeout = 5000;
                socket.BeginConnect(EndPoint, Port, delegate(IAsyncResult result) 
                {
                    if (result != null)
                    {
                        try
                        {
                            message += "\n";
                            byte[] buf = System.Text.ASCIIEncoding.ASCII.GetBytes(message.Replace("\0xFF", "\0xFF\0xFF"));
                            socket.GetStream().Write(buf, 0, buf.Length);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("FGOL.Server.Logging (SendMessage) :: Exception - " + e.Message);
                        }
                        finally
                        {
                            if (socket != null)
                            {
                                socket.EndConnect(result);
                            }
                        }
                    }
                }, null);
            }
            catch(Exception e)
            {
                Debug.LogWarning("FGOL.Server.Logging (SendMessage) :: Exception - " + e.Message);
            }
        }
    }
}
