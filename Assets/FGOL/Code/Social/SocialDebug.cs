using System;
using System.Collections.Generic;

#if !PRODUCTION

public class SocialDebug
{
    private static List<string> s_debugFriendsList = new List<string>
    {
        "AAAAAAAA",
        "BAAAAAAA",
        "DAAAAAAA",
        "CAAAAAAA",
        "EAAAAAAA",
        "FAAAAAAA",
        "HAAAAAAA",
        "IAAAAAAA",
        "JAAAAAAA",
        "GAAAAAAA",
        "KAAAAAAA",
        "LAAAAAAA",
        "OAAAAAAA",
        "MAAAAAAA",
        "PAAAAAAA",
        "QAAAAAAA",
        "RAAAAAAA",
        "SAAAAAAA",
        "NAAAAAAA",
        "TAAAAAAA",
        "UAAAAAAA",
        "VAAAAAAA",
        "WAAAAAAA",
        "XAAAAAAA",
        "YAAAAAAA",
        "ZAAAAAAA",
        "aAAAAAAA",
        "bAAAAAAA",
        "cAAAAAAA",
        "dAAAAAAA",
        "eAAAAAAA",
        "fAAAAAAA",
        "gAAAAAAA",
        "hAAAAAAA",
        "iAAAAAAA",
        "jAAAAAAA",
        "lAAAAAAA",
        "kAAAAAAA",
        "mAAAAAAA",
        "nAAAAAAA",
        "pAAAAAAA",
        "qAAAAAAA",
        "oAAAAAAA",
        "sAAAAAAA",
        "tAAAAAAA",
        "vAAAAAAA",
        "uAAAAAAA",
        "rAAAAAAA",
        "xAAAAAAA",
        "wAAAAAAA",
        "ABAAAAAA",
        "zAAAAAAA",
        "yAAAAAAA",
        "CBAAAAAA",
        "BBAAAAAA",
        "FBAAAAAA",
        "DBAAAAAA",
        "EBAAAAAA",
        "GBAAAAAA",
        "HBAAAAAA",
        "JBAAAAAA",
        "IBAAAAAA",
        "KBAAAAAA",
        "LBAAAAAA",
        "OBAAAAAA",
        "NBAAAAAA",
        "MBAAAAAA",
        "PBAAAAAA",
        "QBAAAAAA",
        "SBAAAAAA",
        "RBAAAAAA",
        "TBAAAAAA",
        "UBAAAAAA",
        "VBAAAAAA",
        "WBAAAAAA",
        "XBAAAAAA",
        "YBAAAAAA",
        "ZBAAAAAA",
        "aBAAAAAA",
        "cBAAAAAA",
        "bBAAAAAA",
        "eBAAAAAA",
        "dBAAAAAA",
        "fBAAAAAA",
        "hBAAAAAA",
        "gBAAAAAA",
        "iBAAAAAA",
        "jBAAAAAA",
        "kBAAAAAA",
        "mBAAAAAA",
        "lBAAAAAA",
        "nBAAAAAA",
        "oBAAAAAA",
        "pBAAAAAA",
        "rBAAAAAA",
        "qBAAAAAA",
        "sBAAAAAA",
        "tBAAAAAA",
        "uBAAAAAA",
        "vBAAAAAA"
    };

    public static Dictionary<string, string> AugmentFriends(Dictionary<string, string> friends)
    {
//        if (SocialFacade.Instance.GetSocialID(SocialFacade.Network.Facebook) == "112060569151757")
        {
            foreach (string socialID in s_debugFriendsList)
            {
                friends.Add(socialID, "Debug User");
            }
        }

        return friends;
    }

    public static string GetRealSocialID(string socialID)
    {
        return Char.IsLetter(socialID[0]) ? "744788272317658" : socialID;
    }
}

#endif
