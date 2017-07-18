using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : MachineAir {
		//--------------------------------------------------
		[SeparatorAttribute("Phoenix Effects")]
		[SerializeField] private GameObject m_fire;
		[SerializeField] private List<GameObject> m_fireView;
		[SerializeField] private Renderer m_bodyRenderer;

		//--------------------------------------------------
		private bool m_phoenixActive = false;
		private Material m_phoenixMaterial;
		//--------------------------------------------------
		protected override void Awake() {
			base.Awake();
			m_phoenixMaterial = m_bodyRenderer.material;
			DeactivateFire();
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// Set not fire view
			DeactivateFire();
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			if (!m_phoenixActive) {
				if (m_pilot.IsActionPressed(Pilot.Action.Fire)) {	
					// Activate Phoenix Mode!!
					ActivateFire();
				}

			} else {
				if (!m_pilot.IsActionPressed(Pilot.Action.Fire)) {
					// Deactivate Phoenix Mode
					DeactivateFire();
				}
			}
		}

		private void ActivateFire() {
			m_phoenixActive = true;
			m_fire.SetActive(true);
			for( int i = 0; i<m_fireView.Count; ++i)
				m_fireView[i].SetActive(true);
			StopCoroutine("FireTo");
			StartCoroutine( FireTo(1) );
		}

		private void DeactivateFire() {
			m_phoenixActive = false;
			m_fire.SetActive(false);
			for( int i = 0; i<m_fireView.Count; ++i)
				m_fireView[i].SetActive(false);
			StopCoroutine("FireTo");
			StartCoroutine( FireTo(0) );
		}

		IEnumerator FireTo( float _targetValue )
		{
			float startValue = m_phoenixMaterial.GetFloat("_FireAmount");
			float timer = 0;
			float duration = 0.5f;
			while( timer < duration )
			{
				yield return null;
				timer += Time.deltaTime;
				float delta = timer / duration;

				m_phoenixMaterial.SetFloat("_FireAmount", Mathf.Lerp(startValue, _targetValue, delta));
			}
			m_phoenixMaterial.SetFloat("_FireAmount", _targetValue);
		}
	}
}