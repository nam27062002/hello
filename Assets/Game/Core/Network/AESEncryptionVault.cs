using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;

public class AESEncryptionVault
{
	RijndaelManaged crypto;


	public AESEncryptionVault(string pass)
	{
		byte[] key = MD5Hash(pass);

		crypto = new RijndaelManaged();

		crypto.Key = key;
		crypto.IV = key;
		crypto.Mode = CipherMode.CBC;
		crypto.Padding = PaddingMode.PKCS7;
	}

	private byte[] MD5Hash(string pass)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
		
		//compute hash from the bytes of text
		md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
		
		//get hash result after compute it
		byte[] result = md5.Hash;

		return result;
	}

	public string Encrypt(string str)
	{
		ICryptoTransform encryptor = crypto.CreateEncryptor();
		byte[] plain = Encoding.UTF8.GetBytes(str);
		byte[] cipher = encryptor.TransformFinalBlock(plain, 0, plain.Length);

		return Convert.ToBase64String(cipher);
		//return Encoding.UTF8.GetString(cipher);
	}

	public string Decrypt(string str)
	{
		ICryptoTransform decryptor = crypto.CreateDecryptor(crypto.Key, crypto.IV);

		byte[] cipher = Convert.FromBase64String(str); //Encoding.UTF8.GetBytes(str);
		byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

		return Encoding.UTF8.GetString(plain);
	}
}
