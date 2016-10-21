using System.IO;
using System;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System.Collections.Generic;



namespace FGOL.Save
{
	public class SaveUtilities
	{
        private const int MD5HashLength = 32;
        private const int MaxDeviceNameLength = 1024;
        private const int MaxHeaderVersionLength = 10;

        /**
         * Serializes the header in the following format:
         *      headerLength
         *      timestamp
         *      deviceNameLength
         *      deviceName
         *      md5Hash
         *      contentLength
         */
		public static byte[] SerializeHeader(/*int modifiedTime, string deviceName,*/ string md5Hash, int contentLength)
		{
			bool isLittleEndian = BitConverter.IsLittleEndian;
		
			using (MemoryStream ms = new MemoryStream())
			{
                // [DGR] SAVE: Not supported yet
                /*
				byte[] timestampBytes = BitConverter.GetBytes(modifiedTime);
				
				//Variable length header so we need to write the length as well
                if(deviceName.Length > MaxDeviceNameLength)
                {
                    deviceName = deviceName.Substring(0, MaxDeviceNameLength - 3);
                    deviceName += "...";
                }                

				byte[] deviceNameBytes = Encoding.UTF8.GetBytes(deviceName);
				byte[] deviceNameLengthBytes = BitConverter.GetBytes(deviceNameBytes.Length);
                */

                byte[] md5HashBytes = Encoding.UTF8.GetBytes(md5Hash);
                byte[] contentLengthBytes = BitConverter.GetBytes(contentLength);

                // [DGR] SAVE: Not supported yet
                byte[] headerLengthBytes = BitConverter.GetBytes(/*timestampBytes.Length + deviceNameBytes.Length + deviceNameLengthBytes.Length +*/ MD5HashLength + contentLengthBytes.Length);
				
				if (!isLittleEndian)
				{
                    Array.Reverse(headerLengthBytes);

                    // [DGR] SAVE: Not supported yet
                    /*
                    Array.Reverse(timestampBytes);
					Array.Reverse(deviceNameLengthBytes);
                    Array.Reverse(deviceNameBytes);
                    */
                    Array.Reverse(md5HashBytes);
                    Array.Reverse(contentLengthBytes);
				}

                ms.Write(headerLengthBytes, 0, headerLengthBytes.Length);

                // [DGR] SAVE: Not supported yet
                /*
                ms.Write(timestampBytes, 0, timestampBytes.Length);
				ms.Write(deviceNameLengthBytes, 0, deviceNameLengthBytes.Length);
				ms.Write(deviceNameBytes, 0, deviceNameBytes.Length);
                */
                ms.Write(md5HashBytes, 0, md5HashBytes.Length);
                ms.Write(contentLengthBytes, 0, contentLengthBytes.Length);

				return ms.ToArray();
			}
		}

		public static bool DeserializeHeader(Stream stream, ref int headerLength, /*ref int modifiedTime, ref string deviceName,*/ ref string md5Hash, ref int contentLength)
		{
            bool validHeader = false;

			bool isLittleEndian = BitConverter.IsLittleEndian;

            byte[] headerLengthBytes = BitConverter.GetBytes(headerLength);
            stream.Read(headerLengthBytes, 0, headerLengthBytes.Length);
            if(!isLittleEndian) Array.Reverse(headerLengthBytes);
            headerLength = BitConverter.ToInt32(headerLengthBytes, 0);
		
            // [DGR] SAVE: Not supported yet
            /*
			byte[] timestampBytes = BitConverter.GetBytes(modifiedTime);
			stream.Read(timestampBytes, 0, timestampBytes.Length);
			if (!isLittleEndian) Array.Reverse(timestampBytes);
			modifiedTime = BitConverter.ToInt32(timestampBytes, 0);

			int deviceNameLength = 0;
			byte[] deviceNameLengthBytes = BitConverter.GetBytes(deviceNameLength);
			stream.Read(deviceNameLengthBytes, 0, deviceNameLengthBytes.Length);
			if (!isLittleEndian) Array.Reverse(deviceNameLengthBytes);
			deviceNameLength = BitConverter.ToInt32(deviceNameLengthBytes, 0);

            if(deviceNameLength <= MaxDeviceNameLength)
            */
            {
                /*
                byte[] deviceNameBytes = new byte[deviceNameLength];
                stream.Read(deviceNameBytes, 0, deviceNameLength);
                if(!isLittleEndian) Array.Reverse(deviceNameBytes);
                deviceName = Encoding.UTF8.GetString(deviceNameBytes);
                */

                byte[] md5HashBytes = new byte[MD5HashLength];
                stream.Read(md5HashBytes, 0, md5HashBytes.Length);
                if(!isLittleEndian) Array.Reverse(md5HashBytes);
                md5Hash = Encoding.UTF8.GetString(md5HashBytes);
               
                byte[] contentLengthBytes = BitConverter.GetBytes(contentLength);
                stream.Read(contentLengthBytes, 0, contentLengthBytes.Length);
                if(!isLittleEndian) Array.Reverse(contentLengthBytes);
                contentLength = BitConverter.ToInt32(contentLengthBytes, 0);

                validHeader = true;
            }

            return validHeader;
		}
		
		public static int DeserializeVersion(Stream stream)
		{
            bool isLittleEndian = BitConverter.IsLittleEndian;



            int version = -1;
			
			byte[] versionBytes = BitConverter.GetBytes(version);
			stream.Read(versionBytes, 0, versionBytes.Length);
			if (!isLittleEndian) Array.Reverse(versionBytes);

            version = BitConverter.ToInt32(versionBytes, 0);

            int versionStringLength = 0;

            byte[] versionStringLengthBytes = BitConverter.GetBytes(versionStringLength);

            stream.Read(versionStringLengthBytes, 0, versionStringLengthBytes.Length);

            if (!isLittleEndian) Array.Reverse(versionStringLengthBytes);

            versionStringLength = BitConverter.ToInt32(versionStringLengthBytes, 0);



            try

            {

                if (versionStringLength < MaxHeaderVersionLength)

                {

                    byte[] versionStringBytes = new byte[versionStringLength];

                    stream.Read(versionStringBytes, 0, versionStringLength);

                    if (!isLittleEndian) Array.Reverse(versionStringBytes);

                    string versionString = Encoding.UTF8.GetString(versionStringBytes);

                    if (version != Convert.ToInt32(versionString))

                    {

                        throw new Exception();

                    }

                }
                else

                {

                    throw new Exception();

                }

            }

            catch (Exception)

            {

                version = -1;

            }

            return version;
		}

        public static byte[] SerializeVersion(int version)
        {
            bool isLittleEndian = BitConverter.IsLittleEndian;

            List<byte> bytes = new List<byte>();

            byte[] versionBytes = BitConverter.GetBytes(version);
            if(!isLittleEndian)
            {
                Array.Reverse(versionBytes);
            }

            bytes.AddRange(versionBytes);

            string versionString = version.ToString();

            byte[] versionStringBytes = Encoding.UTF8.GetBytes(versionString);

            byte[] versionStringLengthBytes = BitConverter.GetBytes(versionStringBytes.Length);



            if (!isLittleEndian)

            {

                Array.Reverse(versionStringLengthBytes);

                Array.Reverse(versionStringBytes);

            }



            bytes.AddRange(versionStringLengthBytes);
            bytes.AddRange(versionStringBytes);

            return bytes.ToArray();
        }

        public static MemoryStream GetUnpaddedSaveData(string savePath)
        {
            using(FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read))
            {
                //We need to remove any padding on the file
                int dataLength = 0;
                byte[] lengthBytes = BitConverter.GetBytes(dataLength);
                fs.Read(lengthBytes, 0, lengthBytes.Length);
                dataLength = BitConverter.ToInt32(lengthBytes, 0);

                MemoryStream ms = new MemoryStream();

                byte[] buffer = new byte[1024];

                int read = 0;

                while((read = fs.Read(buffer, 0, Math.Min(dataLength, buffer.Length))) > 0 && dataLength > 0)
                {
                    dataLength -= read;
                    ms.Write(buffer, 0, read);
                }

                ms.Seek(0, SeekOrigin.Begin);

                return ms;
            }
        }

		public static string GetSavePath(string userID)
		{            
			return Path.Combine(FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath(), userID + ".sav");
		}

        public static string CalculateMD5Hash(byte[] input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(input);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
	}
}