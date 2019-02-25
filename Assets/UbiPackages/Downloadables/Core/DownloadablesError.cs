﻿using System;
using System.IO;
using System.Net;

namespace Downloadables
{
    public class Error
    {
        public enum EType
        {
            None,
            Disk_UnauthorizedAccess,
            Disk_IOException,
            Disk_Other,
            CRC_Mismatch,                           // This error arises when the file downloaded doesn't match the CRC stated by the catalog
            Network_Uri_Malformed,                  // This error arises when the uri of the downloadable to download is malformed            
            Network_Server_Size_Mismatch,           // This error arises when the client requests for more bytes than the ones available in server
            Network_CRC_Mismatch,                   // This error arises when the CRC of the file downloaded doesn't match with the expected one
            Network_Unauthorized_Reachability,      // This error arises when trying to download with unauthorized reachability (4G with no permission)             
            Network_Web_Exception_Connect_Failure,  // This error arises when the server is down
            Network_Web_Exception_Timeout,          // This error arises when there's no response from server after a while
            Network_Web_Exception_Other,            // This error arises when there's any other related to web problem 
            Other
        };

        //public static int TypesCount = Enum.GetValues(typeof(EType)).Length;

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
