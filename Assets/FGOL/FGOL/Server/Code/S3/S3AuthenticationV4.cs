using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

//REF: http://docs.aws.amazon.com/AmazonS3/latest/API/sigv4-auth-using-authorization-header.html

namespace FGOL.Server
{
    public class S3AuthenticationV4 : IS3AuthenticationMethod
    {
        public string BuildAWSRequest(string bucket, string file, Request.Method method, Dictionary<string, string> awsCredentials, int unixTimestamp, int expiryTimestamp, out Hashtable headers)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimestamp);
            string dateISO8601 = dateTime.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);

            int expiry = expiryTimestamp - unixTimestamp;

            Uri uri = new Uri(string.Format("{0}/{1}", string.Format(awsCredentials["BaseURL"], bucket), file));

            string scope = string.Format("{0}/{1}/s3/aws4_request", dateTime.ToString("yyyyMMdd"), awsCredentials["S3Region"]);

            headers = new Hashtable();

            headers.Add("Host", uri.Host);
            headers.Add("Content-Type", "application/octet-stream");

            if (awsCredentials.ContainsKey("SessionToken") && !string.IsNullOrEmpty(awsCredentials["SessionToken"]))
            {
                headers.Add("x-amz-security-token", awsCredentials["SessionToken"]);
            }

            headers.Add("x-amz-content-sha256", "UNSIGNED-PAYLOAD");
            headers.Add("x-amz-date", dateISO8601);
            headers.Add("x-amz-expires", expiry.ToString());

            string signedHeaders = null;
            string signature = GenerateSignature(uri.AbsolutePath, method, expiry, headers, dateTime, scope, awsCredentials["AccessKeyId"], awsCredentials["SecretAccessKey"], awsCredentials["S3Region"], out signedHeaders);

            headers.Add("Authorization", string.Format("AWS4-HMAC-SHA256 Credential={0},SignedHeaders={1},Signature={2}", string.Format("{0}/{1}", awsCredentials["AccessKeyId"], scope), signedHeaders, signature));

            return uri.OriginalString;
        }

        private string GetCanonicalHeaders(Hashtable headers, out List<string> signedHeaders)
        {
            List<string> canonicalHeaders = new List<string>();
            signedHeaders = new List<string>();

            Regex compressWhitespaceRegex = new Regex("\\s+");

            foreach (string key in headers.Keys)
            {
                string value = headers[key] as string;
                value = compressWhitespaceRegex.Replace(value, " ");

                canonicalHeaders.Add(string.Format("{0}:{1}\n", key.ToLower(), value.Trim()));
                signedHeaders.Add(key.ToLower());
            }

            canonicalHeaders.Sort(delegate (string a, string b){
                return string.Compare(a, b);
            });

            signedHeaders.Sort(delegate (string a, string b){
                return string.Compare(a, b);
            });

            return string.Join("", canonicalHeaders.ToArray());
        }

        private string GenerateCanonicalRequest(string resource, Request.Method method, string accessKey, string scope, int expiry, string dateISO8601, Hashtable headers, out string signedHeaders)
        {
            List<string> signedHeadersList = null;

            string canonicalHeaders = GetCanonicalHeaders(headers, out signedHeadersList);
            signedHeaders = string.Join(";", signedHeadersList.ToArray());

            List <string> canonicalValues = new List<string>
            {
                method.ToString(),
                URIEncode(resource, false),
                "",
                canonicalHeaders,
                signedHeaders,
                "UNSIGNED-PAYLOAD"
            };

            return string.Join("\n", canonicalValues.ToArray());
        }

        private string GenerateSignature(string resource, Request.Method method, int expiry, Hashtable headers, DateTime dateTime, string scope, string accessKey, string secretKey, string region, out string signedHeaders)
        {
            string dateISO8601 = dateTime.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);

            string canonicalRequest = GenerateCanonicalRequest(resource, method, accessKey, scope, expiry, dateISO8601, headers, out signedHeaders);

            List<string> stringsToSign = new List<string>
            {
                "AWS4-HMAC-SHA256",
                dateISO8601,
                scope,
                SHA256Hash(Encoding.UTF8.GetBytes(canonicalRequest))
            };

            string stringToSign = string.Join("\n", stringsToSign.ToArray());

            byte[] dateKey = SHA256HMAC(Encoding.UTF8.GetBytes("AWS4" + secretKey), dateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture));
            byte[] dateKeyRegion = SHA256HMAC(dateKey, region);
            byte[] dateKeyRegionService = SHA256HMAC(dateKeyRegion, "s3");
            byte[] signingKey = SHA256HMAC(dateKeyRegionService, "aws4_request");

            return ToHex(SHA256HMAC(signingKey, stringToSign));
        }

        private string URIEncode(string input, bool encodeSlash = true)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];

                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-' || ch == '~' || ch == '.')
                {
                    result.Append(ch);
                }
                else if (ch == '/')
                {
                    result.Append(encodeSlash ? "%2F" : ch.ToString());
                }
                else
                {
                    result.Append(string.Format("%{0}", ToHex(Encoding.UTF8.GetBytes("" + ch), false)));
                }
            }

            return result.ToString();
        }

        private string ToHex(byte[] value, bool lower = true)
        {
            string hex = "";

            foreach (byte x in value)
            {
                if (lower)
                {
                    hex += string.Format("{0:x2}", x);
                }
                else
                {
                    hex += string.Format("{0:X2}", x);
                }
            }

            return hex;
        }

        private string SHA256Hash(byte[] value)
        {
            HashAlgorithm sha256 = HashAlgorithm.Create("SHA-256");
            return ToHex(sha256.ComputeHash(value));
        }

        private byte[] SHA256HMAC(byte[] key, string value)
        {
            KeyedHashAlgorithm algorithm = KeyedHashAlgorithm.Create("HMACSHA256");
            algorithm.Key = key;
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
        }
    }
}
