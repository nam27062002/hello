using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;

public class CacheServerManager {

	#region singleton
	//------------------------------------------------------------------------//
	// SINGLETON IMPLEMENTATION												  //
	//------------------------------------------------------------------------//
	private static CacheServerManager s_pInstance = null;
	public static CacheServerManager SharedInstance
    {
        get
        {
            if (s_pInstance == null)
            {
				s_pInstance = new CacheServerManager();
            }
            return s_pInstance;
        }
    }
    #endregion

    string m_usingVersion;
    int[] m_minValidVersion;
	public void Init(string version)
    {
		m_usingVersion = version;

		// Get Saved Min Version
		string minversion = PlayerPrefs.GetString("MIN_VERSION", "");
		m_minValidVersion = VersionStrToInts( minversion );
		if ( m_minValidVersion == null )
		{
			m_minValidVersion = new int[]{0,0,0};	
		}
    }

    public void SetMinValidVersion( string v )
    {
		PlayerPrefs.SetString("MIN_VERSION",v);
    }

    private void ClearOldVersions()
    {
		string cachedIndex = Application.persistentDataPath + "/cachedVersions.txt";
		if ( File.Exists( cachedIndex ) )
		{
			int[] usingVersion = VersionStrToInts(m_usingVersion);
			using (StreamReader sr = new StreamReader(cachedIndex)) 
            {
                while (sr.Peek() >= 0) 
                {
                    string line = sr.ReadLine();
                    if ( IsVersionOlder( VersionStrToInts( line ) , usingVersion) )
                    {
                    	// Clean files
                    	string dirFolder = Application.persistentDataPath + "/cachedVersions/" + line;
                    	if ( Directory.Exists( dirFolder ) )
                    	{
                    		Directory.Delete( dirFolder, true);
                    	}
                    }
                }
            }
            File.WriteAllLines( cachedIndex, new string[]{m_usingVersion} );
		}
    }


    public int[] minValidVersion
    {
    	get{return m_minValidVersion;}
    }

	/// <summary>
	/// Determines if version v1 is older than version v2. Versions are string of type X.Y.Z
	/// </summary>
	/// <returns><c>true</c> if version v1 older than v2; otherwise, <c>false</c>.</returns>
	/// <param name="v1">V1.</param>
	/// <param name="v2">V2.</param>
	public static  bool IsVersionOlder( string v1, string v2)
	{
		int[] version_1 = VersionStrToInts(v1);
		int[] version_2 = VersionStrToInts(v2);
		bool ret = false;
		if ( version_1 != null && version_2 != null )
			ret = IsVersionOlder( version_1, version_2);
		return ret;
	}

	/// <summary>
	/// Versions the string to ints. 
	/// </summary>
	/// <returns>Array of ints containing the numbers or null if it can't parse the string</returns>
	/// <param name="v">V.</param>
	public static int[] VersionStrToInts( string v )
	{
		int[] ret = null;
		if ( !string.IsNullOrEmpty(v) )
		{
			Match match = Regex.Match(v, @"([0-9]+)\.([0-9]+)\.?([0-9]+)?");
			if(match.Success) {
				ret = new int[ match.Groups.Count - 1 ];
				for( int i = 1; i<match.Groups.Count; i++ )
				{
					int result;
					if (int.TryParse(match.Groups[i].Value, out result))
					{
						ret[i-1] = result;
					}
					else
					{	
						ret[i-1] = 0;
					}
				}
			}
		}
		return ret;
	}

	/// <summary>
	/// Determines if version v1 is older than version v2. Versions are array of numbers being index 0 major to the rest
	/// </summary>
	/// <returns><c>true</c> if version v1 older than v2; otherwise, <c>false</c>.</returns>
	/// <param name="v1">V1.</param>
	/// <param name="v2">V2.</param>
    public static  bool IsVersionOlder( int[] v1, int[] v2)
    {
    	if ( v1 != null && v2 != null )
    	{
	    	for( int i = 0;i<v1.Length && i<v2.Length; i++ )
	    	{
	    		if ( v1[i] < v2[i] )
	    			return true;
	    	}
    	}
    	return false;
    }
}
