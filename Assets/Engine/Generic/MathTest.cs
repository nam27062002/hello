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
    

}
