using UnityEngine;
using System.Collections;

public class SpawnerConditions : MonoBehaviour {
	[System.Serializable]
	public class SpawnCondition {
		public enum Type {
			XP,
			TIME
		}

		public Type type = Type.XP;

		[NumericRange(0f)]	// Force positive value
		public float value = 0f;
	}

	//--------------------------------------------------------------------------

	[Separator("Activation")]
	[SerializeField] public DragonTier m_minTier = DragonTier.TIER_0;

	[Tooltip("Spawners may not be present on every run (percentage).")]
	[SerializeField][Range(0f, 100f)] public float m_activationChance = 100f;

	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] public SpawnCondition[] m_activationTriggers;
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers;
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}


	//--------------------------------------------------------------------------

	public bool IsAvailable() {
		if (InstanceManager.player.data.tier >= m_minTier) {
			return Random.Range(0f, 100f) <= m_activationChance;
		}
		return false;
	}

	public bool IsReadyToSpawn(float _time, float _xp) {
		bool startConditionsOk = (m_activationTriggers.Length == 0);	// If there are no activation triggers defined, the spawner will be considered ready
		for(int i = 0; i < m_activationTriggers.Length; i++) {
			// Is this condition satisfied?
			switch(m_activationTriggers[i].type) {
				case SpawnCondition.Type.XP: {
						startConditionsOk |= (_xp >= m_activationTriggers[i].value);	// We've earned enough xp
					} break;

				case SpawnCondition.Type.TIME: {
						startConditionsOk |= (_time >= m_activationTriggers[i].value);	// We've reached the activation time
					} break;
			}

			// If one of the conditions has already triggered, no need to keep checking
			// [AOC] This would be useful if we had a lot of conditions to check, but it will usually be just one and we would be adding an extra instruction for nothing, so let's keep it commented for now
			// if(startConditionsOk) break;
		}

		return startConditionsOk;
	}

	public bool IsReadyToBeDisabled(float _time, float _xp) {
		// Check end conditions
		bool endConditionsOk = true;
		for(int i = 0; i < m_deactivationTriggers.Length; i++) {
			// Is this condition satisfied?
			switch(m_deactivationTriggers[i].type) {
				case SpawnCondition.Type.XP: {
						endConditionsOk &= (_xp < m_deactivationTriggers[i].value);		// We haven't yet reached the xp limit
					} break;

				case SpawnCondition.Type.TIME: {
						endConditionsOk &= (_time < m_deactivationTriggers[i].value);	// We haven't yet reached the time limit
					} break;
			}

			// If one of the conditions has already triggered, no need to keep checking
			// [AOC] This would be useful if we had a lot of conditions to check, but it will usually be just one and we would be adding an extra instruction for nothing, so let's keep it commented for now
			// if(!endConditionsOk) break;
		}

		return !endConditionsOk;
	}
}
