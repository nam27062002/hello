// Mission3DIconPreview.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 16/05/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class Mission3DIconPreview  : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    private const string MISSION_3D_ICONS_PATH = "Assets/Art/3D/Gameplay/Entities/PrefabsMenu/";

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed
    public Dropdown dropDown;
    public UI3DAddressablesLoader labMission3DIcon, mission3DIcon;


    // Internal
    [SerializeField] private List<string> prefabs;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

	public void Awake()
	{
		HDAddressablesManager.Instance.Initialise();
	}

    public void Start()
    {

        // Get all the assets in the path
        DirectoryInfo dirInfo = new DirectoryInfo(MISSION_3D_ICONS_PATH);
        FileInfo[] files = dirInfo.GetFiles();

        // Strip filename from full file path
        prefabs = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith("prefab", true, System.Globalization.CultureInfo.InvariantCulture))
            {
                prefabs.Add(Path.GetFileNameWithoutExtension(files[i].Name));
            }
        }


        Debug.Log(string.Format("{0} prefabs found", prefabs.Count));


        // Populate dropw down
        List<string> options = new List<string>();

        // Add first row with hint
        options.Add("Select prefab...");

        // Add available prefabs
        foreach (string asset in prefabs)
        {
            options.Add(asset); // Or whatever you want for a label
        }

        dropDown.ClearOptions();
        dropDown.AddOptions(options);

		//AddressablesOp op = HDAddressablesManager.Instance.LoadAssetAsync("Air/PF_Canary01_Flock");
		//op.OnDone = OnAssetLoaded;
    }

	private void OnAssetLoaded(AddressablesOp op)
	{
		GameObject prefab = op.GetAsset<GameObject>();
	}

	public void Update()
	{
		HDAddressablesManager.Instance.Update ();
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    public void OnSelectorChanged ()
    {
        // Ignore the option "Select prefab..."
        if (dropDown.value != 0)
        {

            // Put the selected prefab in the horizontal mission pill (lab missions)
            labMission3DIcon.LoadAsync(prefabs[dropDown.value - 1]);


            // Put the selected prefab in the vertical mission pill
            mission3DIcon.LoadAsync(prefabs[dropDown.value - 1]);

        }

        

    }
}