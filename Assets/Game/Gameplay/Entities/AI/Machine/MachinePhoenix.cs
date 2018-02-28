using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : MachineAir {
		//--------------------------------------------------
		[SeparatorAttribute("Phoenix Effects")]
		[SerializeField] private GameObject m_fire;
		[SerializeField] private string m_fireParticleName;
		[SerializeField] private string m_fireAnchorName;
		[SerializeField] private Renderer m_bodyRenderer;

		private GameObject m_fireView;

		//--------------------------------------------------
		private bool m_phoenixActive = false;
		private Material m_phoenixMaterial = null;
		//--------------------------------------------------
		protected override void Awake() {
			base.Awake();

			string version = "";
			switch(FeatureSettingsManager.instance.Particles)
			{
				default:
				case FeatureSettings.ELevel5Values.very_low:							
				case FeatureSettings.ELevel5Values.low:
						version = "Low/";
					break;
				case FeatureSettings.ELevel5Values.mid:
						version = "Master/";
					break;
				case FeatureSettings.ELevel5Values.very_high:
				case FeatureSettings.ELevel5Values.high:
						version = "High/";
					break;
			}

			string path = "Particles/" + version + m_fireParticleName;

			GameObject prefab = Resources.Load<GameObject>(path);
			if ( prefab )
			{
				m_fireView = Instantiate<GameObject>(prefab);
				if ( m_fireView )
				{
					// Anchor
					Transform p = transform.FindTransformRecursive(m_fireAnchorName);
					m_fireView.transform.SetParent(p, true);
					m_fireView.transform.localPosition = GameConstants.Vector3.zero;
					m_fireView.transform.localRotation = GameConstants.Quaternion.identity;
				}
			}

			DeactivateFire();
		}

		void Start(){
			m_phoenixMaterial = m_bodyRenderer.material;
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
			if ( m_fireView )
				m_fireView.SetActive(true);
			// StopCoroutine("FireTo");
			// StartCoroutine( FireTo(1) );
		}

		private void DeactivateFire() {
			m_phoenixActive = false;
			m_fire.SetActive(false);
			if ( m_fireView )
				m_fireView.SetActive(false);
			// StopCoroutine("FireTo");
			// StartCoroutine( FireTo(0) );
		}

		/*
		IEnumerator FireTo( float _targetValue )
		{
			if ( m_phoenixMaterial != null )
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
		*/
	}
}