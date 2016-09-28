using UnityEngine;
using System.Collections.Generic;


namespace AI {
	public class MachineInflammable : MachineComponent {
		//-----------------------------------------------
		// Constants
		//-----------------------------------------------
		enum State {
			Idle = 0,
			Burned,
			Ashes
		};


		//-----------------------------------------------
		//
		//-----------------------------------------------
		[SerializeField] private string m_ashesAsset = "";
		[SerializeField] private float m_dissolveTime = 1.5f;


		//-----------------------------------------------
		//
		//-----------------------------------------------
		private List<Material[]> m_ashMaterials = new List<Material[]>();
		private Renderer[] m_renderers;

		private float m_timer;
		private State m_state;
		private State m_nextState;


		//-----------------------------------------------
		public MachineInflammable() {}

		public override void Init() {
			// Renderers And Materials
			if (m_renderers == null) {
				m_renderers = m_machine.GetComponentsInChildren<Renderer>();

				if (m_renderers.Length > 0) {
					for(int i = 0; i < m_renderers.Length; i++) {
						Renderer renderer = m_renderers[i];
						Material[] materials = new Material[renderer.materials.Length];

						for (int j = 0; j < renderer.materials.Length; j++) {
							Material newMat = null;
							string shaderName = renderer.materials[j].shader.name;

							if (shaderName.EndsWith("Additive")) {
								// We will set to null and hide it at the beggining 
								newMat = new Material(Resources.Load ("Game/Assets/Materials/Transparent") as Material);
							} else if (shaderName.EndsWith("Bird")) {
								// We ignore mask because its used for masking the diffuse texture
								newMat = new Material(Resources.Load ("Game/Assets/Materials/BurnToAshes") as Material);
							} else {
								newMat = new Material(Resources.Load ("Game/Assets/Materials/BurnToAshes") as Material);
								newMat.SetTexture("_AlphaMask", m_renderers[i].material.mainTexture);
							}

							newMat.renderQueue = 3000;
							materials[j] = newMat;
						}
						m_ashMaterials.Add(materials);
					}
				}
			}

			m_state = State.Idle;
			m_nextState = State.Idle;
		}

		public void Burn(Transform _transform) {
			// raise flags
			m_machine.SetSignal(Signals.Type.Burning, true);
			m_machine.SetSignal(Signals.Type.Panic, true);

			// change materials
			for (int i = 0; i < m_renderers.Length; i++) {
				if (m_ashMaterials[i] != null) {
					m_renderers[i].materials = m_ashMaterials[i];
				} else {
					m_renderers[i].enabled = false;
				}
			}

			// throw particles or explode
			// 


			// reward
			Reward reward = (m_entity as Entity).GetOnKillReward(true);
			Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, m_machine.transform, reward);

			//
			m_nextState = State.Burned;
		}


		public override void Update() {
			if (m_state != m_nextState) {
				ChangeState();
			}

			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_timer = 0f;
			}
				
			switch (m_state) {
				case State.Burned:
					if (m_timer <= 0f) {
						m_nextState = State.Ashes;
					}
					break;

				case State.Ashes:
					if (m_timer <= 0f) {
						m_machine.SetSignal(Signals.Type.Destroyed, true);
						UpdateAshLevel(0);
					} else {
						UpdateAshLevel(m_timer / m_dissolveTime);
					}
					break;
			}
		}

		private void ChangeState() {
			if (m_state == State.Burned) {
				if (m_ashesAsset.Length > 0) {					
					GameObject particle = ParticleManager.Spawn(m_ashesAsset, m_renderers[0].transform.position, "Ashes");
					if (particle) {
						particle.transform.rotation = m_renderers[0].transform.rotation;
						particle.transform.localScale = m_renderers[0].transform.localScale;
					}
				}
			}

			if (m_nextState == State.Burned) {				
				m_timer = 0.5f; //secs
				UpdateAshLevel(1);
			} else if (m_nextState == State.Ashes) {
				m_timer = m_dissolveTime;		
			}

			m_state = m_nextState;
		}

		private void UpdateAshLevel( float delta )
		{
			float ashLevel = Mathf.Min(1, Mathf.Max(0, 1 - delta));
			for (int i = 0; i < m_ashMaterials.Count; i++) {
				Material[] mats = m_ashMaterials[i];
				for (int j = 0; j < mats.Length; j++) {
					mats[j].SetFloat("_AshLevel", ashLevel);
				}
			}
		}
	}
}