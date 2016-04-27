using UnityEngine;

public class ResultsSceneController : MonoBehaviour {
	[SerializeField] private Camera m_mainCamera;
	[SerializeField] private Camera m_resultsCamera;
	[SerializeField] private CameraSnapPoint m_cameraSnapPoint;

	[SerializeField] private GameObject m_gameUI;
	[SerializeField] private GameObject m_resultsUI;

	void OnEnable() {
		
		m_mainCamera.gameObject.SetActive(false);
		m_resultsCamera.gameObject.SetActive(true);
		m_cameraSnapPoint.Apply(m_resultsCamera);

		m_gameUI.SetActive(false);
		m_resultsUI.SetActive(true);

	}
}
