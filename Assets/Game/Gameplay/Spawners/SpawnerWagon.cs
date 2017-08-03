using UnityEngine;
using System.Collections;
using AI;

public class SpawnerWagon : Spawner {

	[SerializeField] private BSpline.BezierSpline m_railSpline;

	protected override void OnMachineSpawned(IMachine machine) {
		MachineWagon machineWagon = machine as MachineWagon;
		machineWagon.SetRails(m_railSpline);
	}
}
