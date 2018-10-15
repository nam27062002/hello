using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Math test. This class will contain math test of the kind AvsB to know if they touch
/// </summary>
public class MathTest 
{
    
    /// <summary>
    /// Tests the circle vs circle.
    /// </summary>
    /// <returns><c>true</c>, if the circles touch, <c>false</c> otherwise.</returns>
    /// <param name="center1">Center1.</param>
    /// <param name="radius1">Radius1.</param>
    /// <param name="center2">Center2.</param>
    /// <param name="radius2">Radius2.</param>
    public static bool TestCircleVsCircle( Vector2 center1, float radius1, Vector2 center2, float radius2 )
    {
        bool ret = false;
        float sqrMagnitude = (center1 - center2).sqrMagnitude;
        float r = radius1 + radius2;
        if ( sqrMagnitude <= r * r  )
        {
            ret = true;
        }
        return ret;
    }
    
    public static bool TestCircleVsRect( Vector2 center, float radius, Rect r )
    {
        bool ret = false;
        Vector2 distance = GameConstants.Vector2.zero;
        distance.x = Mathf.Abs(center.x - r.center.x);
        distance.y = Mathf.Abs(center.y - r.center.y);

        // too far away
        if (distance.x > (r.width/2.0f + radius)) { return false; }
        if (distance.y > (r.height/2.0f + radius)) { return false; }

        // center inside rectangle
        if (distance.x <= (r.width/2.0f)) { return true; } 
        if (distance.y <= (r.height/2.0f)) { return true; }

        // Other cases
        // sqr magnitude
        float cornerDistance_sq = Mathf.Pow( (distance.x - r.width/2), 2) + Mathf.Pow((distance.y - r.height/2), 2);

        ret = (cornerDistance_sq <= (radius*radius));
        return ret;
    }
    

}
