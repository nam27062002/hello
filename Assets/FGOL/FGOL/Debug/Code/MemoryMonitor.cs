using System;
using UnityEngine;

abstract class MemoryMonitor
{
	protected const float BytesPerMegabyte = 1024 * 1024;
	public bool enableAccurateMemorycheck=true;

	public float rssMegabytes { get; private set; }
	public float peakRssMegabytes { get; private set; }
	public float heapMegabytes { get; private set; }
	public float peakHeapMegabytes { get; private set; }

	public const float WAIT_DURATION = 1.0f;
	private float dt = 0;

	public void Update()
	{
		dt += Time.unscaledDeltaTime;

		if (enableAccurateMemorycheck) 
		{
			if (dt >= WAIT_DURATION) {
				dt = 0;
				rssMegabytes = GetRssMegaByte ();
			}

			if (rssMegabytes < 0)
				throw new Exception ("Failed to determine memory usage");
			if (rssMegabytes > peakRssMegabytes)
				peakRssMegabytes = rssMegabytes;
		}
		heapMegabytes = GC.GetTotalMemory(false) / BytesPerMegabyte;
		if (heapMegabytes > peakHeapMegabytes)
			peakHeapMegabytes = heapMegabytes;
	}

	protected abstract float GetRssMegaByte();
}