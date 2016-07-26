using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PopupController))]
public class PopupMerge : MonoBehaviour 
{

	public static readonly string PATH = "UI/Popups/Merge/PF_PopupMerge";

	public PopupMergeProfilePill m_leftPill;
	public PopupMergeProfilePill m_rightPill;

	UserProfile m_profile1;
	UserProfile m_profile2;

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

	/// <summary>
	/// Setup the popup with the two given user profiles.
	/// </summary>
	/// <param name="_profile1">First profile, usually the player's current profile.</param>
	/// <param name="_profile2">Second profile, usually the profile received from the server.</param>
	public void Setup(UserProfile _profile1, UserProfile _profile2) {
		// Initialize left pill with profile 1
		m_profile1 = _profile1;
		m_leftPill.Setup(_profile1, _profile2);

		// Initialize right pill with the other profile
		m_profile2 = _profile2;
		m_rightPill.Setup(_profile2, _profile1);
	}

	public void OnMergeSaveData()
	{
		// Initialize popup using the current user's profile and the one received from the server
		UserProfile p2 = new UserProfile();
		p2.Load(GameServerManager.SharedInstance.GetLastRecievedUniverse());
		Setup(UsersManager.currentUser, p2);
	}

	public void OnUseLeftOption()
	{
		ChooseProfile(m_profile1);
	}

	public void OnUseRightOption()
	{
		ChooseProfile(m_profile2);
	}

	/// <summary>
	/// Perform all the required actions once a profile has been chosen.
	/// </summary>
	/// <param name="_chosenProfile">The chosen profile.</param>
	private void ChooseProfile(UserProfile _chosenProfile) {
		// Special treatment if the chosen profile is the current one
		bool isCurrent = (_chosenProfile == UsersManager.currentUser);
		if(isCurrent) {
			// Update save counter to the latest one
			UsersManager.currentUser.saveCounter = Mathf.Max(m_profile1.saveCounter, m_profile2.saveCounter);
		} else {
			// Load into current user
			UsersManager.currentUser.Load(_chosenProfile.ToJson());	// [AOC] This is a bit hacky, probably we should have a Load() method on UserProfile with another UserProfile as a parameter
		}

		// Flush server data and save new persistence
		GameServerManager.SharedInstance.CleanLastRecievedUniverse();
		GameServerManager.SharedInstance.saveDataRecovered = true;
		PersistenceManager.Save();

		// Close and destroy popup
		GetComponent<PopupController>().Close(true);

		// If loading a new profile, restart game
		if(!isCurrent) {
			FlowManager.Restart();
		}
	}

	/// <summary>
	/// For testing purposes!
	/// </summary>
	public void Test() {
		UserProfile p1 = new UserProfile();
		TextAsset universe1 = Resources.Load("__TEMP/test_universe_1") as TextAsset;
		SimpleJSON.JSONClass json1 = SimpleJSON.JSONNode.Parse(universe1.ToString()) as SimpleJSON.JSONClass;
		p1.Load(json1);
		Debug.Log("PROFILE 1:\n" + p1.ToString());

		UserProfile p2 = new UserProfile();
		TextAsset universe2 = Resources.Load("__TEMP/test_universe_2") as TextAsset;
		SimpleJSON.JSONClass json2 = SimpleJSON.JSONNode.Parse(universe2.ToString()) as SimpleJSON.JSONClass;
		p2.Load(json2);
		Debug.Log("PROFILE 2:\n" + p2.ToString());

		Setup(p1, p2);
	}
}
