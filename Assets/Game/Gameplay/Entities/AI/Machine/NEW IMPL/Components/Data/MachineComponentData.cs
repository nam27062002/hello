using UnityEngine;

namespace AI {
	public abstract class MachineComponentData : MonoBehaviour {

		public abstract MachineComponent.Type type { get; }

	}
}
