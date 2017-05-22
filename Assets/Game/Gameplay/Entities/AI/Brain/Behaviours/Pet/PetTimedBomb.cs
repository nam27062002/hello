using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[CreateAssetMenu(menuName = "Behaviour/Pet/Timed Bomb")]
		public class PetTimedBomb : StateComponent {
			[System.Serializable]
			public class PetTimedBombData : StateComponentData {
				public float m_bombArea;
				public DragonTier m_bombTier;
				public Range m_waitingTime;
			}

			public override StateComponentData CreateData() {
				return new PetTimedBombData();
			}

			public override System.Type GetDataType() {
				return typeof(PetTimedBombData);
			}

			PetTimedBombData m_data;
			private float m_timer = 0;
			Rect m_rect;
			protected PreyAnimationEvents m_animEvents;
			private ViewControl m_viewControl;

			protected override void OnInitialise() {
				m_data = m_pilot.GetComponentData<PetTimedBombData>();
				m_timer = m_data.m_waitingTime.GetRandom();
				m_rect = Rect.zero;

				m_animEvents 	= m_pilot.FindComponentRecursive<PreyAnimationEvents>();
				if ( m_animEvents )
					m_animEvents.onAttackDealDamage += new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);

				m_viewControl = m_pilot.GetComponent<ViewControl>();
			}

			protected override void OnRemove()
			{
				if ( m_animEvents )
					m_animEvents.onAttackDealDamage -= new PreyAnimationEvents.OnAttackDealDamageDelegate(OnAnimDealDamage);
			}

			private void OnAnimDealDamage() {
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
				m_viewControl.PlayExplosion();
				DealDamage();
			}

			protected override void OnUpdate()
			{
				m_timer -= Time.deltaTime;
				if ( m_timer <= 0 )
				{
					if ( m_animEvents == null ) 
					{
						m_viewControl.PlayExplosion();
						DealDamage();
					}
					else
					{
						m_pilot.PressAction(Pilot.Action.Button_A);
					}
					m_timer = m_data.m_waitingTime.GetRandom();
				}
			}


			void DealDamage()
			{
				Entity[] preys = EntityManager.instance.GetEntitiesInRange2D( m_machine.position, m_data.m_bombArea);
				for (int i = 0; i < preys.Length; i++) {
					if (preys[i].IsBurnable(m_data.m_bombTier)) {
						AI.IMachine machine =  preys[i].machine;
						if (machine != null) {
							machine.Burn(m_machine.transform);
						}
					}
				}
				m_rect.center = m_machine.position;
				m_rect.height = m_rect.width = m_data.m_bombArea;
				FirePropagationManager.instance.FireUpNodes( m_rect, Overlaps, m_data.m_bombTier, Vector3.zero);
			}


			bool Overlaps( CircleAreaBounds _fireNodeBounds )
			{
				return _fireNodeBounds.Overlaps(m_machine.position, m_data.m_bombArea);
			}
		}
	}
}