#if !PRODUCTION

//[DGR] No support added yet
//using FGOL.Plugins.Native;
using System.Runtime.InteropServices;

class IosMemoryMonitor : MemoryMonitor
{
	protected override float GetRssMegaByte()
	{
        //[DGR] No support added yet
        //return NativeBinding.Instance.GetMemoryUsage() / BytesPerMegabyte;
        return 0;
    }
}

#endif