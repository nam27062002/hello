using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Why we do this? because always we call Unity's Vector3.zero it calls a function and generates call stack memory etc etc

namespace GameConstants
{
	public class Vector3
	{
		public static readonly UnityEngine.Vector3 zero = UnityEngine.Vector3.zero;
		public static readonly UnityEngine.Vector3 one = UnityEngine.Vector3.one;
	}

	public class Quaternion
	{
		public static readonly UnityEngine.Quaternion identity = UnityEngine.Quaternion.identity;	
	}

}
