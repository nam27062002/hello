using System.Collections.Generic;

public class JoinedActivity : SocialActivity
{
    public JoinedActivity(Dictionary<string, object> json = null)
        : base(json)
    {
    }

    public override string GetActivityType()
    {
        return "Joined";
    }

    public override string GetActivityDescription()
    {
        return "Your friend started playing HSX!";
    }
}
