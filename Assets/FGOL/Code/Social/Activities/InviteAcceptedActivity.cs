using System;
using System.Collections.Generic;
using UnityEngine;

public class InviteAcceptedActivity : SocialActivity
{
    // [DGR] No support added yet
    //private string m_messageID = null;

    public InviteAcceptedActivity(Dictionary<string, object> message) 
        : base(null)
    {
        message["relevance"] = 9;
        Init(message);

        //[DGR] No support added yet
        /*
        m_messageID = message["messageID"] as string;

        //TODO to be removed if below code is uncommented        
        //GameUtil.Noop(m_messageID);
        */

        m_interactive = true;
    }

    public override string GetActivityType()
    {
        return "InviteAccepted";
    }

    public override string GetActivityDescription()
    {
        return "Your friend started playing HSX! Collect your Reward Now!";
    }

    public override void OnClick(Action<bool, bool> onClickFinished)
    {
        /*SocialManager.Instance.ClaimInviteAcceptedReward(m_messageID, delegate(bool success)
        {
            onClickFinished(!success, success);
        });*/
    }
}