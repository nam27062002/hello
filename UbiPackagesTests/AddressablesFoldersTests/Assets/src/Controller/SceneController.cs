using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private ApplicationController.EScene m_scene;

	// Use this for initialization
	void Start ()
    {
        ApplicationController.Instance.SetScene(m_scene);	
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
