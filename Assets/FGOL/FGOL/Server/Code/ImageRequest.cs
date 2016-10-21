using BestHTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FGOL.Server
{
    class ImageRequest : Request
    {
        public void Get(string imageUrl, Action<Error, Texture2D> callback)
        {
            Run(imageUrl, Method.GET, null, null, null, delegate(Error error, HTTPResponse response)
            {
                if(error == null)
                {
                    if(response != null)
                    {
                        switch(response.StatusCode)
                        {
                            case 200:
                                callback(null, response.DataAsTexture2D);
                                break;
                            case 404:
                                callback(new FileNotFoundError("404 on url: " + imageUrl), null);
                                break;
                            default:
                                callback(new UnknownError(string.Format("Failed to download with status code {0} from url: {1}", response.StatusCode, imageUrl)), null);
                                break;
                        }
                    }
                    else
                    {
                        callback(new UnknownError("Failed to download from url: " + imageUrl), null);
                    }
                }
                else
                {
                    callback(error, null);
                }
            });
        }
    }
}
