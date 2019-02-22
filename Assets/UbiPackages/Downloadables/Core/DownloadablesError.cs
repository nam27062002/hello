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
            CRC_Mismatch,                           // This error arises when the file downloaded doesn't match the CRC stated by the catalog
            Network_Uri_Malformed,                  // This error arises when the uri of the downloadable to download is malformed
            Network_Server_Size_Mismatch,           // This error arises when the client requests for more bytes than the ones available in server
            Network_Unauthorized_Reachability,      // This error arises when trying to download with unauthorized reachability (4G with no permission) 
            Network_Web_Exception,                  // This error arises when there's a problem accessing a url
            Internal
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
                    Type = EType.Network_Web_Exception;
                }
                else if (e is UriFormatException)
                {
                    Type = EType.Network_Uri_Malformed;
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
