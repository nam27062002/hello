using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
	public class MachinePhoenix : MachineAir {
		//--------------------------------------------------
		[SeparatorAttribute("Phoenix Effects")]
		[SerializeField] private GameObject m_fire;
		[SerializeField] private ParticleSystem m_fireParticle;
		[SerializeField] private Renderer m_bodyRenderer;

		//--------------------------------------------------
		private bool m_phoenixActive = false;
		private Material m_phoenixMaterial;
		private float m_originalFresnel;
		private Color m_originalFresnelColor;

		float m_phoenixFresnelValue = 0.5f;
		Color m_phoenixFresnelColor = new Color(240/255.0f,140/255.0f,12/255.0f);
		//--------------------------------------------------
		protected override void Awake() {
			base.Awake();
			m_phoenixMaterial = m_bodyRenderer.material;
			m_originalFresnel = m_phoenixMaterial.GetFloat("_Fresnel");
			m_originalFresnelColor = m_phoenixMaterial.GetColor("_FresnelColor");
			Deactivate();
		}

		public override void Spawn(ISpawner _spawner) {
			base.Spawn(_spawner);

			// Set not fire view
			Deactivate();
		}

		public override void CustomUpdate() {
			base.CustomUpdate();

			if (!m_phoenixActive) {
				if (m_pilot.IsActionPressed(Pilot.Action.Fire)) {	
					// Activate Phoenix Mode!!
					Activate();
				}

			} else {
				if (!m_pilot.IsActionPressed(Pilot.Action.Fire)) {
					// Deactivate Phoenix Mode
					Deactivate();
				}
			}
		}

		private void Activate() {
			m_phoenixActive = true;
			m_fire.SetActive(true);
			m_fireParticle.Play();
			StopCoroutine("FresnelToDeactive");
			StartCoroutine( FresnelTo( m_phoenixFresnelValue, m_phoenixFresnelColor));
		}

		private void Deactivate() {
			m_phoenixActive = false;
			m_fire.SetActive(false);
			m_fireParticle.Stop();
			StopCoroutine("FresnelToActive");
			StartCoroutine( FresnelTo( m_originalFresnel, m_originalFresnelColor ));
		}

		IEnumerator FresnelTo( float _targetValue, Color _targetColor)
		{
			float startValue = m_phoenixMaterial.GetFloat("_Fresnel");
			Color startColor = m_phoenixMaterial.GetColor("_FresnelColor");
			float timer = 0;
			float duration = 0.5f;
			while( timer < duration )
			{
				yield return null;
				timer += Time.deltaTime;
				float delta = timer / duration;

				m_phoenixMaterial.SetFloat("_Fresnel", Mathf.Lerp(startValue, _targetValue, delta));
				m_phoenixMaterial.SetColor("_FresnelColor", Color.Lerp(startColor, _targetColor, delta));
			}
			m_phoenixMaterial.SetFloat("_Fresnel", _targetValue);
			m_phoenixMaterial.SetColor("_FresnelColor", _targetColor);
		}
	}
}