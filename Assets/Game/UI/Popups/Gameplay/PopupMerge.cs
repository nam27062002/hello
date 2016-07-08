using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PopupController))]
public class PopupMerge : MonoBehaviour 
{

	public static readonly string PATH = "UI/Popups/Merge/PF_PopupMerge";

	public SaveDataPill m_leftPill;
	public SaveDataPill m_rightPill;

	PopupController m_controller;

	UserProfile otherData;

	protected void Awake () 
	{
		m_controller = GetComponent<PopupController>();
		Messenger.AddListener(GameEvents.MERGE_SERVER_SAVE_DATA, onMergeSaveData);
	}

	protected void OnDestroy() 
	{
		Messenger.RemoveListener(GameEvents.MERGE_SERVER_SAVE_DATA, onMergeSaveData);
	}


	void onMergeSaveData()
	{
		m_leftPill.Setup( UsersManager.currentUser );

		otherData = new UserProfile();
		otherData.Load(GameServerManager.SharedInstance.GetLastRecievedUniverse());
		m_rightPill.Setup( otherData );

		m_controller.Open();
	}
}
