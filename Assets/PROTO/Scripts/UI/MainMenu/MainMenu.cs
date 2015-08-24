// MainMenu.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// End of game popup.
/// </summary>
public class MainMenu : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------

	#endregion

	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	public GameObject dragonPreviewObj;
	public string[] skinNames;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private SkinnedMeshRenderer mDragonBodyMesh = null;
	private SkinnedMeshRenderer mDragonWingsMesh = null;

	GameObject dragonView = null;
	Transform dragonPivot;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization
	/// </summary>
	void Start() {
		// Initialize object references
		//mDragonBodyMesh = dragonPreviewObj.FindSubObject("dragon_mesh").GetComponent<SkinnedMeshRenderer>();
		//mDragonWingsMesh = dragonPreviewObj.FindSubObject("dragon_wings").GetComponent<SkinnedMeshRenderer>();

		// Apply initial skin
		//LoadSkin(GameSettings.skinName);

		dragonPivot = GameObject.Find ("DragonPivot").transform;
		ReloadDragon();
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	void Update() {
		
	}
	#endregion

	#region INTERNAL UTILS ---------------------------------------------------------------------------------------------
	/// <summary>
	/// Load a specific skin into the dragon's preview.
	/// </summary>
	/// <param name="_sSkinName">The name of the skin to be loaded.</param>
	void LoadSkin(string _sSkinName) {
		// Load both materials
		Material bodyMat = Resources.Load<Material>("Materials/Dragon/MT_dragon_" + _sSkinName + "_bump");
		Material wingsMat = Resources.Load<Material>("Materials/Dragon/MT_dragon_" + _sSkinName + "_alphaTest");

		// Apply body material
		mDragonBodyMesh.material = bodyMat;
		mDragonWingsMesh.material = wingsMat;
	}

	void ReloadDragon(){
	
		if (dragonView != null)
			DestroyObject(dragonView);

		dragonView = (GameObject)Object.Instantiate(Resources.Load ("Dragons/Menu"+GameSettings.dragonType));
		dragonView.transform.SetParent (dragonPivot,false);
	}

	#endregion

	#region BUTTON CALLBACKS -------------------------------------------------------------------------------------------
	/// <summary
	/// The play button has been clicked.
	/// </summary>
	public void OnPlayFuryClick() {
		// Go to main menu
		App.Instance.flowManager.GoToScene(FlowManager.EScenes.GAME);
	}

	/// <summary>
	/// One of the buttons to change the dragon's skin has been clicked.
	/// </summary>
	/// <param name="_sSkinName">The skin to be selected.</param>
	public void OnSkinChangeClick(string _sSkinName) {
		// Skip if skin is already selected
		if(_sSkinName == GameSettings.skinName) return;

		// Load the skin into the preview
	//	LoadSkin(_sSkinName);

		// Tell the game settings which skin to load upon starting the game
		GameSettings.skinName = _sSkinName;
	}

	/// <summary>
	/// One of the buttons to change the dragon's skin has been clicked.
	/// </summary>
	/// <param name="_sSkinName">The skin to be selected.</param>
	public void OnDragonChangeClick(int _iDragonType) {

		// [PAC] Don't change skin, just change dragon
		OnSkinChangeClick(skinNames[_iDragonType-1]);

		// Skip if dragon is already selected
		string sDragon = "Dragon "+_iDragonType.ToString();
		if(GameSettings.dragonType.Equals(sDragon)) return;

		// Tell the game settings which skin to load upon starting the game
		GameSettings.dragonType = sDragon;

		ReloadDragon();
	}
	#endregion
}
#endregion
