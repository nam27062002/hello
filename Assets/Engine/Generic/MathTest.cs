using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathTest 
{

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
