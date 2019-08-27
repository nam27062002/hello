// SpawnersCaptureTool.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialized capture tool to take snapshots of spawners.
/// </summary>
public class SpawnersCaptureTool : CaptureTool {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	protected const string DRAG_CONTROL_VALUE = ".DragControlValue";	

	private class PrefabData {
		public string name = "";
		public string displayName = "";
		public string path = "";
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Spawner Setup")]
	[SerializeField] private Dropdown m_dropdown = null;
	[SerializeField] private PrefabLoader m_loader = null;
	[SerializeField] private DragControlRotation m_dragControl = null;

	private string selectedSpawnerName {
		get { return m_prefabsList[m_dropdown.value].name; }
	}

	// Internal
	private List<PrefabData> m_prefabsList = new List<PrefabData>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		// Call parent
		base.Start();

		// Same for localization
		LocalizationManager.SharedInstance.SetLanguage("lang_english");

		// Gather all prefabs within the root path recursively
		string dirPath = Application.dataPath + IEntity.ENTITY_PREFABS_PATH;
		DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
		ScanFiles(dirInfo, "*.prefab");

		// Sort files!
		// Let's do it alphabetically for now
		// Alphanumeric sorting is quite complex, use auxiliar class to do it
		AlphanumComparatorFast comparer = new AlphanumComparatorFast();
		m_prefabsList.Sort((x, y) => comparer.Compare(x.displayName, y.displayName));

		// Initialize dropdown
		// For clarity, show pet name next to its sku
		List<string> options = new List<string>(m_prefabsList.Count);
		for(int i = 0; i < m_prefabsList.Count; ++i) {
			options.Add(m_prefabsList[i].displayName);
		}
		m_dropdown.ClearOptions();
		m_dropdown.AddOptions(options);
		m_dropdown.value = 0;

		// Setup drag control
		m_dragControl.value = Prefs.GetVector2Editor(GetKey(DRAG_CONTROL_VALUE));
		m_dragControl.forceInitialValue = true;
		m_dragControl.initialValue = m_dragControl.value;
		m_dragControl.InitFromTarget(m_loader.transform, true);
		m_dragControl.forceInitialValue = false;

		// Load initial spawner
		OnLoadButton();
	}

	/// <summary>
	/// Component is about to be disabled.
	/// </summary>
	override protected void OnDisable() {
		// Store some values to prefs
		Prefs.SetVector2Editor(GetKey(DRAG_CONTROL_VALUE), m_dragControl.value);

		// Call parent
		base.OnDisable();
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Put the camera at the default position, rotation, fov.
	/// </summary>
	protected override void ResetCamera() {
		m_mainCamera.transform.SetPositionAndRotation(
			new Vector3(0f, 0f, -10f),
			Quaternion.Euler(0f, 0f, 0f)
		);

		m_mainCamera.fieldOfView = 30f;
	}

	/// <summary>
	/// Get the filename of the screenshot. No extension, can include subfolders.
	/// </summary>
	/// <returns>The filename of the screenshot.</returns>
	override protected string GetFilename() {
		return selectedSpawnerName;	// Each pet sku is already unique
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Scan all the files (recursively) within a folder and adds them both to the m_prefabs list.
	/// Doesn't clear the list.
	/// Unity's *.meta files will be excluded.
	/// </summary>
	/// <param name="_dirInfo">Directory to scan.</param>
	/// <param name="_filter">The search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
	/// <param name="_optionsPrefix">The prefix to be used when adding the file name to the options list. Used for nesting the foldable menu.</param>
	private void ScanFiles(DirectoryInfo _dirInfo, string _filter, string _prefix = "") {
		// Scan target dir's files
		FileInfo[] files = _dirInfo.GetFiles(_filter).Where(_file => !_file.Extension.EndsWith(".meta")).ToArray();	// Use file filter, exclude .meta files

		// Clean up displayed names and add to the options list as well
		// Add files to files list!
		for(int i = 0; i < files.Length; i++) {
			PrefabData prefabData = new PrefabData();
			prefabData.name = Path.GetFileNameWithoutExtension(files[i].Name);
			prefabData.displayName = prefabData.name;
			prefabData.path = IEntity.ENTITY_PREFABS_PATH + _prefix + prefabData.name;	// Strip filename from full file path and attach prefix
			Debug.Log(prefabData.path);
			m_prefabsList.Add(prefabData);
		}

		// Recursively scan subdirs as well
		DirectoryInfo[] dirs = _dirInfo.GetDirectories();
		for(int i = 0; i < dirs.Length; i++) {
			// Update prefix (so all files in this subdir are in nested menus
			string prefix = _prefix + dirs[i].Name + "/";
			ScanFiles(dirs[i], _filter, prefix);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reload dragon preview button has been pressed.
	/// </summary>
	public void OnLoadButton() {
		PrefabData selectedSpawner = m_prefabsList[m_dropdown.value];
		m_loader.Load(selectedSpawner.path);

		// Spawners need an extra material initialization!
		if(m_loader.loadedInstance != null) {
			ViewControl vc = m_loader.loadedInstance.GetComponent<ViewControl>();
			if (vc != null) {
				vc.SetMaterialType(ViewControl.MaterialType.NORMAL);
			}
		}
	}
}