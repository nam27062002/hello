using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Why we do this? because always we call Unity's Vector3.zero it calls a function and generates call stack memory etc etc

namespace GameConstants
{
	public static class Layers
	{
        public static readonly int GROUNDS = LayerMask.GetMask( "Ground", "GroundVisible" );
	}
}
