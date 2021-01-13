using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineShieldman : MachineGround {
		[SeparatorAttribute("Shield")]
		[SerializeField] private Collider m_shield;
		[CommentAttribute("Shield will block dragon and fire from tier equal or lower than this.")]
		[SerializeField] private DragonTier m_shieldTier;

		private GameSceneControllerBase m_gameSceneController;
		private float m_hitTime;

		public override void Spawn(ISpawner _spawner) {			
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			m_shield.enabled = (tier <= m_shieldTier);

			m_gameSceneController = InstanceManager.gameSceneControllerBase;
			m_hitTime = 0f;

			base.Spawn(_spawner);
		}

		protected override void OnCollisionEnter(Collision _collision) {
			base.OnCollisionEnter(_collision);

			if (m_gameSceneController.elapsedSeconds > m_hitTime) {
				m_viewControl.Impact();
				m_hitTime = m_gameSceneController.elapsedSeconds + 2f;
			}
		}
	}
}