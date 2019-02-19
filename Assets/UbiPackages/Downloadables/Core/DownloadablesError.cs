using System;
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
            Network_Server_Size_Mismatch,           // This error arises when the client requests for more bytes than the ones available in server
            Network_Unauthorized_Reachability,      // This error arises when trying to download with unauthorized reachability (4G with no permission) 
            Network_Web_Exception,                  // This error arises when there's a problem accessing a url
            Internal
        };

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
                    Type = EType.Disk_Other;
                }
                else
                {
                    Type = EType.Internal;
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
