using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachineShield : MachineOld {
		[SeparatorAttribute("Shield")]
		[SerializeField] private Collider m_shield;
		[CommentAttribute("Shield will block dragon and fire from tier equal or lower than this.")]
		[SerializeField] private DragonTier m_shieldTier;


		public override void Spawn(ISpawner _spawner) {
			DragonPlayer player = InstanceManager.player;
			DragonTier tier = player.GetTierWhenBreaking();

			m_shield.enabled = (tier <= m_shieldTier);

			base.Spawn(_spawner);
		}
	}
}