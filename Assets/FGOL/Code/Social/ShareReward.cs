using System;
using System.Collections.Generic;

public class ShareReward
{
    private string m_messageID = null;
    private int m_friends = 0;

    public int Friends
    {
        get { return m_friends; }
    }

    public string MessageID
    {
        get { return m_messageID; }
    }

    public ShareReward(Dictionary<string, object> message)
    {
        m_messageID = message["messageID"] as string;
        m_friends = Convert.ToInt32(message["friends"]);
    }
}
