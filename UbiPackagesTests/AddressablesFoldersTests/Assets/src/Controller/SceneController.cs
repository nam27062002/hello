using UnityEngine;

public class SceneController : MonoBehaviour
{
    [SerializeField]
    private ApplicationController.EScene m_scene;

	// Use this for initialization
	void Start ()
    {
        ApplicationController.Instance.SetScene(m_scene);

        GameObject prefab = ApplicationController.Instance.AddressablesManager.LoadAsset<GameObject>("PF_ConfigSprite");
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab);
            ConfigSprite configSprite = go.GetComponent<ConfigSprite>();
            if (configSprite != null)
            {
                configSprite.Setup("village-castle");
            }
        }
	}		
}
