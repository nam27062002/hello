using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class CameraUpdateTest : MonoBehaviour {

	[SerializeField] private GameCamera m_camera;
	[SerializeField] private GameObject m_canvas;

	[SerializeField] private bool m_enabledFog = true;

	private bool m_initCamera = true;
	private string m_currentDragon = "";
	private float m_cameraSize = 0f;
	private float m_cameraFrameWidth = 0f;

	private FogManager m_fogManager = null;

	// Update is called once per frame
	void Update () {
		if (!Application.isPlaying) {
			Camera editor = Camera.current;

			if (m_initCamera) {
				m_camera.PublicAwake();
				m_initCamera = false;
			}

			if (m_currentDragon != LevelEditor.LevelEditor.settings.testDragon) {
				// If definitions are not loaded, do it now
				if(!ContentManager.ready){
					ContentManager.InitContent(true, false);
				}

				DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, LevelEditor.LevelEditor.settings.testDragon);
				m_cameraSize = dragonDef.GetAsFloat("defaultSize");
				m_cameraFrameWidth = dragonDef.GetAsFloat("cameraFrameWidthModifier");

				m_currentDragon = LevelEditor.LevelEditor.settings.testDragon;
			}

		
			float frameWidth = m_camera.GetFrameWidth(m_cameraSize, m_cameraFrameWidth);
			m_camera.Snap(false);
			m_camera.UpdatePixelData();
			m_camera.UpdateZooming(frameWidth, false);
			m_camera.UpdateFOV();

			if (editor != null) {
				if (m_camera != null) {
					Vector3 pos = editor.transform.position;
					pos.z = m_camera.position.z;
					m_camera.transform.position = pos;
				}
			
				if (m_enabledFog) {
					if (m_fogManager == null) {
						m_fogManager = m_camera.GetComponent<FogManager>();
					}

					if (m_fogManager != null) {		
						bool useDefault = true;
						Vector3 pos = editor.transform.position;
						pos.z = 0f;
                        Collider[] colliders = Physics.OverlapSphere(pos, 5f, GameConstants.Layers.TRIGGERS, QueryTriggerInteraction.Collide);
						
						for (int i = 0; i < colliders.Length; ++i) {
							FogArea fog = colliders[i].GetComponent<FogArea>();
							if (fog != null) {
								fog.EditorFogSetup();
								useDefault = false;
								break;
							}
						}

						if (useDefault) {
							m_fogManager.RefreshFog();
						}
					}
				}
			}

			if (m_canvas != null) {
				m_canvas.SetActive(false);
			}
		}
	}

	void LateUpdate() {

	}
}
