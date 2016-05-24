using UnityEngine;

namespace AI {
	namespace Pilot {
		enum Action {
			Boost = 0,
			Bite,
			Fire
		}
	}

	interface IPilot {
		Vector3 GetImpulse(); // Vector3.zero -> the pilot doesn't want to move
		bool ActionPressed(Pilot.Action _action);
	}
}