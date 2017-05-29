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

	[System.Serializable]
	public class SpawnKillCondition {
		[EntityCategoryListAttribute]
		public string category;

		[NumericRange(0f)]	// Force positive value
		public float value = 0f;
	}

	//--------------------------------------------------------------------------

	[Separator("Activation")]
	[SerializeField] public DragonTier m_minTier = DragonTier.TIER_0;
	[SerializeField] private DragonTier m_maxTier = DragonTier.TIER_4;
	[SerializeField] private bool	    m_checkMaxTier = false;

	[Tooltip("Spawners may not be present on every run (percentage).")]
	[SerializeField][Range(0f, 100f)] public float m_activationChance = 100f;

	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] public SpawnCondition[] m_activationTriggers = new SpawnCondition[0];
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[SerializeField] public SpawnKillCondition[] m_activationKillTriggers = new SpawnKillCondition[0];
	public SpawnKillCondition[] activationKillTriggers { get { return m_activationKillTriggers; } }

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers = new SpawnCondition[0];
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}


	//--------------------------------------------------------------------------

	public bool IsAvailable() {
		float rnd = Random.Range(0f, 100f);
		DragonTier playerTier = InstanceManager.player.data.tier;

		bool enabledByTier = false;
		if (m_checkMaxTier) {
			enabledByTier = (playerTier >= m_minTier) && (playerTier <= m_maxTier);
		} else {
			enabledByTier = (playerTier >= m_minTier);
		}

		if (m_activationChance < 100f) {
			// check debug 
			if (DebugSettings.spawnChance0) {
				rnd = 100f;
			} else if (DebugSettings.spawnChance100) {
				rnd = 0f;
			}
		}

		if (InstanceManager.player != null && enabledByTier) {			
			return rnd <= m_activationChance;
		}
		return false;
	}

	public bool IsReadyToSpawn(float _time, float _xp) {
		bool startConditionsOk = (m_activationTriggers.Length == 0) && (m_activationKillTriggers.Length == 0);	// If there are no activation triggers defined, the spawner will be considered ready

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
		}

		for (int i = 0; i < m_activationKillTriggers.Length; i++) {
			string cat = m_activationKillTriggers[i].category;

			if (RewardManager.categoryKillCount.ContainsKey(cat)) {
				startConditionsOk |= RewardManager.categoryKillCount[cat] >= m_activationKillTriggers[i].value;
			}
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
		}

		return !endConditionsOk;
	}
}
