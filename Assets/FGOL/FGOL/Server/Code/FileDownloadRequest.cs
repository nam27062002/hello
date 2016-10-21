using BestHTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FGOL.Server
{
    class FileDownloadRequest : Request
    {
        public delegate void OnDownloadCompleteDG(Error error, byte[] fileData = null);

        public void Download(string fileUrl, OnDownloadCompleteDG callback)
        {
            Run(fileUrl, Method.GET, null, null, null, delegate(Error error, HTTPResponse response)
            {
                if(error == null)
                {
                    if(response != null)
                    {
                        switch(response.StatusCode)
                        {
                            case 200:
                                callback(null, response.Data);
                                break;
                            case 404:
                                callback(new FileNotFoundError("404 on url: " + fileUrl));
                                break;
                            default:
                                callback(new UnknownError(string.Format("Failed to download with status code {0} from url: {1}", response.StatusCode, fileUrl)));
                                break;
                        }
                    }
                    else
                    {
                        callback(new UnknownError("Failed to download from url: " + fileUrl));
                    }
                }
                else
                {
                    callback(error);
                }
            });
        }
    }
}
