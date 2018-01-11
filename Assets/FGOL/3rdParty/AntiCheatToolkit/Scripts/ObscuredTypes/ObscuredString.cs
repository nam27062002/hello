﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeStage.AntiCheat.ObscuredTypes
{
	/// <summary>
	/// Use it instead of regular <c>string</c> for any cheating-sensitive variables.
	/// </summary>
	/// <strong><em>Regular type is faster and memory wiser comparing to the obscured one!</em></strong>
	[Serializable]
	public sealed class ObscuredString
	{
		private static string cryptoKey = "4441";

#if UNITY_EDITOR
		// For internal Editor usage only (may be useful for drawers).
		public static string cryptoKeyEditor = cryptoKey;
#endif

		[SerializeField]
		private string currentCryptoKey;

		[SerializeField]
		private byte[] hiddenValue;

		[SerializeField]
		private string fakeValue;

		[SerializeField]
		private bool inited;

		// for serialization purposes
		private ObscuredString(){}

		private ObscuredString(byte[] value)
		{
			currentCryptoKey = cryptoKey;
			hiddenValue = value;
			fakeValue = null;
			inited = true;
		}

		/// <summary>
		/// Allows to change default crypto key of this type instances. All new instances will use specified key.<br/>
		/// All current instances will use previous key unless you call ApplyNewCryptoKey() on them explicitly.
		/// </summary>
		public static void SetNewCryptoKey(string newKey)
		{
			cryptoKey = newKey;
		}

		/// <summary>
		/// Simple symmetric encryption, uses default crypto key.
		/// </summary>
		/// <returns>Encrypted or decrypted <c>string</c> (depending on what <c>string</c> was passed to the function)</returns>
		public static string EncryptDecrypt(string value)
		{
			return EncryptDecrypt(value, "");
		}

		/// <summary>
		/// Simple symmetric encryption, uses passed crypto key.
		/// </summary>
		/// <returns>Encrypted or decrypted <c>string</c> (depending on what <c>string</c> was passed to the function)</returns>
		public static string EncryptDecrypt(string value, string key)
		{
			if (string.IsNullOrEmpty(value))
			{
				return "";
			}

			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}

			int keyLength = key.Length;
			int valueLength = value.Length;

			char[] result = new char[valueLength];

			for (int i = 0; i < valueLength; i++)
			{
				result[i] = (char)(value[i] ^ key[i % keyLength]);
			}

			return new string(result);
		}

		/// <summary>
		/// Use it after SetNewCryptoKey() to re-encrypt current instance using new crypto key.
		/// </summary>
		public void ApplyNewCryptoKey()
		{
			if (currentCryptoKey != cryptoKey)
			{
				hiddenValue = InternalEncrypt(InternalDecrypt());
				currentCryptoKey = cryptoKey;
			}
		}

		/// <summary>
		/// Allows to change current crypto key to the new random value and re-encrypt variable using it.
		/// Use it for extra protection against 'unknown value' search.
		/// Just call it sometimes when your variable doesn't change to fool the cheater.
		/// </summary>
		/// <strong>\htmlonly<font color="FF4040">WARNING:</font>\endhtmlonly produces some garbage, be careful when using it!</strong>
		public void RandomizeCryptoKey()
		{
			string decrypted = InternalDecrypt();

			currentCryptoKey = Random.Range(int.MinValue, int.MaxValue).ToString();
			hiddenValue = InternalEncrypt(decrypted, currentCryptoKey);
		}

		/// <summary>
		/// Allows to pick current obscured value as is.
		/// </summary>
		/// Use it in conjunction with SetEncrypted().<br/>
		/// Useful for saving data in obscured state.
		public string GetEncrypted()
		{
			ApplyNewCryptoKey();
			return GetString(hiddenValue);
		}

		/// <summary>
		/// Allows to explicitly set current obscured value.
		/// </summary>
		/// Use it in conjunction with GetEncrypted().<br/>
		/// Useful for loading data stored in obscured state.
		public void SetEncrypted(string encrypted)
		{
			inited = true;
			hiddenValue = GetBytes(encrypted);
			if (Detectors.ObscuredCheatingDetector.IsRunning)
			{
				fakeValue = InternalDecrypt();
			}
		}

		private static byte[] InternalEncrypt(string value)
		{
			return InternalEncrypt(value, cryptoKey);
		}

		private static byte[] InternalEncrypt(string value, string key)
		{
			return GetBytes(EncryptDecrypt(value, key));
		}

		private string InternalDecrypt()
		{
			if (!inited)
			{
				currentCryptoKey = cryptoKey;
				hiddenValue = InternalEncrypt("");
				fakeValue = "";
				inited = true;
			}

			string key = currentCryptoKey;
			if (string.IsNullOrEmpty(key))
			{
				key = cryptoKey;
			}

			string result = EncryptDecrypt(GetString(hiddenValue), key);

			if (Detectors.ObscuredCheatingDetector.IsRunning && !string.IsNullOrEmpty(fakeValue) && result != fakeValue)
			{
				Detectors.ObscuredCheatingDetector.Instance.OnCheatingDetected();
			}

			return result;
		}

		#region operators and overrides
		//! @cond
		public static implicit operator ObscuredString(string value)
		{
			if (value == null)
			{
				return null;
			}

			ObscuredString obscured = new ObscuredString(InternalEncrypt(value));
			if (Detectors.ObscuredCheatingDetector.IsRunning)
			{
				obscured.fakeValue = value;
			}
			return obscured;
		}

		public static implicit operator string(ObscuredString value)
		{
			if (value == null)
			{
				return null;
			}
			return value.InternalDecrypt();
		}

		/// <summary>
		/// Overrides default ToString to provide easy implicit conversion to the <c>string</c>.
		/// </summary>
		public override string ToString()
		{
			return InternalDecrypt();
		}

		/// <summary>
		/// Determines whether two specified ObscuredStrings have the same value.
		/// </summary>
		/// 
		/// <returns>
		/// true if the value of <paramref name="a"/> is the same as the value of <paramref name="b"/>; otherwise, false.
		/// </returns>
		/// <param name="a">An ObscuredString or null. </param><param name="b">An ObscuredString or null. </param><filterpriority>3</filterpriority>
		public static bool operator ==(ObscuredString a, ObscuredString b)
		{
			if (ReferenceEquals(a, b))
			{
				return true;
			}

			if ((object)a == null || (object)b == null)
			{
				return false;
			}

			if (a.currentCryptoKey == b.currentCryptoKey)
			{
				return ArraysEquals(a.hiddenValue, b.hiddenValue);
			}

			return string.Equals(a.InternalDecrypt(), b.InternalDecrypt());
		}

		/// <summary>
		/// Determines whether two specified ObscuredStrings have different values.
		/// </summary>
		/// 
		/// <returns>
		/// true if the value of <paramref name="a"/> is different from the value of <paramref name="b"/>; otherwise, false.
		/// </returns>
		/// <param name="a">An ObscuredString or null. </param><param name="b">An ObscuredString or null. </param><filterpriority>3</filterpriority>
		public static bool operator !=(ObscuredString a, ObscuredString b)
		{
			return !(a == b);
		}

		/// <summary>
		/// Determines whether this instance of ObscuredString and a specified object, which must also be a ObscuredString object, have the same value.
		/// </summary>
		/// 
		/// <returns>
		/// true if <paramref name="obj"/> is a ObscuredString and its value is the same as this instance; otherwise, false.
		/// </returns>
		/// <param name="obj">An <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (!(obj is ObscuredString))
				return false;

			return Equals((ObscuredString)obj);
		}

		/// <summary>
		/// Determines whether this instance and another specified ObscuredString object have the same value.
		/// </summary>
		/// 
		/// <returns>
		/// true if the value of the <paramref name="value"/> parameter is the same as this instance; otherwise, false.
		/// </returns>
		/// <param name="value">A ObscuredString. </param><filterpriority>2</filterpriority>
		public bool Equals(ObscuredString value)
		{
			if (value == null) return false;

			if (currentCryptoKey == value.currentCryptoKey)
			{
				return ArraysEquals(hiddenValue, value.hiddenValue);
			}

			return string.Equals(InternalDecrypt(), value.InternalDecrypt());
		}

		/// <summary>
		/// Determines whether this string and a specified ObscuredString object have the same value. A parameter specifies the culture, case, and sort rules used in the comparison.
		/// </summary>
		/// 
		/// <returns>
		/// true if the value of the <paramref name="value"/> parameter is the same as this string; otherwise, false.
		/// </returns>
		/// <param name="value">An ObscuredString to compare.</param><param name="comparisonType">A value that defines the type of comparison. </param><exception cref="T:System.ArgumentException"><paramref name="comparisonType"/> is not a <see cref="T:System.StringComparison"/> value. </exception><filterpriority>2</filterpriority>
		public bool Equals(ObscuredString value, StringComparison comparisonType)
		{
			if (value == null) return false;

			return string.Equals(InternalDecrypt(), value.InternalDecrypt(), comparisonType);
		}

		/// <summary>
		/// Returns the hash code for this ObscuredString.
		/// </summary>
		/// 
		/// <returns>
		/// A 32-bit signed integer hash code.
		/// </returns>
		public override int GetHashCode()
		{
			return InternalDecrypt().GetHashCode();
		}
		//! @endcond
		#endregion

		private static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		private static string GetString(byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		private static bool ArraysEquals(byte[] a1, byte[] a2)
		{
			if (a1 == a2)
			{
				return true;
			}

			if ((a1 != null) && (a2 != null))
			{
				if (a1.Length != a2.Length)
				{
					return false;
				}
				for (int i = 0; i < a1.Length; i++)
				{
					if (a1[i] != a2[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
	}
}