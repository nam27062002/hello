using System;

namespace Downloadables
{
    public class Error
    {
        public enum EType
        {
            None,
            Disk_UnauthorizedAccess,
            Disk_IOException,
            Disk_Other
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
                if (e is System.IO.IOException)
                {
                    Type = EType.Disk_IOException;
                }
                else if (e is System.UnauthorizedAccessException)
                {
                    Type = EType.Disk_UnauthorizedAccess;
                }
                else
                {
                    Type = EType.Disk_Other;
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
