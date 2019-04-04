//#define OLD_SIMPLEJSON

#if !OLD_SIMPLEJSON

//#define USE_SharpZipLib
#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif
/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
//#define USE_SharpZipLib
#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif
/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 *
 * Features / attributes:
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser now properly identifies types. So generating JSON with this framework should work.
 * - only double quotes (") are used for quoting strings.
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser tries to avoid errors, but if malformed JSON is parsed the result is more or less undefined
 * - It can serialize/deserialize a node tree into/from an experimental compact binary format. It might
 *   be handy if you want to store things in a file and don't want it to be easily modifiable
 * 
 * 
 * 2012-12-17 Update:
 * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * 2017-03-08 Update:
 * - Optimised parsing by using a StringBuilder for token. This prevents performance issues when large
 *   string data fields are contained in the json data.
 * - Finally refactored the badly named JSONClass into JSONClass (Calety change).
 * - Replaced the old JSONData class by distict typed classes ( JSONString, JSONNumber, JSONBool, JSONNull ) this
 *   allows to propertly convert the node tree back to json without type information loss. The actual value
 *   parsing now happens at parsing time and not when you actually access one of the casting properties.
 * 
 * 2017-04-11 Update:
 * - Fixed parsing bug where empty string values have been ignored.
 * - Optimised "ToString" by using a StringBuilder internally. This should heavily improve performance for large files
 * - Changed the overload of "ToString(string aIndent)" to "ToString(int aIndent)"
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2012-2017 Markus Göbel
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleJSON
{
	public enum JSONNodeType
	{
		Array = 1,
		Object = 2,
		String = 3,
		Number = 4,
		NullValue = 5,
		Boolean = 6,
		None = 7
	}
	public enum JSONTextMode
	{
		Compact,
		Indent
	}

	public abstract partial class JSONNode
	{
		#region common interface

		public virtual JSONNode this[int aIndex] { get { return null; } set { } }

		public virtual JSONNode this[string aKey] { get { return null; } set { } }

		public virtual string Value { get { return ""; } set { } }

		public virtual int Count { get { return 0; } }

		public virtual bool IsNumber { get { return false; } }
		public virtual bool IsString { get { return false; } }
		public virtual bool IsBoolean { get { return false; } }
		public virtual bool IsNull { get { return false; } }
		public virtual bool IsArray { get { return false; } }
		public virtual bool IsObject { get { return false; } }

		// CALETY ADDED
		private bool m_bDoubleParsed = false;
		private double m_fDoubleParsedValue;
		private bool m_bBoolParsed = false;
		private bool m_fBoolParsedValue;

		public virtual IEnumerable<JSONNode> Childs { get { yield break;} }
		public IEnumerable<JSONNode> DeepChilds
		{
			get
			{
				foreach (var C in Childs)
					foreach (var D in C.DeepChilds)
						yield return D;
			}
		}
		///////////////////////////////////////

		public virtual void Add(string aKey, JSONNode aItem)
		{
		}
		public virtual void Add(JSONNode aItem)
		{
			Add("", aItem);
		}

		public virtual JSONNode Remove(string aKey)
		{
			return null;
		}

		public virtual JSONNode Remove(int aIndex)
		{
			return null;
		}

		public virtual JSONNode Remove(JSONNode aNode)
		{
			return aNode;
		}

		public virtual IEnumerable<JSONNode> Children
		{
			get
			{
				yield break;
			}
		}

		public IEnumerable<JSONNode> DeepChildren
		{
			get
			{
				foreach (var C in Children)
					foreach (var D in C.DeepChildren)
						yield return D;
			}
		}

		// CALETY ADDED
		public virtual bool ContainsKey(string strKey)
		{
			return false;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			WriteToStringBuilder(sb, 0, 0, JSONTextMode.Compact);
			return sb.ToString();
		}

		public virtual string ToString(int aIndent)
		{
			StringBuilder sb = new StringBuilder();
			WriteToStringBuilder(sb, 0, aIndent, JSONTextMode.Indent);
			return sb.ToString();
		}
		internal abstract void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode);

		#endregion common interface

		#region typecasting properties

		public abstract JSONNodeType Tag { get; }

		public virtual double AsDouble
		{
			get
			{
				if (m_bDoubleParsed)
					return m_fDoubleParsedValue;

				double v = 0.0;
				if (double.TryParse (Value, out v))
				{
					m_fDoubleParsedValue = v;
					m_bDoubleParsed = true;

					return v;
				}
				return 0.0;
			}
			set
			{
				m_bDoubleParsed = false;
				Value = value.ToString();
			}
		}

		public virtual int AsInt
		{
			get { return (int)AsDouble; }
			set { AsDouble = value; }
		}

		// CALETY ADDED
		public virtual long AsLong
		{
			get { return (long)AsDouble; }
			set { AsDouble = value; }
		}

		public virtual float AsFloat
		{
			get { return (float)AsDouble; }
			set { AsDouble = value; }
		}

		public virtual bool AsBool
		{
			get
			{
				if (m_bBoolParsed)
					return m_fBoolParsedValue;
				
				bool v = false;
				if (bool.TryParse (Value, out v))
				{
					m_fBoolParsedValue = v;
					m_bBoolParsed = true;

					return v;
				}

				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				m_bBoolParsed = false;

				Value = (value) ? "true" : "false";
			}
		}

		public virtual JSONArray AsArray
		{
			get
			{
				return this as JSONArray;
			}
		}

		public virtual JSONClass AsObject
		{
			get
			{
				return this as JSONClass;
			}
		}


		#endregion typecasting properties

		#region operators

		public static implicit operator JSONNode(string s)
		{
			return new JSONString(s);
		}
		public static implicit operator string(JSONNode d)
		{
			return (d == null) ? null : d.Value;
		}

		public static implicit operator JSONNode(double n)
		{
			return new JSONNumber(n);
		}
		public static implicit operator double(JSONNode d)
		{
			return (d == null) ? 0 : d.AsDouble;
		}

		public static implicit operator JSONNode(float n)
		{
			return new JSONNumber(n);
		}
		public static implicit operator float(JSONNode d)
		{
			return (d == null) ? 0 : d.AsFloat;
		}

		public static implicit operator JSONNode(int n)
		{
			return new JSONNumber(n);
		}
		public static implicit operator int(JSONNode d)
		{
			return (d == null) ? 0 : d.AsInt;
		}

		public static implicit operator JSONNode(bool b)
		{
			return new JSONBool(b);
		}
		public static implicit operator bool(JSONNode d)
		{
			return (d == null) ? false : d.AsBool;
		}

		public static bool operator ==(JSONNode a, object b)
		{
			if (ReferenceEquals(a, b))
				return true;
			bool aIsNull = a is JSONNull || ReferenceEquals(a, null) || a is JSONLazyCreator;
			bool bIsNull = b is JSONNull || ReferenceEquals(b, null) || b is JSONLazyCreator;
			if (aIsNull && bIsNull)
				return true;
			return a.Equals(b);
		}

		public static bool operator !=(JSONNode a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion operators
		internal static StringBuilder m_EscapeBuilder = new StringBuilder();
		internal static string Escape(string aText)
		{
            if (aText == null)
                return null;

			m_EscapeBuilder.Length = 0;
			if (m_EscapeBuilder.Capacity < aText.Length + aText.Length / 10)
				m_EscapeBuilder.Capacity = aText.Length + aText.Length / 10;
			foreach (char c in aText)
			{
				switch (c)
				{
				case '\\':
					m_EscapeBuilder.Append("\\\\");
					break;
				case '\"':
					m_EscapeBuilder.Append("\\\"");
					break;
				case '\n':
					m_EscapeBuilder.Append("\\n");
					break;
				case '\r':
					m_EscapeBuilder.Append("\\r");
					break;
				case '\t':
					m_EscapeBuilder.Append("\\t");
					break;
				case '\b':
					m_EscapeBuilder.Append("\\b");
					break;
				case '\f':
					m_EscapeBuilder.Append("\\f");
					break;
				default:
					m_EscapeBuilder.Append(c);
					break;
				}
			}
			string result = m_EscapeBuilder.ToString();
			m_EscapeBuilder.Length = 0;
			return result;
		}

		static void ParseElement(JSONNode ctx, string token, string tokenName, bool quoted)
		{
			if (quoted)
			{
				ctx.Add(tokenName, token);
				return;
			}
			string tmp = token.ToLower();
			if (tmp == "false" || tmp == "true")
				ctx.Add(tokenName, tmp == "true");
			else if (tmp == "null")
				ctx.Add(tokenName, null);
			else
			{
				double val;
				if (double.TryParse(token, out val))
					ctx.Add(tokenName, val);
				else
					ctx.Add(tokenName, token);
			}
		}

		public static JSONNode Parse(string aJSON)
		{
			Stack<JSONNode> stack = new Stack<JSONNode>();
			JSONNode ctx = null;
			int i = 0;
			StringBuilder Token = new StringBuilder();
			string TokenName = "";
			bool QuoteMode = false;
			bool TokenIsQuoted = false;
			while (i < aJSON.Length)
			{
				switch (aJSON[i])
				{
				case '{':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}
					stack.Push(new JSONClass());
					if (ctx != null)
					{
						ctx.Add(TokenName, stack.Peek());
					}
					TokenName = "";
					Token.Length = 0;
					ctx = stack.Peek();
					break;

				case '[':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}

					stack.Push(new JSONArray());
					if (ctx != null)
					{
						ctx.Add(TokenName, stack.Peek());
					}
					TokenName = "";
					Token.Length = 0;
					ctx = stack.Peek();
					break;

				case '}':
				case ']':
					if (QuoteMode) {

						Token.Append (aJSON [i]);
						break;
					}
					if (stack.Count == 0) {
						/*string strExceptionMessage = "JSON Parse: Too many closing brackets in: \n\n" + aJSON + "\n\n";
						if (aJSON != null) {
							if (NetworkManager.SharedInstance.GetCryptoKey () != null) {
								strExceptionMessage += "AES Decrypt: " + NetworkManager.SharedInstance.GetCryptoKey ().Decrypt (aJSON) + "\n\n";
							}
							strExceptionMessage += "Simple Decrypt: " + NetworkManager.SharedInstance.SimpleStringDecrypt (aJSON) + "\n\n";
						}

						throw new Exception (strExceptionMessage);
						*/
					}

					stack.Pop();
					if (Token.Length > 0 || TokenIsQuoted)
					{
						ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
						TokenIsQuoted = false;
					}
					TokenName = "";
					Token.Length = 0;
					if (stack.Count > 0)
						ctx = stack.Peek();
					break;

				case ':':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}
					TokenName = Token.ToString();
					Token.Length = 0;
					TokenIsQuoted = false;
					break;

				case '"':
					QuoteMode ^= true;
					TokenIsQuoted |= QuoteMode;
					break;

				case ',':
					if (QuoteMode)
					{
						Token.Append(aJSON[i]);
						break;
					}
					if (Token.Length > 0 || TokenIsQuoted)
					{
						ParseElement(ctx, Token.ToString(), TokenName, TokenIsQuoted);
						TokenIsQuoted = false;
					}
					TokenName = "";
					Token.Length = 0;
					TokenIsQuoted = false;
					break;

				case '\r':
				case '\n':
					break;

				case ' ':
				case '\t':
					if (QuoteMode)
						Token.Append(aJSON[i]);
					break;

				case '\\':
					++i;
					if (QuoteMode)
					{
						char C = aJSON[i];
						switch (C)
						{
						case 't':
							Token.Append('\t');
							break;
						case 'r':
							Token.Append('\r');
							break;
						case 'n':
							Token.Append('\n');
							break;
						case 'b':
							Token.Append('\b');
							break;
						case 'f':
							Token.Append('\f');
							break;
						case 'u':
							{
								string s = aJSON.Substring(i + 1, 4);
								Token.Append((char)int.Parse(
									s,
									System.Globalization.NumberStyles.AllowHexSpecifier));
								i += 4;
								break;
							}
						default:
							Token.Append(C);
							break;
						}
					}
					break;

				default:
					Token.Append(aJSON[i]);
					break;
				}
				++i;
			}
			if (QuoteMode)
			{
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
			}
			return ctx;
		}

		public virtual void Serialize(System.IO.BinaryWriter aWriter)
		{
		}

		public void SaveToStream(System.IO.Stream aData)
		{
			var W = new System.IO.BinaryWriter(aData);
			Serialize(W);
		}

		#if USE_SharpZipLib
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
		using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
		{
		gzipOut.IsStreamOwner = false;
		SaveToStream(gzipOut);
		gzipOut.Close();
		}
		}

		public void SaveToCompressedFile(string aFileName)
		{

		#if USE_FileIO
		System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
		using(var F = System.IO.File.OpenWrite(aFileName))
		{
		SaveToCompressedStream(F);
		}

		#else
		throw new Exception("Can't use File IO stuff in the webplayer");
		#endif
		}
		public string SaveToCompressedBase64()
		{
		using (var stream = new System.IO.MemoryStream())
		{
		SaveToCompressedStream(stream);
		stream.Position = 0;
		return System.Convert.ToBase64String(stream.ToArray());
		}
		}

		#else
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public void SaveToCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public string SaveToCompressedBase64()
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		#endif

		public void SaveToFile(string aFileName)
		{
			#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using (var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToStream(F);
			}
			#else
			throw new Exception ("Can't use File IO stuff in the webplayer");
			#endif
		}

		public string SaveToBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}

		public static JSONNode Deserialize(System.IO.BinaryReader aReader)
		{
			JSONNodeType type = (JSONNodeType)aReader.ReadByte();
			switch (type)
			{
			case JSONNodeType.Array:
				{
					int count = aReader.ReadInt32();
					JSONArray tmp = new JSONArray();
					for (int i = 0; i < count; i++)
						tmp.Add(Deserialize(aReader));
					return tmp;
				}
			case JSONNodeType.Object:
				{
					int count = aReader.ReadInt32();
					JSONClass tmp = new JSONClass();
					for (int i = 0; i < count; i++)
					{
						string key = aReader.ReadString();
						var val = Deserialize(aReader);
						tmp.Add(key, val);
					}
					return tmp;
				}
			case JSONNodeType.String:
				{
					return new JSONString(aReader.ReadString());
				}
			case JSONNodeType.Number:
				{
					return new JSONNumber(aReader.ReadDouble());
				}
			case JSONNodeType.Boolean:
				{
					return new JSONBool(aReader.ReadBoolean());
				}
			case JSONNodeType.NullValue:
				{
					return new JSONNull();
				}
			default:
				{
					throw new Exception("Error deserializing JSON. Unknown tag: " + type);
				}
			}
		}

		#if USE_SharpZipLib
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
		var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
		return LoadFromStream(zin);
		}
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
		#if USE_FileIO
		using(var F = System.IO.File.OpenRead(aFileName))
		{
		return LoadFromCompressedStream(F);
		}
		#else
		throw new Exception("Can't use File IO stuff in the webplayer");
		#endif
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
		var tmp = System.Convert.FromBase64String(aBase64);
		var stream = new System.IO.MemoryStream(tmp);
		stream.Position = 0;
		return LoadFromCompressedStream(stream);
		}
		#else
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		#endif

		public static JSONNode LoadFromStream(System.IO.Stream aData)
		{
			using (var R = new System.IO.BinaryReader(aData))
			{
				return Deserialize(R);
			}
		}

		public static JSONNode LoadFromFile(string aFileName)
		{
			#if USE_FileIO
			using (var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromStream(F);
			}
			#else
			throw new Exception ("Can't use File IO stuff in the webplayer");
			#endif
		}

		public static JSONNode LoadFromBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromStream(stream);
		}
	}
	// End of JSONNode

	public class JSONArray : JSONNode, IEnumerable
	{
		private List<JSONNode> m_List = new List<JSONNode>();
		public bool inline = false;

		public override JSONNodeType Tag { get { return JSONNodeType.Array; } }
		public override bool IsArray { get { return true; } }

		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= m_List.Count)
					return new JSONLazyCreator(this);
				return m_List[aIndex];
			}
			set
			{
				if (value == null)
					value = new JSONNull();
				if (aIndex < 0 || aIndex >= m_List.Count)
					m_List.Add(value);
				else
					m_List[aIndex] = value;
			}
		}

		public override JSONNode this[string aKey]
		{
			get { return new JSONLazyCreator(this); }
			set
			{
				if (value == null)
					value = new JSONNull();
				m_List.Add(value);
			}
		}

		public override int Count
		{
			get { return m_List.Count; }
		}

		// ETC ADDED
		public bool ContainsValue(string strValue) {
			for (int i = 0; i < m_List.Count; ++i) {
				if(m_List[i].Value == strValue) {
					return true;
				}
			}
			return false;
		}

		// CALETY ADDED
		public override bool ContainsKey(string strKey)
		{
			return false;
		}

		// CALETY ADDED
		public override IEnumerable<JSONNode> Childs
		{
			get
			{
				foreach(JSONNode N in m_List)
					yield return N;
			}
		}

		public override void Add(string aKey, JSONNode aItem)
		{
			if (aItem == null)
				aItem = new JSONNull();
			m_List.Add(aItem);
		}

		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
				return null;
			JSONNode tmp = m_List[aIndex];
			m_List.RemoveAt(aIndex);
			return tmp;
		}

		public override JSONNode Remove(JSONNode aNode)
		{
			m_List.Remove(aNode);
			return aNode;
		}

		public override IEnumerable<JSONNode> Children
		{
			get
			{
				foreach (JSONNode N in m_List)
					yield return N;
			}
		}

		public IEnumerator GetEnumerator()
		{
			foreach (JSONNode N in m_List)
				yield return N;
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONNodeType.Array);
			aWriter.Write(m_List.Count);
			for (int i = 0; i < m_List.Count; i++)
			{
				m_List[i].Serialize(aWriter);
			}
		}

		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append('[');
			int count = m_List.Count;
			if (inline)
				aMode = JSONTextMode.Compact;
			for (int i = 0; i < count; i++)
			{
				if (i > 0)
					aSB.Append(',');
				if (aMode == JSONTextMode.Indent)
					aSB.AppendLine();

				if (aMode == JSONTextMode.Indent)
					aSB.Append(' ', aIndent + aIndentInc);
				m_List[i].WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
			}
			if (aMode == JSONTextMode.Indent)
				aSB.AppendLine().Append(' ', aIndent);
			aSB.Append(']');
		}
	}
	// End of JSONArray

	public class JSONClass : JSONNode, IEnumerable
	{
		// CALETY ADDED
		// Change private to public
		public Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();

		public bool inline = false;

		public override JSONNodeType Tag { get { return JSONNodeType.Object; } }
		public override bool IsObject { get { return true; } }


		public override JSONNode this[string aKey]
		{
			get
			{
				if (m_Dict.ContainsKey(aKey))
					return m_Dict[aKey];
				else
					return new JSONLazyCreator(this, aKey);
			}
			set
			{
				if (value == null)
					value = new JSONNull();
				if (m_Dict.ContainsKey(aKey))
					m_Dict[aKey] = value;
				else
					m_Dict.Add(aKey, value);
			}
		}

		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return null;
				return m_Dict.ElementAt(aIndex).Value;
			}
			set
			{
				if (value == null)
					value = new JSONNull();
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return;
				string key = m_Dict.ElementAt(aIndex).Key;
				m_Dict[key] = value;
			}
		}

		public override int Count
		{
			get { return m_Dict.Count; }
		}

		// CALETY ADDED
		public override bool ContainsKey(string strKey)
		{
			return m_Dict.ContainsKey(strKey);
		}

		// CALETY ADDED
		public override IEnumerable<JSONNode> Childs
		{
			get
			{
				foreach(KeyValuePair<string,JSONNode> N in m_Dict)
					yield return N.Value;
			}
		}

		public override void Add(string aKey, JSONNode aItem)
		{
			if (aItem == null)
				aItem = new JSONNull();

			if (!string.IsNullOrEmpty(aKey))
			{
				if (m_Dict.ContainsKey(aKey))
					m_Dict[aKey] = aItem;
				else
					m_Dict.Add(aKey, aItem);
			}
			else
				m_Dict.Add(Guid.NewGuid().ToString(), aItem);
		}

		public override JSONNode Remove(string aKey)
		{
			if (!m_Dict.ContainsKey(aKey))
				return null;
			JSONNode tmp = m_Dict[aKey];
			m_Dict.Remove(aKey);
			return tmp;
		}

		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_Dict.Count)
				return null;
			var item = m_Dict.ElementAt(aIndex);
			m_Dict.Remove(item.Key);
			return item.Value;
		}

		public override JSONNode Remove(JSONNode aNode)
		{
			try
			{
				var item = m_Dict.Where(k => k.Value == aNode).First();
				m_Dict.Remove(item.Key);
				return aNode;
			}
			catch
			{
				return null;
			}
		}

		public override IEnumerable<JSONNode> Children
		{
			get
			{
				foreach (KeyValuePair<string, JSONNode> N in m_Dict)
					yield return N.Value;
			}
		}

		// CALETY ADDED
		public ArrayList GetKeys() // The method is named "GetKeys()"
		{
			ArrayList arrayOfStrings = new ArrayList(); // declares new array

			foreach (KeyValuePair<string, JSONNode> N in m_Dict) // for each key/values
				arrayOfStrings.Add(N.Key); // I add only the keys

			return arrayOfStrings; // And then I get them all :D
		}

		public IEnumerator GetEnumerator()
		{
			foreach (KeyValuePair<string, JSONNode> N in m_Dict)
				yield return N;
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONNodeType.Object);
			aWriter.Write(m_Dict.Count);
			foreach (string K in m_Dict.Keys)
			{
				aWriter.Write(K);
				m_Dict[K].Serialize(aWriter);
			}
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append('{');
			bool first = true;
			if (inline)
				aMode = JSONTextMode.Compact;
			foreach (var k in m_Dict)
			{
				if (!first)
					aSB.Append(',');
				first = false;
				if (aMode == JSONTextMode.Indent)
					aSB.AppendLine();
				if (aMode == JSONTextMode.Indent)
					aSB.Append(' ', aIndent + aIndentInc);
				aSB.Append('\"').Append(Escape(k.Key)).Append('\"');
				if (aMode == JSONTextMode.Compact)
					aSB.Append(':');
				else
					aSB.Append(" : ");
				k.Value.WriteToStringBuilder(aSB, aIndent + aIndentInc, aIndentInc, aMode);
			}
			if (aMode == JSONTextMode.Indent)
				aSB.AppendLine().Append(' ', aIndent);
			aSB.Append('}');
		}

	}
	// End of JSONClass

	public class JSONString : JSONNode
	{
		public string m_Data;

		public override JSONNodeType Tag { get { return JSONNodeType.String; } }
		public override bool IsString { get { return true; } }

		public override string Value
		{
			get { return m_Data; }
			set
			{
				m_Data = value;
			}
		}

		public JSONString(string aData)
		{
			m_Data = aData;
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONNodeType.String);
			aWriter.Write(m_Data);
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append('\"').Append(Escape(m_Data)).Append('\"');
		}
		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
				return true;
			string s = obj as string;
			if (s != null)
				return m_Data == s;
			JSONString s2 = obj as JSONString;
			if (s2 != null)
				return m_Data == s2.m_Data;
			return false;
		}
		public override int GetHashCode()
		{
			return m_Data.GetHashCode();
		}
	}
	// End of JSONString

	public class JSONNumber : JSONNode
	{
		public double m_Data;

		public override JSONNodeType Tag { get { return JSONNodeType.Number; } }
		public override bool IsNumber { get { return true; } }


		public override string Value
		{
			get { return m_Data.ToString(); }
			set
			{
				double v;
				if (double.TryParse(value, out v))
					m_Data = v;
			}
		}

		public override double AsDouble
		{
			get { return m_Data; }
			set { m_Data = value; }
		}

		public JSONNumber(double aData)
		{
			m_Data = aData;
		}

		public JSONNumber(string aData)
		{
			Value = aData;
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONNodeType.Number);
			aWriter.Write(m_Data);
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append(m_Data);
		}
		private static bool IsNumeric(object value)
		{
			return value is int || value is uint
				|| value is float || value is double
				|| value is decimal
				|| value is long || value is ulong
				|| value is short || value is ushort
				|| value is sbyte || value is byte;
		}
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (base.Equals(obj))
				return true;
			JSONNumber s2 = obj as JSONNumber;
			if (s2 != null)
				return m_Data == s2.m_Data;
			if (IsNumeric(obj))
				return Convert.ToDouble(obj) == m_Data;
			return false;
		}
		public override int GetHashCode()
		{
			return m_Data.GetHashCode();
		}
	}
	// End of JSONNumber

	public class JSONBool : JSONNode
	{
		public bool m_Data;

		public override JSONNodeType Tag { get { return JSONNodeType.Boolean; } }
		public override bool IsBoolean { get { return true; } }


		public override string Value
		{
			get { return m_Data.ToString(); }
			set
			{
				bool v;
				if (bool.TryParse(value, out v))
					m_Data = v;
			}
		}
		public override bool AsBool
		{
			get { return m_Data; }
			set { m_Data = value; }
		}

		public JSONBool(bool aData)
		{
			m_Data = aData;
		}

		public JSONBool(string aData)
		{
			Value = aData;
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONNodeType.Boolean);
			aWriter.Write(m_Data);
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append((m_Data) ? "true" : "false");
		}
		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (obj is bool)
				return m_Data == (bool)obj;
			return false;
		}
		public override int GetHashCode()
		{
			return m_Data.GetHashCode();
		}
	}
	// End of JSONBool

	public class JSONNull : JSONNode
	{

		public override JSONNodeType Tag { get { return JSONNodeType.NullValue; } }
		public override bool IsNull { get { return true; } }

		public override string Value
		{
			get { return "null"; }
			set { }
		}
		public override bool AsBool
		{
			get { return false; }
			set { }
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(this, obj))
				return true;
			return (obj is JSONNull);
		}
		public override int GetHashCode()
		{
			return 0;
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONNodeType.NullValue);
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append("null");
		}
	}
	// End of JSONNull

	internal class JSONLazyCreator : JSONNode
	{
		private JSONNode m_Node = null;
		private string m_Key = null;

		public override JSONNodeType Tag { get { return JSONNodeType.None; } }

		public JSONLazyCreator(JSONNode aNode)
		{
			m_Node = aNode;
			m_Key = null;
		}

		public JSONLazyCreator(JSONNode aNode, string aKey)
		{
			m_Node = aNode;
			m_Key = aKey;
		}

		private void Set(JSONNode aVal)
		{
			if (m_Key == null)
			{
				m_Node.Add(aVal);
			}
			else
			{
				m_Node.Add(m_Key, aVal);
			}
			m_Node = null; // Be GC friendly.
		}

		public override JSONNode this[int aIndex]
		{
			get
			{
				return new JSONLazyCreator(this);
			}
			set
			{
				var tmp = new JSONArray();
				tmp.Add(value);
				Set(tmp);
			}
		}

		public override JSONNode this[string aKey]
		{
			get
			{
				return new JSONLazyCreator(this, aKey);
			}
			set
			{
				var tmp = new JSONClass();
				tmp.Add(aKey, value);
				Set(tmp);
			}
		}

		public override void Add(JSONNode aItem)
		{
			var tmp = new JSONArray();
			tmp.Add(aItem);
			Set(tmp);
		}

		public override void Add(string aKey, JSONNode aItem)
		{
			var tmp = new JSONClass();
			tmp.Add(aKey, aItem);
			Set(tmp);
		}

		public static bool operator ==(JSONLazyCreator a, object b)
		{
			if (b == null)
				return true;
			return System.Object.ReferenceEquals(a, b);
		}

		public static bool operator !=(JSONLazyCreator a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return true;
			return System.Object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override int AsInt
		{
			get
			{
				JSONNumber tmp = new JSONNumber(0);
				Set(tmp);
				return 0;
			}
			set
			{
				JSONNumber tmp = new JSONNumber(value);
				Set(tmp);
			}
		}

		public override float AsFloat
		{
			get
			{
				JSONNumber tmp = new JSONNumber(0.0f);
				Set(tmp);
				return 0.0f;
			}
			set
			{
				JSONNumber tmp = new JSONNumber(value);
				Set(tmp);
			}
		}

		public override double AsDouble
		{
			get
			{
				JSONNumber tmp = new JSONNumber(0.0);
				Set(tmp);
				return 0.0;
			}
			set
			{
				JSONNumber tmp = new JSONNumber(value);
				Set(tmp);
			}
		}

		public override bool AsBool
		{
			get
			{
				JSONBool tmp = new JSONBool(false);
				Set(tmp);
				return false;
			}
			set
			{
				JSONBool tmp = new JSONBool(value);
				Set(tmp);
			}
		}

		public override JSONArray AsArray
		{
			get
			{
				JSONArray tmp = new JSONArray();
				Set(tmp);
				return tmp;
			}
		}

		public override JSONClass AsObject
		{
			get
			{
				JSONClass tmp = new JSONClass();
				Set(tmp);
				return tmp;
			}
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			aSB.Append("null");
		}
	}
	// End of JSONLazyCreator

	public static class JSON
	{
		public static JSONNode Parse(string aJSON)
		{
			return JSONNode.Parse(aJSON);
		}
	}

	// CALETY ADDED
	public class JSONData : JSONNode
	{
		private JSONNodeType m_kType;

		private JSONString m_kJSONString = null;
		private JSONNumber m_kJSONNumber = null;
		private JSONBool m_kJSONBool = null;
		//private JSONNull m_kJSONNull = null;

		public JSONData(string aData)
		{
			m_kJSONString = new JSONString (aData);

			m_kType = JSONNodeType.String;
		}
		public JSONData(float aData)
		{
			m_kJSONNumber = new JSONNumber (aData);

			m_kType = JSONNodeType.Number;
		}
		public JSONData(double aData)
		{
			m_kJSONNumber = new JSONNumber (aData);

			m_kType = JSONNodeType.Number;
		}
		public JSONData(bool aData)
		{
			m_kJSONBool = new JSONBool (aData);

			m_kType = JSONNodeType.Boolean;
		}
		public JSONData(int aData)
		{
			m_kJSONNumber = new JSONNumber (aData);

			m_kType = JSONNodeType.Number;
		}
		public JSONData(long aData)
		{
			m_kJSONNumber = new JSONNumber (aData);

			m_kType = JSONNodeType.Number;
		}
			
		public override JSONNodeType Tag { get { return m_kType; } }

		public override bool IsString { get { return (m_kJSONString != null); } }
		public override bool IsNumber { get { return (m_kJSONNumber != null); } }
		public override bool IsBoolean { get { return (m_kJSONBool != null); } }

		public override string Value
		{
			get
			{
				if (m_kType == JSONNodeType.String)
					return m_kJSONString.Value;
				else if (m_kType == JSONNodeType.Number)
					return m_kJSONNumber.Value;
				else if (m_kType == JSONNodeType.Boolean)
					return m_kJSONBool.Value;

				return "";
			}
			set
			{
				if (m_kType == JSONNodeType.String) {
					m_kJSONString.m_Data = value;
				} else if (m_kType == JSONNodeType.Number) {
					double v;
					if (double.TryParse(value, out v))
						m_kJSONNumber.m_Data = v;
				} else if (m_kType == JSONNodeType.Boolean) {
					bool v;
					if (bool.TryParse(value, out v))
						m_kJSONBool.m_Data = v;
				}
			}
		}

		public override double AsDouble
		{
			get
			{
				if (m_kType == JSONNodeType.Number)
				{
					return m_kJSONNumber.m_Data;
				}

				return 0;
			}
			set
			{
				if (m_kType == JSONNodeType.Number)
				{
					m_kJSONNumber.m_Data = value;
				}
			}
		}
		public override bool AsBool
		{
			get
			{
				if (m_kType == JSONNodeType.Boolean)
				{
					return m_kJSONBool.m_Data;
				}

				return false;
			}
			set
			{
				if (m_kType == JSONNodeType.Boolean)
				{
					m_kJSONBool.m_Data = value;
				}
			}
		}

		public override void Serialize(System.IO.BinaryWriter aWriter)
		{
			if (m_kType == JSONNodeType.String) {
				m_kJSONString.Serialize (aWriter);
			} else if (m_kType == JSONNodeType.Number) {
				m_kJSONNumber.Serialize (aWriter);
			} else if (m_kType == JSONNodeType.Boolean) {
				m_kJSONBool.Serialize (aWriter);
			}
		}
		internal override void WriteToStringBuilder(StringBuilder aSB, int aIndent, int aIndentInc, JSONTextMode aMode)
		{
			if (m_kType == JSONNodeType.String) {
				m_kJSONString.WriteToStringBuilder (aSB, aIndent, aIndentInc, aMode);
			} else if (m_kType == JSONNodeType.Number) {
				m_kJSONNumber.WriteToStringBuilder (aSB, aIndent, aIndentInc, aMode);
			} else if (m_kType == JSONNodeType.Boolean) {
				m_kJSONBool.WriteToStringBuilder (aSB, aIndent, aIndentInc, aMode);
			}
		}
		public override bool Equals(object obj)
		{
			if (m_kType == JSONNodeType.String) {
				return m_kJSONString.Equals (obj);
			} else if (m_kType == JSONNodeType.Number) {
				return m_kJSONNumber.Equals (obj);
			} else if (m_kType == JSONNodeType.Boolean) {
				return m_kJSONBool.Equals (obj);
			}
			
			return false;
		}

		public override int GetHashCode()
		{
			if (m_kType == JSONNodeType.String) {
				return m_kJSONString.GetHashCode ();
			} else if (m_kType == JSONNodeType.Number) {
				return m_kJSONNumber.GetHashCode ();
			} else if (m_kType == JSONNodeType.Boolean) {
				return m_kJSONBool.GetHashCode ();
			}

			return 0;
		}

	} // End of JSONData
}

#else

//#define USE_SharpZipLib
#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif

/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Features / attributes:
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser ignores data types. Each value is a string.
 * - only double quotes (") are used for quoting strings.
 * - values and names are not restricted to quoted strings. They simply add up and are trimmed.
 * - There are only 3 types: arrays(JSONArray), objects(JSONClass) and values(JSONData)
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser try to avoid errors, but if malformed JSON is parsed the result is undefined
 * 
 * 
 * 2012-12-17 Update:
 * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleJSON
{
	public enum JSONBinaryTag
	{
		Array               = 1,
		Class               = 2,
		Value               = 3,
		IntValue            = 4,
		DoubleValue         = 5,
		BoolValue           = 6,
		FloatValue          = 7,
		LongValue           = 8
	}

	public class JSONNode
	{
		#region common interface
		public virtual void Add(string aKey, JSONNode aItem){ }
		public virtual JSONNode this[int aIndex]   { get { return null; } set { } }
		public virtual JSONNode this[string aKey]  { get { return null; } set { } }
		public virtual string Value                { get { return "";   } set { } }
		public virtual int Count                   { get { return 0;    } }

		public virtual void Add(JSONNode aItem)
		{
			Add("", aItem);
		}
			
		public virtual bool ContainsKey(string strKey)
		{
			return false;
		}

		public virtual JSONNode Remove(string aKey) { return null; }
		public virtual JSONNode Remove(int aIndex) { return null; }
		public virtual JSONNode Remove(JSONNode aNode) { return aNode; }

		public virtual IEnumerable<JSONNode> Childs { get { yield break;} }
		public IEnumerable<JSONNode> DeepChilds
		{
			get
			{
				foreach (var C in Childs)
					foreach (var D in C.DeepChilds)
						yield return D;
			}
		}

		public override string ToString()
		{
			return "JSONNode";
		}
		public virtual string ToUnstyledString()
		{
			return "JSONNode";
		}
		public virtual string ToString(string aPrefix)
		{
			return "JSONNode";
		}

		#endregion common interface

		#region typecasting properties
		public virtual int AsInt
		{
			get
			{
				int v = 0;
				if (int.TryParse(Value,out v))
					return v;
				return 0;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual long AsLong
		{
			get
			{
				long v = 0;
				if (long.TryParse(Value,out v))
					return v;
				return 0;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual float AsFloat
		{
			get
			{
				float v = 0.0f;
				if (float.TryParse(Value,out v))
					return v;
				return 0.0f;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual double AsDouble
		{
			get
			{
				double v = 0.0;
				if (double.TryParse(Value,out v))
					return v;
				return 0.0;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual bool AsBool
		{
			get
			{
				bool v = false;
				if (bool.TryParse(Value,out v))
					return v;
				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				Value = (value)?"true":"false";
			}
		}
		public virtual JSONArray AsArray
		{
			get
			{
				return this as JSONArray;
			}
		}
		public virtual JSONClass AsObject
		{
			get
			{
				return this as JSONClass;
			}
			set
			{
				Value = value;
			}
		}


		#endregion typecasting properties

		#region operators
		public static implicit operator JSONNode(string s)
		{
			return new JSONData(s);
		}
		public static implicit operator string(JSONNode d)
		{
			return (d == null)?null:d.Value;
		}
		public static bool operator ==(JSONNode a, object b)
		{
			if (b == null && a is JSONLazyCreator)
				return true;
			return System.Object.ReferenceEquals(a,b);
		}

		public static bool operator !=(JSONNode a, object b)
		{
			return !(a == b);
		}
		public override bool Equals (object obj)
		{
			return System.Object.ReferenceEquals(this, obj);
		}
		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}


		#endregion operators

		internal static string Escape(string aText)
		{
			string result = "";
			foreach(char c in aText)
			{
				switch(c)
				{
				case '\\' : result += "\\\\"; break;
				case '\"' : result += "\\\""; break;
				case '\n' : result += "\\n" ; break;
				case '\r' : result += "\\r" ; break;
				case '\t' : result += "\\t" ; break;
				case '\b' : result += "\\b" ; break;
				case '\f' : result += "\\f" ; break;
				default   : result += c     ; break;
				}
			}
			return result;
		}

		public static JSONNode Parse(string aJSON)
		{
			Stack<JSONNode> stack = new Stack<JSONNode>();
			JSONNode ctx = null;
			int i = 0;
			string Token = "";
			string TokenName = "";
			bool QuoteMode = false;
			while (i < aJSON.Length)
			{
				switch (aJSON[i])
				{
				case '{':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					stack.Push(new JSONClass());
					if (ctx != null)
					{
						TokenName = TokenName.Trim();
						if (ctx is JSONArray)
							ctx.Add(stack.Peek());
						else if (TokenName != "")
							ctx.Add(TokenName,stack.Peek());
					}
					TokenName = "";
					Token = "";
					ctx = stack.Peek();
					break;

				case '[':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}

					stack.Push(new JSONArray());
					if (ctx != null)
					{
						TokenName = TokenName.Trim();
						if (ctx is JSONArray)
							ctx.Add(stack.Peek());
						else if (TokenName != "")
							ctx.Add(TokenName,stack.Peek());
					}
					TokenName = "";
					Token = "";
					ctx = stack.Peek();
					break;

				case '}':
				case ']':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					if (stack.Count == 0)
						throw new Exception("JSON Parse: Too many closing brackets");

					stack.Pop();
					if (Token != "")
					{
						TokenName = TokenName.Trim();
						if (ctx is JSONArray)
							ctx.Add(Token);
						else if (TokenName != "")
							ctx.Add(TokenName,Token);
					}
					TokenName = "";
					Token = "";
					if (stack.Count>0)
						ctx = stack.Peek();
					break;

				case ':':
				case '=':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					TokenName = Token;
					Token = "";
					break;

				case '"':
					QuoteMode ^= true;
					break;

				case ',':
				case ';':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					if (Token != "")
					{
						if (ctx is JSONArray)
							ctx.Add(Token);
						else if (TokenName != "")
							ctx.Add(TokenName, Token);
					}
					TokenName = "";
					Token = "";
					break;

				case '\r':
				case '\n':
					break;

				case ' ':
				case '\t':
					if (QuoteMode)
						Token += aJSON[i];
					break;

				case '\\':
					++i;
					if (QuoteMode)
					{
						char C = aJSON[i];
						switch (C)
						{
						case 't' : Token += '\t'; break;
						case 'r' : Token += '\r'; break;
						case 'n' : Token += '\n'; break;
						case 'b' : Token += '\b'; break;
						case 'f' : Token += '\f'; break;
						case 'u':
							{
								string s = aJSON.Substring(i+1,4);
								Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
								i += 4;
								break;
							}
						default  : Token += C; break;
						}
					}
					break;

				default:
					Token += aJSON[i];
					break;
				}
				++i;
			}
			if (QuoteMode)
			{
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
			}
			return ctx;
		}

		public virtual void Serialize(System.IO.BinaryWriter aWriter) {}

		public void SaveToStream(System.IO.Stream aData)
		{
			var W = new System.IO.BinaryWriter(aData);
			Serialize(W);
		}

#if USE_SharpZipLib
public void SaveToCompressedStream(System.IO.Stream aData)
{
using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
{
gzipOut.IsStreamOwner = false;
SaveToStream(gzipOut);
gzipOut.Close();
}
}

public void SaveToCompressedFile(string aFileName)
{
#if USE_FileIO
System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
using(var F = System.IO.File.OpenWrite(aFileName))
{
SaveToCompressedStream(F);
}
#else
throw new Exception("Can't use File IO stuff in webplayer");
#endif
}
public string SaveToCompressedBase64()
{
using (var stream = new System.IO.MemoryStream())
{
SaveToCompressedStream(stream);
stream.Position = 0;
return System.Convert.ToBase64String(stream.ToArray());
}
}

#else
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public void SaveToCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public string SaveToCompressedBase64()
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
#endif

		public void SaveToFile(string aFileName)
		{
#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToStream(F);
			}
#else
throw new Exception("Can't use File IO stuff in webplayer");
#endif
		}
		public string SaveToBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
		public static JSONNode Deserialize(System.IO.BinaryReader aReader)
		{
			JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
			switch(type)
			{
			case JSONBinaryTag.Array:
				{
					int count = aReader.ReadInt32();
					JSONArray tmp = new JSONArray();
					for(int i = 0; i < count; i++)
						tmp.Add(Deserialize(aReader));
					return tmp;
				}
			case JSONBinaryTag.Class:
				{
					int count = aReader.ReadInt32();                
					JSONClass tmp = new JSONClass();
					for(int i = 0; i < count; i++)
					{
						string key = aReader.ReadString();
						var val = Deserialize(aReader);
						tmp.Add(key, val);
					}
					return tmp;
				}
			case JSONBinaryTag.Value:
				{
					return new JSONData(aReader.ReadString());
				}
			case JSONBinaryTag.IntValue:
				{
					return new JSONData(aReader.ReadInt32());
				}
			case JSONBinaryTag.DoubleValue:
				{
					return new JSONData(aReader.ReadDouble());
				}
			case JSONBinaryTag.BoolValue:
				{
					return new JSONData(aReader.ReadBoolean());
				}
			case JSONBinaryTag.FloatValue:
				{
					return new JSONData(aReader.ReadSingle());
				}

			default:
				{
					throw new Exception("Error deserializing JSON. Unknown tag: " + type);
				}
			}
		}

#if USE_SharpZipLib
public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
{
var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
return LoadFromStream(zin);
}
public static JSONNode LoadFromCompressedFile(string aFileName)
{
#if USE_FileIO
using(var F = System.IO.File.OpenRead(aFileName))
{
return LoadFromCompressedStream(F);
}
#else
throw new Exception("Can't use File IO stuff in webplayer");
#endif
}
public static JSONNode LoadFromCompressedBase64(string aBase64)
{
var tmp = System.Convert.FromBase64String(aBase64);
var stream = new System.IO.MemoryStream(tmp);
stream.Position = 0;
return LoadFromCompressedStream(stream);
}
#else
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
#endif

		public static JSONNode LoadFromStream(System.IO.Stream aData)
		{
			using(var R = new System.IO.BinaryReader(aData))
			{
				return Deserialize(R);
			}
		}
		public static JSONNode LoadFromFile(string aFileName)
		{
#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromStream(F);
			}
#else
throw new Exception("Can't use File IO stuff in webplayer");
#endif
		}
		public static JSONNode LoadFromBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromStream(stream);
		}
	} // End of JSONNode

	public class JSONArray : JSONNode, IEnumerable
	{
		private List<JSONNode> m_List = new List<JSONNode>();
		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex<0 || aIndex >= m_List.Count)
					return new JSONLazyCreator(this);
				return m_List[aIndex];
			}
			set
			{
				if (aIndex<0 || aIndex >= m_List.Count)
					m_List.Add(value);
				else
					m_List[aIndex] = value;
			}
		}
		public override JSONNode this[string aKey]
		{
			get{ return new JSONLazyCreator(this);}
			set{ m_List.Add(value); }
		}
		public override int Count
		{
			get { return m_List.Count; }
		}
		public override void Add(string aKey, JSONNode aItem)
		{
			m_List.Add(aItem);
		}
		public override bool ContainsKey(string strKey)
		{
			return false;
		}
		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
				return null;
			JSONNode tmp = m_List[aIndex];
			m_List.RemoveAt(aIndex);
			return tmp;
		}
		public override JSONNode Remove(JSONNode aNode)
		{
			m_List.Remove(aNode);
			return aNode;
		}
		public override IEnumerable<JSONNode> Childs
		{
			get
			{
				foreach(JSONNode N in m_List)
					yield return N;
			}
		}
		public IEnumerator GetEnumerator()
		{
			foreach(JSONNode N in m_List)
				yield return N;
		}
		public override string ToString()
		{
			string result = "[ ";
			foreach (JSONNode N in m_List)
			{
				if (result.Length > 2)
					result += ", ";
				result += N.ToString();
			}
			result += " ]";
			return result;
		}
		public override string ToUnstyledString()
		{
			string result = "[ ";
			foreach (JSONNode N in m_List)
			{
				if (result.Length > 2)
					result += ", ";
				result += N.ToUnstyledString();
			}
			result += " ]";
			return result;
		}
		public override string ToString(string aPrefix)
		{
			string result = "[ ";
			foreach (JSONNode N in m_List)
			{
				if (result.Length > 3)
					result += ", ";
				result += "\n" + aPrefix + "   ";                
				result += N.ToString(aPrefix+"   ");
			}
			result += "\n" + aPrefix + "]";
			return result;
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONBinaryTag.Array);
			aWriter.Write(m_List.Count);
			for(int i = 0; i < m_List.Count; i++)
			{
				m_List[i].Serialize(aWriter);
			}
		}
	} // End of JSONArray

	public class JSONClass : JSONNode, IEnumerable
	{
		public Dictionary<string,JSONNode> m_Dict = new Dictionary<string,JSONNode>();
		public override JSONNode this[string aKey]
		{
			get
			{
				if (m_Dict.ContainsKey(aKey))
					return m_Dict[aKey];
				else
					return new JSONLazyCreator(this, aKey);
			}
			set
			{
				if (m_Dict.ContainsKey(aKey))
					m_Dict[aKey] = value;
				else
					m_Dict.Add(aKey,value);
			}
		}
		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return null;
				return m_Dict.ElementAt(aIndex).Value;
			}
			set
			{
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return;
				string key = m_Dict.ElementAt(aIndex).Key;
				m_Dict[key] = value;
			}
		}
		public override int Count
		{
			get { return m_Dict.Count; }
		}

		public override bool ContainsKey(string strKey)
		{
			return m_Dict.ContainsKey(strKey);
		}

		public ArrayList GetKeys() // The method is named "GetKeys()"
		{
			ArrayList arrayOfStrings = new ArrayList(); // declares new array

			foreach (KeyValuePair<string, JSONNode> N in m_Dict) // for each key/values
				arrayOfStrings.Add(N.Key); // I add only the keys

			return arrayOfStrings; // And then I get them all :D
		}

		public override void Add(string aKey, JSONNode aItem)
		{
			if (!string.IsNullOrEmpty(aKey))
			{
				if (m_Dict.ContainsKey(aKey))
					m_Dict[aKey] = aItem;
				else
					m_Dict.Add(aKey, aItem);
			}
			else
				m_Dict.Add(Guid.NewGuid().ToString(), aItem);
		}

		public override JSONNode Remove(string aKey)
		{
			if (!m_Dict.ContainsKey(aKey))
				return null;
			JSONNode tmp = m_Dict[aKey];
			m_Dict.Remove(aKey);
			return tmp;        
		}
		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_Dict.Count)
				return null;
			var item = m_Dict.ElementAt(aIndex);
			m_Dict.Remove(item.Key);
			return item.Value;
		}
		public override JSONNode Remove(JSONNode aNode)
		{
			try
			{
				var item = m_Dict.Where(k => k.Value == aNode).First();
				m_Dict.Remove(item.Key);
				return aNode;
			}
			catch
			{
				return null;
			}
		}

		public override IEnumerable<JSONNode> Childs
		{
			get
			{
				foreach(KeyValuePair<string,JSONNode> N in m_Dict)
					yield return N.Value;
			}
		}

		public IEnumerator GetEnumerator()
		{
			foreach(KeyValuePair<string, JSONNode> N in m_Dict)
				yield return N;
		}
		public override string ToString()
		{
			string result = "{";
			foreach (KeyValuePair<string, JSONNode> N in m_Dict)
			{
				if (result.Length > 2)
					result += ", ";
				result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
			}
			result += "}";
			return result;
		}
		public override string ToUnstyledString()
		{
			string result = "{";
			foreach (KeyValuePair<string, JSONNode> N in m_Dict)
			{
				if (result.Length > 2)
					result += ", ";
				result += "\\\"" + Escape(N.Key) + "\\\":" + N.Value.ToUnstyledString();
			}
			result += "}";
			return result;
		}
		public override string ToString(string aPrefix)
		{
			string result = "{ ";
			foreach (KeyValuePair<string, JSONNode> N in m_Dict)
			{
				if (result.Length > 3)
					result += ", ";
				result += "\n" + aPrefix + "   ";
				result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix+"   ");
			}
			result += "\n" + aPrefix + "}";
			return result;
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONBinaryTag.Class);
			aWriter.Write(m_Dict.Count);
			foreach(string K in m_Dict.Keys)
			{
				aWriter.Write(K);
				m_Dict[K].Serialize(aWriter);
			}
		}
	} // End of JSONClass

	public class JSONData : JSONNode
	{
		private string m_Data;
		public override string Value
		{
			get { return m_Data; }
			set { m_Data = value; }
		}
		public JSONData(string aData)
		{
			m_Data = aData;
		}
		public JSONData(float aData)
		{
			AsFloat = aData;
		}
		public JSONData(double aData)
		{
			AsDouble = aData;
		}
		public JSONData(bool aData)
		{
			AsBool = aData;
		}
		public JSONData(int aData)
		{
			AsInt = aData;
		}
		public JSONData(long aData)
		{
			AsLong = aData;
		}

		public override string ToString()
		{
			return "\"" + Escape(m_Data) + "\"";
		}
		public override string ToUnstyledString()
		{
			return "\\\"" + Escape(m_Data) + "\\\"";
		}
		public override string ToString(string aPrefix)
		{
			return "\"" + Escape(m_Data) + "\"";
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			var tmp = new JSONData("");

			tmp.AsInt = AsInt;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.IntValue);
				aWriter.Write(AsInt);
				return;
			}
			tmp.AsLong = AsLong;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.LongValue);
				aWriter.Write(AsLong);
				return;
			}
			tmp.AsFloat = AsFloat;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.FloatValue);
				aWriter.Write(AsFloat);
				return;
			}
			tmp.AsDouble = AsDouble;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.DoubleValue);
				aWriter.Write(AsDouble);
				return;
			}

			tmp.AsBool = AsBool;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.BoolValue);
				aWriter.Write(AsBool);
				return;
			}
			aWriter.Write((byte)JSONBinaryTag.Value);
			aWriter.Write(m_Data);
		}
	} // End of JSONData

	internal class JSONLazyCreator : JSONNode
	{
		private JSONNode m_Node = null;
		private string m_Key = null;

		public JSONLazyCreator(JSONNode aNode)
		{
			m_Node = aNode;
			m_Key  = null;
		}
		public JSONLazyCreator(JSONNode aNode, string aKey)
		{
			m_Node = aNode;
			m_Key = aKey;
		}

		private void Set(JSONNode aVal)
		{
			if (m_Key == null)
			{
				m_Node.Add(aVal);
			}
			else
			{
				m_Node.Add(m_Key, aVal);
			}
			m_Node = null; // Be GC friendly.
		}

		public override JSONNode this[int aIndex]
		{
			get
			{
				return new JSONLazyCreator(this);
			}
			set
			{
				var tmp = new JSONArray();
				tmp.Add(value);
				Set(tmp);
			}
		}

		public override JSONNode this[string aKey]
		{
			get
			{
				return new JSONLazyCreator(this, aKey);
			}
			set
			{
				var tmp = new JSONClass();
				tmp.Add(aKey, value);
				Set(tmp);
			}
		}
		public override void Add (JSONNode aItem)
		{
			var tmp = new JSONArray();
			tmp.Add(aItem);
			Set(tmp);
		}
		public override void Add (string aKey, JSONNode aItem)
		{
			var tmp = new JSONClass();
			tmp.Add(aKey, aItem);
			Set(tmp);
		}
		public static bool operator ==(JSONLazyCreator a, object b)
		{
			if (b == null)
				return true;
			return System.Object.ReferenceEquals(a,b);
		}

		public static bool operator !=(JSONLazyCreator a, object b)
		{
			return !(a == b);
		}
		public override bool Equals (object obj)
		{
			if (obj == null)
				return true;
			return System.Object.ReferenceEquals(this, obj);
		}
		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return "";
		}
		public override string ToString(string aPrefix)
		{
			return "";
		}

		public override int AsInt
		{
			get
			{
				JSONData tmp = new JSONData(0);
				Set(tmp);
				return 0;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override long AsLong
		{
			get
			{
				JSONData tmp = new JSONData(0);
				Set(tmp);
				return 0;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override float AsFloat
		{
			get
			{
				JSONData tmp = new JSONData(0.0f);
				Set(tmp);
				return 0.0f;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override double AsDouble
		{
			get
			{
				JSONData tmp = new JSONData(0.0);
				Set(tmp);
				return 0.0;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override bool AsBool
		{
			get
			{
				JSONData tmp = new JSONData(false);
				Set(tmp);
				return false;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override JSONArray AsArray
		{
			get
			{
				JSONArray tmp = new JSONArray();
				Set(tmp);
				return tmp;
			}
		}
		public override JSONClass AsObject
		{
			get
			{
				JSONClass tmp = new JSONClass();
				Set(tmp);
				return tmp;
			}
		}
	} // End of JSONLazyCreator

	public static class JSON
	{
		public static JSONNode Parse(string aJSON)
		{
			return JSONNode.Parse(aJSON);
		}
	}
}

#endif