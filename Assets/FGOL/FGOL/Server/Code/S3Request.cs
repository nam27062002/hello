using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Xml;
using System.IO;
using BestHTTP;
using FGOL.Configuration;
using BestHTTP.JSON;

namespace FGOL.Server
{
    class S3Request : Request
    {
		public const string S3BaseURL = "https://{0}.s3.amazonaws.com/{1}";
		public const string S3BaseURLChina = "https://{0}.s3.cn-north-1.amazonaws.com.cn/{1}";

		public delegate void OnS3DownloadCompleteDG(Error error, byte[] data = null);

        private IS3AuthenticationMethod m_authMethod = null;

        public S3Request()
        {
#if S3_AUTH_V2
            m_authMethod = new S3AuthenticationV2();
#else
            m_authMethod = new S3AuthenticationV4();
#endif
        }

		public static string GetGeoFormattedURL()
		{
			// China url format:
			// https://{0}.s3.cn-north-1.amazonaws.com.cn/{1}
			// 0 = bucket
			// 1 = path to file
			if(GeoLocation.location == GeoLocation.Location.China)
			{
				return S3BaseURLChina;
			}
			else
			{
				// US url format
				// https://{0}.s3.amazonaws.com/{1}
				// 0 = bucket
				// 1 = path to file
				return S3BaseURL;
			}
		}

		public void DownloadFile(string bucket, string file, Dictionary<string, string> awsCredentials, int unixTimestamp, int expiryTimestamp, OnS3DownloadCompleteDG callback)
        {
            Hashtable headers = null;

            string url = m_authMethod.BuildAWSRequest(bucket, file, Method.GET, awsCredentials, unixTimestamp, expiryTimestamp, out headers);

            Run(url, Method.GET, headers, null, null, delegate(Error error, HTTPResponse response)
            {
                bool rawOutput = Convert.ToBoolean(Config.Instance["request.rawOutput"]);

                if (rawOutput)
                {
                    Debug.Log(string.Format("FGOL.Server.S3Request :: Response for url: {0}\nResponse: {1}", url, response != null ? response.DataAsText : null));
                }

                if (error == null)
                {
                    //With request determine if we have successfully downloaded file
                    //403 is forbidden, AWS credentials wrong!
                    if(response != null)
                    {
                        switch(response.StatusCode)
                        {
                            case 200:
                                callback(null, response.Data);
                                break;
                            case 403:
                                try
                                {
                                    using(XmlReader reader = XmlReader.Create(new StringReader(response.DataAsText)))
                                    {
                                        reader.ReadToFollowing("Code");
                                        string errorCode = reader.ReadElementContentAsString();

                                        //TODO may want to deal with ExpiredToken code and have client reauthenticate? 
                                        switch (errorCode)
                                        {
                                            case "AccessDenied":
                                                //TODO We are assuming this is because the file does not exist (S3 file exist leak prevention) but it may be due to actually not having the correct permissions
                                                callback(new FileNotFoundError("File not found on S3"));
                                                break;
                                            case "InvalidSecurity":
                                                callback(new AuthenticationError("Invalid AWS credentials used!"));
                                                break;
                                            default:
                                                callback(new UnknownError("Failed to downloaded file from S3 with error code: " + errorCode));
                                                break;
                                        }
                                    }
                                }
                                catch(System.Exception e)
                                {
                                    callback(new UnknownError("Failed to downloaded file from S3 with exception: " + e.Message));
                                }
                                break;
                            case 404:
                                callback(new FileNotFoundError("File not found on S3"));
                                break;
                            default:
                                callback(new UnknownError("Failed to downloaded file from S3 with status code: " + response.DataAsText));
                                break;
                        }
                    }
                    else
                    {
                        callback(new ServerConnectionError("Failed to downloaded file from S3 with empty response"));
                    }
                }
                else
                {
                    callback(error);
                }
            });
        }

		public delegate void OnS3UploadCompleteDG(Error error);

		public void UploadFile(string bucket, string file, Dictionary<string, string> awsCredentials, byte[] data, int unixTimestamp, int expiryTimestamp, OnS3UploadCompleteDG callback)
		{
			Hashtable headers = null;

            string url = m_authMethod.BuildAWSRequest(bucket, file, Method.PUT, awsCredentials, unixTimestamp, expiryTimestamp, out headers);

			Run(url, Method.PUT, headers, null, data, delegate(Error error, HTTPResponse response)
            {
                bool rawOutput = Convert.ToBoolean(Config.Instance["request.rawOutput"]);

                if (rawOutput)
                {
                    Debug.Log(string.Format("FGOL.Server.S3Request :: Response for url: {0}\nResponse: {1}", url, response != null ? response.DataAsText : null));
                }

                if (error == null)
                {
                    //With request determine if we have successfully uploaded file
                    //403 is forbidden, AWS credentials wrong!
                    if(response != null)
                    {
                        switch(response.StatusCode)
                        {
                            case 200:
                                callback(null);
                                break;
                            case 403:
                                try
                                {
                                    using(XmlReader reader = XmlReader.Create(new StringReader(response.DataAsText)))
                                    {
                                        reader.ReadToFollowing("Code");
                                        string errorCode = reader.ReadElementContentAsString();

                                        switch(errorCode)
                                        {
                                            case "AccessDenied":
                                                callback(new FilePermissionError("No permission to upload to S3"));
                                                break;
                                            case "InvalidSecurity":
                                                callback(new AuthenticationError("Invalid AWS credentials used!"));
                                                break;
                                            default:
                                                callback(new UnknownError("Failed to upload file to S3"));
                                                break;
                                        }
                                    }
                                }
                                catch(System.Exception e)
                                {
                                    callback(new UnknownError("Failed to downloaded file from S3 - " + e.Message));
                                }
                                break;
                            default:
                                callback(new UnknownError("Failed to upload file to S3 with status code: " + response.DataAsText));
                                break;
                        }
                    }
                    else
                    {
                        callback(new ServerConnectionError("Failed to upload file to S3 with empty response"));
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