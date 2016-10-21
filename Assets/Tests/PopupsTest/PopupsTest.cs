using UnityEngine;

public class PopupsTest : SceneController
{	
	void Start ()
    {
        // Rules and language are initialized so the content of the popups tested in this scene can be right
        ContentManager.InitContent();
        LoadingSceneController.SetSavedLanguage();
    }		
}
