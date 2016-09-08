using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace FGOL.Server
{
    public class S3AuthenticationV2 : IS3AuthenticationMethod
    {
        public string BuildAWSRequest(string bucket, string file, FGOL.Server.Request.Method method, Dictionary<string, string> awsCredentials, int unixTimestamp, int expiryTimestamp, out Hashtable headers)
        {
            System.DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimestamp);

            headers = new Hashtable();
            headers.Add("x-amz-date", dateTime.ToString("yyyyMMddTHHmmssZ"));

            if (awsCredentials.ContainsKey("SessionToken") && awsCredentials["SessionToken"] != "")
            {
                headers.Add("x-amz-security-token", awsCredentials["SessionToken"]);
            }

            string expires = expiryTimestamp.ToString();

            // Build request
            string request = "/" + file;

            // Generate signature
            string signature = SignAWSRequest(awsCredentials["SecretAccessKey"], method.ToString(), bucket, request, expires, headers);

            // Build final url
            return string.Format(awsCredentials["BaseURL"], bucket) + request +
                "?AWSAccessKeyId=" + WWW.EscapeURL(awsCredentials["AccessKeyId"]) +
                "&Signature=" + WWW.EscapeURL(signature) +
                "&Expires=" + expires;
        }

        private string CanonicalizeHashtable(Hashtable table)
        {

            List<string> strList = new List<string>(table.Count);
            foreach (string key in table.Keys)
            {
                strList.Add(key);
            }
            strList.Sort(StringComparer.Ordinal);

            string ret = "";
            foreach (string str in strList)
            {
                ret += str.ToLower() + ":" + table[str] + "\n";
            }

            return ret;
        }

        private string SignAWSRequest(string secretKey, string HTTPVerb, string bucket, string request, string date, Hashtable headers, Hashtable resource = null, string contentType = null, string contentMD5 = null)
        {
            // Build sign string
            string str2Sign = "";
            str2Sign += HTTPVerb + "\n";
            str2Sign += contentMD5 + "\n";
            str2Sign += contentType + "\n";
            str2Sign += date == null ? DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss +0000") + "\n" : date + "\n";

            if (headers != null)
            {
                str2Sign += CanonicalizeHashtable(headers);
            }

            if (resource != null)
            {
                str2Sign += CanonicalizeHashtable(resource);
            }

            str2Sign += "/" + bucket + request;

            HMACSHA1 algorithm = new HMACSHA1(System.Text.Encoding.UTF8.GetBytes(secretKey));
            return System.Convert.ToBase64String(algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str2Sign)));
        }
    }
}
