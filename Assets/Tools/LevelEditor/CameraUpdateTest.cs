using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class CameraUpdateTest : MonoBehaviour {

	[SerializeField] private GameCamera m_camera;
	[SerializeField] private GameObject m_canvas;


	private string m_currentDragon = "";

	// Update is called once per frame
	void Update () {
		if (!Application.isPlaying) {
			Camera editor = Camera.current;

			if (m_currentDragon != LevelEditor.LevelEditor.settings.testDragon) {
				// If definitions are not loaded, do it now
				if(!ContentManager.ready){
					ContentManager.InitContent(true, false);
				}

				DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, LevelEditor.LevelEditor.settings.testDragon);
				float size = dragonDef.GetAsFloat("defaultSize");
				float cameraFrameWidthModifier = dragonDef.GetAsFloat("cameraFrameWidthModifier");

				float frameWidth = m_camera.GetFrameWidth(size, cameraFrameWidthModifier);
				m_camera.Snap();
				m_camera.UpdatePixelData();
				m_camera.UpdateZooming(frameWidth, false);
				m_camera.UpdateFOV();

				m_currentDragon = LevelEditor.LevelEditor.settings.testDragon;
			}

			if (editor != null) {
				if (m_camera != null) {
					Vector3 pos = editor.transform.position;
					pos.z = m_camera.position.z;
					m_camera.transform.position = pos;
				}
			}

			if (m_canvas != null) {
				m_canvas.SetActive(false);
			}
		}
	}
}
