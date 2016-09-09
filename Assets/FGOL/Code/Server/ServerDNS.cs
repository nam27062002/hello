using System;
using System.Collections.Generic;

public static class ServerDNS
{
    public static Dictionary<string, string> geoDnsRecords = new Dictionary<string, string>
    {
        { "development", "http://auth-dev-geo.fgol.mobi:3000" },
        { "preproduction", "https://auth-preprod-geo.fgol.mobi" },
        { "production", "https://hsx-auth-geo.fgol.mobi" }
    };

    public static Dictionary<string, Dictionary<string, Dictionary<GeoLocation.Location, string>>> dnsRecords = new Dictionary<string, Dictionary<string, Dictionary<GeoLocation.Location, string>>>()
    {
        {
            "HSX", new Dictionary<string, Dictionary<GeoLocation.Location, string>>()
            {
                {
                    "development", new Dictionary<GeoLocation.Location, string>
                    {
                        { GeoLocation.Location.Default, "https://hsx-dev.fgol.mobi" },
                        { GeoLocation.Location.China,   "http://hsx-dev-china.fgol.mobi:3000" }
                    }
                },
                {
                    "preproduction", new Dictionary<GeoLocation.Location, string>
                    {
                        { GeoLocation.Location.Default, "https://hsx-preprod.fgol.mobi" },
                        { GeoLocation.Location.China,   "https://hsx-preprod-china.fgol.mobi" },
                    }
                },
                {
                    "production", new Dictionary<GeoLocation.Location, string>
                    {
                        { GeoLocation.Location.Default, "https://hsx.fgol.mobi" },
                        { GeoLocation.Location.China,   "https://hsx-china.fgol.mobi" }
                    }
                }
            }
        },
        {
            "Auth", new Dictionary<string, Dictionary<GeoLocation.Location, string>>()
            {
                {
                    "development", new Dictionary<GeoLocation.Location, string>
                    {
                        { GeoLocation.Location.Default, "https://auth-dev.fgol.mobi" },
                        { GeoLocation.Location.China,   "http://auth-dev-china.fgol.mobi:3000" }
                    }
                },
                {
                    "preproduction", new Dictionary<GeoLocation.Location, string>
                    {
                        { GeoLocation.Location.Default, "https://auth-preprod.fgol.mobi" },
                        { GeoLocation.Location.China,   "https://auth-preprod-china.fgol.mobi" },
                    }
                },
                {
                    "production", new Dictionary<GeoLocation.Location, string>
                    {
                        { GeoLocation.Location.Default, "https://hsx-auth.fgol.mobi" },
                        { GeoLocation.Location.China,   "https://hsx-auth-china.fgol.mobi" }
                    }
                }
            }
        }
    };
}
