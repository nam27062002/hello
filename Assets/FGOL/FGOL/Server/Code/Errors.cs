using System;
using System.Collections.Generic;
namespace FGOL.Server
{
    public enum ErrorCodes
    {
        Unset = 0,

        #region Server Defined Errors
        UnknownError = -1,
        ParamError = -2,
        ValidationError = -3,
        SDKError = -4,
        UserError = -5,
        ConfigError = -6,
        AuthError = -7,
        CompatibilityError = -8,
        UploadDisallowedError = -9,
        MaintanenceError = -10,
        #endregion

        ClientConnectionTimeout = -11,
        ClientConnectionError = -12,
        ServerConnectionTimeout = -13,
        ServerConnectionError = -14,
        InvalidResponseError = -15,
        LoginError = -16,
        PermissionError = -17,
        FileNotFoundError = -18,
        FilePermissionError = -19,
        CorruptedFileError = -20,
        SaveError = -21,
        S3TokenInvalid = -22,
        UserAuthError = -23
    }

    #region Errors
    public class Error
    {
        protected string m_message = null;
        protected ErrorCodes m_code = ErrorCodes.Unset;

        private string m_stackTrace = null;

        public Error(string message, ErrorCodes code)
        {
            m_message = message;
            m_code = code;
            m_stackTrace = System.Environment.StackTrace;
        }

        public string message
        {
            get { return m_message; }
        }

        public ErrorCodes code
        {
            get { return m_code; }
        }

        public string stackTrace
        {
            get { return m_stackTrace; }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} ({2})", this.GetType(), m_message, m_code);
        }
    }

    public class UnknownError : Error
    {
        public UnknownError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Unknown error", code == ErrorCodes.Unset ? ErrorCodes.UnknownError : code)
        {
        }
    }

    public class ServerConnectionError : Error
    {
        public ServerConnectionError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Connection to server failed", code == ErrorCodes.Unset ? ErrorCodes.ServerConnectionError : code)
        {
        }
    }

    public class ServerInternalError : Error
    {
        private string m_errorName = null;

        public ServerInternalError(string message, string errorName, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Error occured on server", code == ErrorCodes.Unset ? ErrorCodes.UnknownError : code)
        {
            m_errorName = errorName;
        }

        public string InternalName
        {
            get { return m_errorName; }
        }
    }

    public class ClientConnectionError : Error
    {
        public ClientConnectionError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "No connection to internet", code == ErrorCodes.Unset ? ErrorCodes.ClientConnectionError : code)
        {
        }
    }

    public class InvalidServerResponseError : Error
    {
        public InvalidServerResponseError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Invalid response from server", code == ErrorCodes.Unset ? ErrorCodes.InvalidResponseError : code)
        {
        }
    }

    public class MaintenanceError : Error
    {
        public MaintenanceError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Server maintenance in progress", code == ErrorCodes.Unset ? ErrorCodes.MaintanenceError : code)
        {
        }
    }

    public class TimeoutError : Error
    {
        public TimeoutError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Timeout when connecting to server", code == ErrorCodes.Unset ? ErrorCodes.UnknownError : code)
        {
        }
    }

    public class AuthenticationError : Error
    {
        public AuthenticationError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Authentication error", code == ErrorCodes.Unset ? ErrorCodes.AuthError : code)
        {
        }
    }
    
    public class UserAuthError : Error
    {
        public UserAuthError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "User Auth Error", code == ErrorCodes.Unset ? ErrorCodes.UserAuthError : code)
        {
        }
    }

    public class PermissionError : Error
    {
        public PermissionError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Permission Error", code == ErrorCodes.Unset ? ErrorCodes.PermissionError : code)
        {
        }
    }

	public class FileNotFoundError : Error
	{
        public FileNotFoundError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "File not found", code == ErrorCodes.Unset ? ErrorCodes.FileNotFoundError : code)
		{
		}
	}
	
	public class FilePermissionError : Error
	{
        public FilePermissionError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "File permissions invalid", code == ErrorCodes.Unset ? ErrorCodes.FilePermissionError : code)
		{
		}
	}

    public class CorruptedFileError : Error
    {
        public CorruptedFileError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "File corrupted", code == ErrorCodes.Unset ? ErrorCodes.CorruptedFileError : code)
        {
        }
    }

    public class SyncError : Error
    {
        public SyncError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Synchronization Error", code == ErrorCodes.Unset ? ErrorCodes.UnknownError : code)
        {
        }
    }

    public class CompatibilityError : Error
    {
        private Dictionary<string, object> m_serverData = null;

        public Dictionary<string, object> serverData
        {
            get { return m_serverData; }
        }

        public CompatibilityError(string message = null, ErrorCodes code = ErrorCodes.Unset, Dictionary<string, object> serverData = null)
            : base(message != null ? message : "Compatibility Error", code == ErrorCodes.Unset ? ErrorCodes.CompatibilityError : code)
        {
            m_serverData = serverData;
        }
    }

    public class UploadDisallowedError : Error
    {
        public UploadDisallowedError(string message = null, ErrorCodes code = ErrorCodes.Unset)
            : base(message != null ? message : "Upload Disallowed Error", code == ErrorCodes.Unset ? ErrorCodes.UploadDisallowedError : code)
        {
        }
    }
    #endregion

    #region Exceptions
    public class CorruptedSaveException : Exception
    {
        public CorruptedSaveException(Exception innerException)
            : base("Save Corrupted", innerException)
        {
        }
    }
    #endregion
}
