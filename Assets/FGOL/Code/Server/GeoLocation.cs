using System;
using System.Collections.Generic;
using UnityEngine;

public static class GeoLocation
{
    public enum Location
    {
        Default,
        China
    }

    private const string geoServer = "Geo";
    private const string geoPref = "ServerGeoLocation";

    private static Location s_location = Location.Default;

    public static Location location
    {
        get { return s_location; }
    }

    public static void DetermineGeoLocation(Action<Location> onComplete)
    {
        Action<Location> onGotLocation = delegate(Location location){
            Debug.Log("GeoLocation (DetermineGeoLocation) :: Location - " + location);

            UpdateLocation(location);
            onComplete(location);
        };

        Location? storedLocation = GetStoredLocation();

        if (!storedLocation.HasValue)
        {
            GetServerLocation(delegate(Location? serverLocation){
                if (!serverLocation.HasValue)
                {
                    onGotLocation(GetLocationByLanguage());
                }
                else
                {
                    onGotLocation(serverLocation.Value);
                }
            });
        }
        else
        {
            onGotLocation(storedLocation.Value);
        }
    }

    public static void UpdateLocation(Location location)
    {
        s_location = location;

        PlayerPrefs.SetString(geoPref, location.ToString());
        PlayerPrefs.Save();
    }

    private static Location? GetStoredLocation()
    {
        Location? location = null;

        string loc = PlayerPrefs.GetString(geoPref, null);

        try
        {
            location = (Location)Enum.Parse(typeof(Location), loc);
        }
        catch (Exception) { }

        return location;
    }

    private static void GetServerLocation(Action<Location?> onComplete)
    {
        // [DGR] SERVER: Not supported yet
        if (onComplete != null)
        {
            onComplete(Location.Default);
        }
        /*    
        string configEnvironment = Config.Instance["server.environment"] as string;
        string environment = configEnvironment != null ? configEnvironment : Globals.GetEnvironment().ToString();

        string configGeoUrl = Config.Instance["server.geourl"] as string;
        string geoUrl = configGeoUrl != null ? configGeoUrl : ServerDNS.geoDnsRecords[environment];

        Command.BaseUrls[geoServer] = geoUrl;

        Command geoPingCommand = new Command("GeoPing", "/Ping", geoServer, Command.Type.Normal, Request.Method.GET, 5, 5);

        geoPingCommand.Run(new Dictionary<string, string>(), Commander.GlobalHeaders, delegate (Error error, Dictionary<string, object> result) {
            string region = null;

            if (error == null)
            {
                if (result != null)
                {
                    if (result.ContainsKey("Pong") && result.ContainsKey("Region"))
                    {
                        region = result["Region"] as string;
                    }
                    else
                    {
                        error = new InvalidServerResponseError("Response not as expected");
                    }
                }
                else
                {
                    error = new InvalidServerResponseError("Response not as expected");
                }
            }

            if (error != null)
            {
                Debug.LogWarning("GeoLocation (DetermineGeoLocation) :: Error determining server location - " + error);
            }

            Location? loc = null;

            if (!string.IsNullOrEmpty(region))
            {
                switch (region)
                {
                    case "china":
                        loc = Location.China;
                        break;
                    default:
                        loc = Location.Default;
                        break;
                }
            }

            onComplete(loc);
        }, "*.fgol.mobi");
        */
    }

    private static Location GetLocationByLanguage()
    {
        Location loc = Location.Default;

        switch(Application.systemLanguage)
        {
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                loc = Location.China;
                break;
            default:
                break;
        }

        return loc;
    }
}
