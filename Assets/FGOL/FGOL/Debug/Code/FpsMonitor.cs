using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class FpsMonitor
{
	const int SampleAmount = 30;

	readonly List<float> m_samples = new List<float>(10000);

	public float current { get; private set; }

	public FpsMonitor()
	{
		current = -1;
	}

	public void Update()
	{
		m_samples.Add(Time.deltaTime);

		if (m_samples.Count < SampleAmount)
			return;

		var total = 0f;

		for (var i = m_samples.Count - SampleAmount; i < m_samples.Count; i++)
			total += m_samples[i];

		current = 1f / (total / (float)SampleAmount);
	}

	public void GetAverageAndDeviation(out float average, out float averageDeviation)
	{
		var a = average = m_samples.Average();
		averageDeviation = m_samples.Select(f => Math.Abs(f - a)).Average();
	}
}