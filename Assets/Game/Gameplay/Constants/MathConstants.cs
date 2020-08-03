using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Why we do this? because always we call Unity's Vector3.zero it calls a function and generates call stack memory etc etc

namespace GameConstants
{
	public class Vector3
	{
		public static readonly UnityEngine.Vector3 zero 	= UnityEngine.Vector3.zero;
		public static readonly UnityEngine.Vector3 one 		= UnityEngine.Vector3.one;
		public static readonly UnityEngine.Vector3 right 	= UnityEngine.Vector3.right;
		public static readonly UnityEngine.Vector3 left 	= UnityEngine.Vector3.left;
		public static readonly UnityEngine.Vector3 up 		= UnityEngine.Vector3.up;
		public static readonly UnityEngine.Vector3 down 	= UnityEngine.Vector3.down;
		public static readonly UnityEngine.Vector3 forward 	= UnityEngine.Vector3.forward;
		public static readonly UnityEngine.Vector3 back 	= UnityEngine.Vector3.back;
		public static readonly UnityEngine.Vector3 center	= new UnityEngine.Vector3(0.5f, 0.5f, 0.5f);
	}

	public class Vector2
	{
		public static readonly UnityEngine.Vector2 zero 	= UnityEngine.Vector2.zero;
		public static readonly UnityEngine.Vector2 one 		= UnityEngine.Vector2.one;
		public static readonly UnityEngine.Vector2 right 	= UnityEngine.Vector2.right;
		public static readonly UnityEngine.Vector2 left 	= UnityEngine.Vector2.left;
		public static readonly UnityEngine.Vector2 up 		= UnityEngine.Vector2.up;
		public static readonly UnityEngine.Vector2 down 	= UnityEngine.Vector2.down;
		public static readonly UnityEngine.Vector2 center	= new UnityEngine.Vector2(0.5f, 0.5f);
	}

	public class Quaternion
	{
		public static readonly UnityEngine.Quaternion identity = UnityEngine.Quaternion.identity;	
	}

}
