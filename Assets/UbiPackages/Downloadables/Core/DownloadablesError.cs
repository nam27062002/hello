using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Downloadables
{
    public class Error
    {
        public enum EType
        {
            None = 0,

            Disk_UnauthorizedAccess = 100,                          // The user doesn't have permission to read/write in disk (typically in Android)
            Disk_IOException,                                       // Typically no free space available
            Disk_Other,                                             // Any other disk related disk

            Network_Uri_Malformed = 200,                            // This error arises when the uri of the downloadable to download is malformed            
            Network_Server_Size_Mismatch,                           // This error arises when the client requests for more bytes than the ones available in server            
            Network_No_Reachability,                                // This error arises when trying to download with no access to internet
            Network_Unauthorized_Reachability,                      // This error arises when trying to download with unauthorized reachability (4G with no permission)             
            Network_Web_Exception_Connect_Failure,                  // This error arises when the server is down
            Network_Web_Exception_Timeout,                          // This error arises when there's no response from server after a while
            Network_Web_Exception_Protocol_Error,                   // This error arises when server responds with 403 (Forbidden)
            Network_Web_Exception_Proxy_Failure,                    // This error arises when there's a problem resolvind name because of proxy
            Network_Web_Exception_No_Access_To_Content,             // This error arises when content is not accessible (No 2xx status code)
            Network_Web_Exception_Other,                            // This error arises when there's any other related to web problem 

            Internal_CRC_Mismatch = 300,                            // This error arises when the file downloaded doesn't match the CRC stated by the catalog
            Internal_Too_Many_CRC_Mismatches,                       // This error arises when a download is blocked because it has finishes with Internal_CRC_Mismatch too many times
            Internal_NotAvailable,                                  // This error arises when the downloadable is not available but it's been requested
            Internal_Automatic_Download_Disabled,                   // This error arises when automatic downloads are required before the system is disabled, typically because it
                                                                    // hasn't been unlocked yet
            Internal_Download_Disabled,                             // This error arises when downloading is not enabled, typically because high performance is required, for example
                                                                    // while the user is playing
            Internal_Download_Aborted,                              // This error arises when downloading is aborted, typically because the user quits the application
            Other = 400                                             // Any other error
        };

        public static Array ErrorTypeValues = Enum.GetValues(typeof(EType));

        private static List<string> sm_typeString;
        private static List<string> TypeStrings
        {
            get
            {
                if (sm_typeString == null)
                {
                    sm_typeString = new List<string>();

                    int count = ErrorTypeValues.Length;
                    for (int i = 0; i < count; i++)
                    {
                        sm_typeString.Add((ErrorTypeValues.GetValue(i)).ToString());
                    }
                }

                return sm_typeString;
            }
        }        

        public static EType StringToType(string typeAsString)
        {            
            int index = TypeStrings.IndexOf(typeAsString);
            return (index == -1) ? EType.None : ((EType)ErrorTypeValues.GetValue(index));
        }

        public EType Type;

        public string Message;

        public Error(Exception e)
        {
            if (e == null)
            {
                Type = EType.None;
                Message = null;
            }
            else
            {
                if (e is IOException)
                {
                    Type = EType.Disk_IOException;
                }
                else if (e is UnauthorizedAccessException)
                {
                    Type = EType.Disk_UnauthorizedAccess;
                }
                else if (e is WebException)
                {
                    WebException we = e as WebException;
                    switch (we.Status)
                    {
                        case WebExceptionStatus.ConnectFailure:
                            Type = EType.Network_Web_Exception_Connect_Failure;
                            break;

                        case WebExceptionStatus.Timeout:
                            Type = EType.Network_Web_Exception_Timeout;
                            break;

                        case WebExceptionStatus.ProtocolError:
                            Type = EType.Network_Web_Exception_Protocol_Error;
                            break;

                        case WebExceptionStatus.ProxyNameResolutionFailure:
                            Type = EType.Network_Web_Exception_Proxy_Failure;
                            break;

                        default:
                            Type = EType.Network_Web_Exception_Other;
                            break;
                    }                    
                }
                else if (e is UriFormatException)
                {
                    Type = EType.Network_Uri_Malformed;
                }
                else
                {
                    Type = EType.Other;
                }                

                Message = e.Message;
            }
        }

        public Error(EType type, string message = null)
        {
            Type = type;
            Message = message;
        }
    }
}
