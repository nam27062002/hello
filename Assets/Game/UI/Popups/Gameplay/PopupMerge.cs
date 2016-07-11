using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PopupController))]
public class PopupMerge : MonoBehaviour 
{

	public static readonly string PATH = "UI/Popups/Merge/PF_PopupMerge";

	public SaveDataPill m_leftPill;
	public SaveDataPill m_rightPill;

	PopupController m_controller;

	UserProfile m_otherData;

	/*
	PopupMerge()
	{
		Messenger.AddListener(GameEvents.MERGE_SERVER_SAVE_DATA, onMergeSaveData);
	}

	protected void OnDestroy() 
	{
		Messenger.RemoveListener(GameEvents.MERGE_SERVER_SAVE_DATA, onMergeSaveData);
	}
	*/

	public void onMergeSaveData()
	{
		gameObject.SetActive(true);
		m_controller = GetComponent<PopupController>();
		m_leftPill.Setup( UsersManager.currentUser );

		m_otherData = new UserProfile();
		m_otherData.Load(GameServerManager.SharedInstance.GetLastRecievedUniverse());
		m_rightPill.Setup( m_otherData );
	}

	public void OnUseLeftOption()
	{
		UsersManager.currentUser.saveCounter = Mathf.Max( UsersManager.currentUser.saveCounter, m_otherData.saveCounter );
		PersistenceManager.Save();
		GameServerManager.SharedInstance.CleanLastRecievedUniverse();

		m_controller.Close(true);
	}

	public void OnUseRightOption()
	{
		// Here we have problems because we need to reload
		UsersManager.currentUser.Load( GameServerManager.SharedInstance.GetLastRecievedUniverse() );
		PersistenceManager.Save();
		GameServerManager.SharedInstance.CleanLastRecievedUniverse();

		// Reload? Can we do something to avoid reloading?

		m_controller.Close(true);
	}
}
