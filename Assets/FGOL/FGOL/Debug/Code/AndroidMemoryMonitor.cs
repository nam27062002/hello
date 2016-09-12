#if UNITY_ANDROID


//[DGR] No support added yet
//using FGOL.Plugins.Native;
using System;
using UnityEngine;

class AndroidMemoryMonitor : MemoryMonitor
{
	protected override float GetRssMegaByte()
	{
        //[DGR] No support added yet
        //return NativeBinding.Instance.GetMemoryUsage() / 256f;
        return 0;
	}
}

#endif