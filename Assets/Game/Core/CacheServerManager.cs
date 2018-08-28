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

    private const string OBSOLETE_KEY = "obsolete";

    // String with version the cache is using
    string m_usingVersion;
    // if at some point server says this game needs an update we save the version we were using
    int[] m_obsoleteVersion;


	public void Init(string version)
    {
		SetUsingVersion(version);
		LoadObsoleteVersion();
        ClearOldVersions();
        LoadCountryBlacklisted();
    }

    public void SetUsingVersion( string version)
    {
		m_usingVersion = version;
		string dirFolder = FileUtils.GetDeviceStoragePath ("/cachedVersions/" + m_usingVersion, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
		if ( !Directory.Exists(dirFolder) )
		{
			Directory.CreateDirectory( dirFolder );	
		}
		string cachedIndex = FileUtils.GetDeviceStoragePath ("/cachedVersions.txt", CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
		bool append = true;
		if ( File.Exists( cachedIndex ) )
		{
			string[] lines = File.ReadAllLines( cachedIndex );
			for( int i = 0; i<lines.Length && append; ++i )
			{
				append = lines[i].CompareTo( m_usingVersion ) != 0;
			}
		}

		if ( append )
		{
			using (StreamWriter sw = File.AppendText(cachedIndex)) 
	        {
	            sw.WriteLine( m_usingVersion );
	        }	
	   	}

    }

    private void LoadObsoleteVersion()
    {
		string out_of_date_version = "";
		if ( HasKey(OBSOLETE_KEY, false) )
		{
			out_of_date_version = GetVariable( OBSOLETE_KEY, false );
		}
		m_obsoleteVersion = VersionStrToInts( out_of_date_version );
		if ( m_obsoleteVersion == null )
		{
			m_obsoleteVersion = new int[]{0,0,0};	
		}
    }

	public void RemoveObsoleteVersion()
	{
		DeleteKey( OBSOLETE_KEY, false);
	}

    public void SaveCurrentVersionAsObsolete()
    {
		SetVersionAsObsolete( m_usingVersion );
    }

    public void SetVersionAsObsolete( string v )
    {
		SetVariable(OBSOLETE_KEY, v, false);
		LoadObsoleteVersion();
    }

    private void ClearOldVersions()
    {
		string cachedIndex = FileUtils.GetDeviceStoragePath ("/cachedVersions.txt", CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
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
						string dirFolder = FileUtils.GetDeviceStoragePath ("/cachedVersions/" + line, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
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


    public int[] obsoleteVersion
    {
    	get{return m_obsoleteVersion;}
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

	/// <summary>
	/// Determines if version v1 is older or equal to version v2. Versions are array of numbers being index 0 major to the rest
	/// </summary>
	/// <returns><c>true</c> if version v1 older than v2; otherwise, <c>false</c>.</returns>
	/// <param name="v1">V1.</param>
	/// <param name="v2">V2.</param>
	public static  bool IsVersionOlderOrEqual( int[] v1, int[] v2)
	{
		if ( v1 != null && v2 != null )
    	{
	    	for( int i = 0;i<v1.Length && i<v2.Length; i++ )
	    	{
	    		if ( v1[i] > v2[i] )
	    			return false;
				else if (v1[i] < v2[i])
					return true;
	    	}
			return true;
    	}
    	return false;
	}


	public bool GameNeedsUpdate()
	{
		int[] appVersion = CacheServerManager.VersionStrToInts( m_usingVersion );
		return CacheServerManager.IsVersionOlderOrEqual( appVersion, m_obsoleteVersion );
	}

    #region country_blacklisted
    private const string COUNTRY_BLACKLISTED_KEY = "country_blacklisted";
    private bool m_isCountryBlaclisted = false;

    public void LoadCountryBlacklisted() {
        m_isCountryBlaclisted = false;
     /*   if (HasKey(COUNTRY_BLACKLISTED_KEY)) {
            string str = GetVariable(COUNTRY_BLACKLISTED_KEY);
            m_isCountryBlaclisted = bool.Parse(str);
        }*/
    }

    public void SetCountryBlacklisted(bool _value) {
        m_isCountryBlaclisted = _value;
    //   SetVariable(COUNTRY_BLACKLISTED_KEY, _value.ToString());
    }

    public bool IsCountryBlacklisted() {
        return m_isCountryBlaclisted;
    }
    #endregion


    #region saving_and_loading

    string GetFilePath( string key, bool versioned )
	{
		string path = "";
		if ( versioned )
		{
			path = FileUtils.GetDeviceStoragePath ("/cachedVersions/" + m_usingVersion + "/" + key, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
		}
		else
		{
			path = FileUtils.GetDeviceStoragePath ( key, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
		}
		return path;
	}

	public void SetVariable( string key, string value, bool versioned = true)
	{
		File.WriteAllText( GetFilePath( key, versioned ) , value);	
	}

	public string GetVariable( string key, bool versioned = true )
	{
		string ret = File.ReadAllText( GetFilePath( key, versioned )  );
		return ret;
	}

	public bool HasKey( string key, bool versioned = true )
	{
		return File.Exists( GetFilePath( key, versioned )  );
	}

	public void DeleteKey( string key, bool versioned = true )
	{
		string path = GetFilePath( key, versioned );
		if ( File.Exists( path ) )
			File.Delete( path );
	}

	#endregion

}
