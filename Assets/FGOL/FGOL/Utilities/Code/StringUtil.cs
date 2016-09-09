//--------------------------------------------------------------------------------
// StringUtil.cs
//--------------------------------------------------------------------------------
// String helpers.  TODO: add enough things to justify this having its own file.
//--------------------------------------------------------------------------------
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

public static class StringUtil
{
    public static CultureInfo currentCulture = null;

	// int to hex string - a simple extension method of int, rather than typing the gibberish string.Format stuff.
	public static string ToHexString(this int n)
	{
		return string.Format("{0:X}", n);
	}
	
	// extension method of string to remove "(Clone)" from the end if it is present, otherwise returns an unchanged string.
	public static string RemoveCloneSuffix(this string s)
	{
		if(s.EndsWith("(Clone)"))
			return s.Substring(0, s.Length-7);
		return s;
	}

	public static float ParsePriceStringToFloat(string formattedPrice)
	{
		try
		{
			formattedPrice = Regex.Match(formattedPrice, "([0-9]+[.,\u066B]{0,1}[0-9]*)").Value;
			formattedPrice = formattedPrice.Replace(",", ".");
			formattedPrice = formattedPrice.Replace("\u066B", ".");
			return float.Parse(formattedPrice);
		} 
		catch( Exception )
		{
			Debug.LogError("Unable to parse price: " + formattedPrice);
			return 0f;
		}
	}

	public static float ExtractPriceFromString(string dirtyPrice)
	{
		try
		{
			int priceStartIndex = 0;
			int priceEndIndex = 0;

			// Take only the digits and seperators from price string
			for(int i = 0; i < dirtyPrice.Length; i++)
			{
				// Check if within digit ASCII range 
				if(dirtyPrice[i] - 48 >= 0 && dirtyPrice[i] - 48 <= 9)
                {
					priceStartIndex = i;
					break;
				}
			}

			for(int i = dirtyPrice.Length - 1; i >= 0; i--)
			{
				// Check if within digit ASCII range 
				if(dirtyPrice[i] - 48 >= 0 && dirtyPrice[i] - 48 <= 9)
				{
					priceEndIndex = i;
					break;
				}
			}

			string validStringPrice = dirtyPrice.Substring(priceStartIndex, (priceEndIndex + 1) - priceStartIndex);

			//	We first try current culture parse, if that fails we try invariant one
			//	if that fails we try swaping manually decimal and thousands symbol
			//	Could be overkill, but since hard to test on different stores this way we support multiple cultures
			float parsed = 0;
			bool success = float.TryParse(validStringPrice, NumberStyles.Currency, CultureInfo.CurrentCulture, out parsed);
			if (!success)
			{
				success = float.TryParse(validStringPrice, NumberStyles.Currency, CultureInfo.InvariantCulture, out parsed);
				if (!success)
				{
					//	Manually swap decimal and thousands
					CultureInfo info = CultureInfo.InvariantCulture.Clone() as CultureInfo;
					string dec = info.NumberFormat.NumberDecimalSeparator;
					string tho = info.NumberFormat.NumberGroupSeparator;

					info.NumberFormat.NumberDecimalSeparator = tho;
					info.NumberFormat.NumberGroupSeparator = dec;

					success = float.TryParse(validStringPrice, NumberStyles.Currency, info, out parsed);
				}
			}

			return parsed;
		}
		catch(Exception e)
		{
			string err = e.Message;
			Debug.LogError("Unable to parse price: " + dirtyPrice + " " + err);
			return 0f;
		}
	}

	// For overriding stats, need to be able to parse a string to the appropriate type and return as a System.Object.
	// May need to add more types to this.
	// Returns null if parsing failed.
	public static System.Object ToObjectOfType(this string s, System.Type t)
	{
		//Debug.Log("Parsing type "+t.ToString()+", type code "+System.Type.GetTypeCode(t).ToString());
	
		// handle enums separately
		if(t.IsEnum)
		{
			try
			{
				System.Object o = System.Enum.Parse(t, s);
				return o;
			}
			catch(System.Exception)
			{
				return null;
			}
		}
	
		switch(System.Type.GetTypeCode(t))
		{
			case System.TypeCode.Boolean:
			{
				bool n;
				if(bool.TryParse(s, out n))
					return n;
				return null;
			}
			
			case System.TypeCode.Int32:
			{
				int n;
				if(int.TryParse(s, out n))
					return n;
				return null;
			}
			
			case System.TypeCode.Single:
			{
				float n;
				if(float.TryParse(s, out n))
					return n;
				return null;
			}
			
			// obviously nothing to do for string
			case System.TypeCode.String:
			{
				return s;
			}
		}
		
		// If it's a type that we didn't already handle, check for some specific ones like Vector3
		if(t==typeof(Vector2))
		{
			string[] values = s.Split(',');
			if(values.Length != 2)
				return null;
			float x = float.Parse(values[0]);
			float y = float.Parse(values[1]);
			return new Vector2(x, y);
		}
		
		if(t==typeof(Vector3))
		{
			string[] values = s.Split(',');
			if(values.Length != 3)
				return null;
			float x = float.Parse(values[0]);
			float y = float.Parse(values[1]);
			float z = float.Parse(values[2]);
			return new Vector3(x, y, z);
		}
		
		if(t==typeof(Color))
		{
			string[] values = s.Split(',');
			float len = values.Length;
			if((len < 3) || (len > 4))
				return null;
			float range = 255.0f;
			float scale = 1.0f/range;
			float r = float.Parse(values[0])*scale;
			float g = float.Parse(values[1])*scale;
			float b = float.Parse(values[2])*scale;
			float a = (len==3) ? range : (float.Parse(values[3])*scale);
			return new Color(r, g, b, a);
		}
		
		return null;
	}
	
	// Try to convert labels from how they appear in the inspector (separate words, m_ prefix stripped out, etc) back to the real
	// class or field names.
	public static string ToMixedCaseNoSpaces(this string s, bool camelCase, bool memberPrefix)
	{
		// split the string into words, with all whitespace stripped out
		string[] words = s.Split(null as char[], System.StringSplitOptions.RemoveEmptyEntries);
		string str = "";
		// add m_ prefix if necessary (only if we want it, and if it isn't already there)
		if(memberPrefix && !s.StartsWith("m_"))
			str = "m_";
			
		bool firstWord = true;
		foreach(string word in words)
		{
			bool wantUpper = !(camelCase && firstWord);		// we use uppercase first letter on all words except on the first word in camel case
		
			char firstLetter = word[0];
			if(System.Char.IsUpper(firstLetter) == wantUpper)	// if the word is already in the desired format, take it as-is
				str += word;
			else
			{
				// otherwise flip case on the first letter
				firstLetter = wantUpper ? System.Char.ToUpper(firstLetter) : System.Char.ToLower(firstLetter);	
				str += firstLetter;
				if(word.Length > 1)
					str += word.Substring(1);
			}
			firstWord = false;
		}
		
		return str;
	}

    public static string GetCultureNameByLanguage( string language )
    {
        string cultureName = "";
        switch( language )
        {
            case "English":
                cultureName = "en-US";
            break;

            case "Spanish":
                cultureName = "es-ES";
            break;

            case "French":
                cultureName = "fr-FR";
            break;

            case "German":
                cultureName = "de-DE";
            break;

            case "Italian":
                cultureName = "it-IT";
            break;

            case "Japanese":
                cultureName = "ja-JP";
            break;

            case "Korean":
                cultureName = "ko-KR";
            break;

            case "Turkish":
                cultureName = "tr-TR";
            break;

            case "Russian":
                cultureName = "ru-RU";
            break;

            case "Chinese":
                cultureName = "zh-CN";
            break;

            case "Taiwanese":
                cultureName = "zh-TW";
            break;

            case "BrazilianPortuguese":
                cultureName = "pt-BR";
            break;

            default:
                cultureName = "en-US";
            break;

        }

        return cultureName;
    }
    public static string FormatNumberToThousands( int number, string toStringFormat = "#,#" )
    {
        //[DGR] Not support yet
        /*string cultureName = GetCultureNameByLanguage(Localization.language);
        if (currentCulture == null || currentCulture.DisplayName != cultureName )
        {
            //currentCulture = new CultureInfo("",true);   //If we want to use the culture from the system, you do it like this.
            currentCulture = new CultureInfo(cultureName);
        }*/
        
        string result = number.ToString(toStringFormat, currentCulture);
        

        return result;
    }
	
    
    // todo: add helper stuff for JSON parsing?
}