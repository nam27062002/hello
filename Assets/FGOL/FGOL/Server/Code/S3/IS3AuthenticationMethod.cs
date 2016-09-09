using System;
using System.Collections;
using System.Collections.Generic;

namespace FGOL.Server
{
    public interface IS3AuthenticationMethod
    {
        string BuildAWSRequest(string bucket, string file, FGOL.Server.Request.Method method, Dictionary<string, string> awsCredentials, int unixTimestamp, int expiryTimestamp, out Hashtable headers);
    }
}
