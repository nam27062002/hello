// BabyDragonWizard.cs
// Hungry Dragon
// 
// Created by Jordi Riambau on 06/05/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

using System.Reflection;
using AI;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class BabyDragonWizard : EditorWindow
{
	// Constants
	const string PET_BASE_PATH  = "Assets/Art/3D/Gameplay/Pets/Prefabs/";

    // Enums
    enum EBabyDragonPet
    {
        Dactylus,
        Froggy,
        BallGrenade,
        BallMedic,
        Bruce,
        GhostEater,
        GodzillaBasic,
        MineEater
    }

	// Required
	Object babyDragonFBX;
	Object lastBabyDragonFBX;
	Editor gameObjectEditor;
	string sku;
	EBabyDragonPet babyDragonBasePet;
	static Dictionary<string, string> petCloneDict = new Dictionary<string, string>();
	static List<string> popupPetClone = new List<string>();

    // Optional
	string tagName = "Pet";
	Material babyDragonMaterial;

    // Main menu
	RuntimeAnimatorController runtimeAnimatorControllerMenu;
	string assetBundleMainMenu;

    // Gameplay
	RuntimeAnimatorController runtimeAnimatorControllerGameplay;
	string assetBundleGameplay;

    // Menu
	[MenuItem("Hungry Dragon/Tools/Baby Dragon Wizard...", false, -150)]
	static void Init()
	{
		BabyDragonWizard window = (BabyDragonWizard) GetWindow(typeof(BabyDragonWizard));
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Common/Icons/icon_btn_pets.png");
		window.titleContent = new GUIContent(" Baby Dragon", icon);

		string[] directory = Directory.GetDirectories(PET_BASE_PATH);
		for (int i = 0; i < directory.Length; i++)
		{
			string[] guid = AssetDatabase.FindAssets("PF_", new[] { directory[i] });
			for (int x = 0; x < guid.Length; x++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid[x]);
				string petName = path.Substring(path.LastIndexOf("/") + 1);
				petName = petName.Substring(0, petName.IndexOf(".prefab"));
				if (!petName.Contains("Menu"))
				{
					popupPetClone.Add(petName);
					petCloneDict.Add(petName, path);
					Debug.LogError("PetName: " + petName + " - " + path);
				}
			}
		}

		window.Show();
	}

    // GUI
	void OnGUI()
    {
		EditorGUILayout.HelpBox("This tool automatically creates 2 prefabs for Baby Dragons.\nIt will create the prefabs for Main Menu and Gameplay.", MessageType.Info, true);
		EditorGUILayout.LabelField("Required", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("View FBX:");
		babyDragonFBX = EditorGUILayout.ObjectField(babyDragonFBX, typeof(Object), true);
		EditorGUILayout.EndHorizontal();
		sku = EditorGUILayout.TextField("Definitions SKU:", sku);
		EditorGUILayout.BeginHorizontal();
		babyDragonBasePet = (EBabyDragonPet) EditorGUILayout.EnumPopup("Clone pet behaviour", babyDragonBasePet);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
		tagName = EditorGUILayout.TextField("Tag:", tagName);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Material:");
		babyDragonMaterial = (Material) EditorGUILayout.ObjectField(babyDragonMaterial, typeof(Material), true);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Main menu prefab settings", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Animation Controller :");
		runtimeAnimatorControllerMenu = (RuntimeAnimatorController)EditorGUILayout.ObjectField(runtimeAnimatorControllerMenu, typeof(RuntimeAnimatorController), true);
		EditorGUILayout.EndHorizontal();
		assetBundleMainMenu = EditorGUILayout.TextField("AssetBundle:", assetBundleMainMenu);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Gameplay prefab settings", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Animation Controller :");
		runtimeAnimatorControllerGameplay = (RuntimeAnimatorController)EditorGUILayout.ObjectField(runtimeAnimatorControllerGameplay, typeof(RuntimeAnimatorController), true);
		EditorGUILayout.EndHorizontal();
		assetBundleGameplay = EditorGUILayout.TextField("AssetBundle:", assetBundleGameplay);

		if (GUILayout.Button("Create prefabs...", GUILayout.Height(40)))
		{
			CreatePrefabs();
		}

        // Preview FBX
		if (babyDragonFBX != null)
		{
			if (gameObjectEditor == null || lastBabyDragonFBX != babyDragonFBX)
			{
				gameObjectEditor = Editor.CreateEditor(babyDragonFBX);
			}

			GUIStyle bgColor = new GUIStyle();
			bgColor.normal.background = EditorGUIUtility.whiteTexture;

			EditorGUILayout.Space();
			gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(512, 512), bgColor);
			lastBabyDragonFBX = babyDragonFBX;
		}
    }

    // Create main menu and gameplay prefabs
    void CreatePrefabs()
    {
		if (babyDragonFBX == null)
		{
			EditorUtility.DisplayDialog("Error", "View FBX field is required", "Close");
			return;
		}

		if (string.IsNullOrEmpty(sku))
		{
			EditorUtility.DisplayDialog("Error", "Definitions SKU field is required", "Close");
			return;
		}

		EditorUtility.DisplayProgressBar("Baby Dragon", "Creating main menu prefab...", 0.5f);
		CreateMenuPrefab();

		EditorUtility.DisplayProgressBar("Baby Dragon", "Creating gameplay prefab...", 1.0f);
		CreateGameplayPrefab();

		EditorUtility.ClearProgressBar();
    }

    void CreateMenuPrefab()
    {
        // View
        GameObject view = (GameObject) Instantiate(babyDragonFBX);
		view.name = "view";

		GameObject rootNode = FindHipBone(ref view);
        if (rootNode == null)
        {
			Debug.LogError("Root bone on FBX not found");
			return;
        }

		view.AddComponent<DragonAnimationEventsMenu>();

		// Create root object
		string rootName = "PF_Pet" + babyDragonFBX.name.Replace("_LOW", "") + "Menu";
		GameObject root = new GameObject(rootName) { tag = tagName };

        // Root - Equipable script
		root.AddComponent<Equipable>();

		// Root - Menu Pet Preview script
		MenuPetPreview menuPetPreview = root.AddComponent<MenuPetPreview>();
		foreach (FieldInfo field in menuPetPreview.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
		{
			if (field.Name == "m_rootNode")
            {
				field.SetValue(menuPetPreview, rootNode.transform);
				break;
            }
		}
        
        // Attach view as child
		view.transform.SetParentAndReset(root.transform);
		view.transform.localPosition = new Vector3(-0.214f, -1.45f, 0.0f);
		view.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);

		// Set material
		SetMaterial(ref view);

        // Set animation controller
        if (runtimeAnimatorControllerMenu != null)
			SetAnimationController(ref view, runtimeAnimatorControllerMenu);

		// Export prefab
		Export(root, rootName, assetBundleMainMenu, "Main Menu");

        // Clean-up
		DestroyImmediate(root);
	}

    string GetClonePetPath()
    {
        string path;
        switch (babyDragonBasePet)
        {
			case EBabyDragonPet.Dactylus:
				path = "Bundle_01/PF_PetDactylus_0.prefab";
				break;
			case EBabyDragonPet.Froggy:
				path = "Bundle_02/PF_PetFroggy_v5_13.prefab";
				break;
			case EBabyDragonPet.BallGrenade:
				path = "Bundle_03/PF_PetBallGrenade_18.prefab";
				break;
			case EBabyDragonPet.BallMedic:
				path = "Bundle_03/PF_PetBallMedic_42.prefab";
				break;
			case EBabyDragonPet.Bruce:
				path = "Bundle_04/PF_PetBruce_50.prefab";
				break;
			case EBabyDragonPet.GhostEater:
				path = "Bundle_05/PF_PetGhostEater_28.prefab";
				break;
			case EBabyDragonPet.GodzillaBasic:
				path = "Bundle_05/PF_PetGodzillaBasic_24.prefab";
				break;
			case EBabyDragonPet.MineEater:
				path = "Bundle_05/PF_PetMineEater_29.prefab";
				break;
			default:
				return string.Empty;
		}

		return Path.Combine(PET_BASE_PATH, path);
    }

    void CreateGameplayPrefab()
    {
        // Pet to clone behaviour
		string petClonePath = GetClonePetPath();
		if (string.IsNullOrEmpty(petClonePath))
		{
			Debug.LogError("Pet to clone was not defined: " + babyDragonBasePet);
			return;
		}

		// View
		GameObject view = (GameObject)Instantiate(babyDragonFBX);
		view.name = "view";

		GameObject rootNode = FindHipBone(ref view);
		if (rootNode == null)
		{
			Debug.LogError("Root bone on FBX not found");
			return;
		}

		view.AddComponent<DragonAnimationEvents>();

		// Create root object
		string rootName = "PF_Pet" + babyDragonFBX.name.Replace("_LOW", "");
		GameObject root = new GameObject(rootName) { tag = tagName };
		GameObject basePetModel = (GameObject)AssetDatabase.LoadAssetAtPath(petClonePath, typeof(GameObject));

		// Attach view as child
		view.transform.SetParentAndReset(root.transform);
		view.transform.localPosition = new Vector3(0.0f, -1.239f, -0.64f);

		// Set material
		SetMaterial(ref view);

		// Set animation controller
		if (runtimeAnimatorControllerGameplay != null)
			SetAnimationController(ref view, runtimeAnimatorControllerGameplay);

		// First copy components from original prefab to the new prefab
		Component[] components = basePetModel.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
			UnityEditorInternal.ComponentUtility.CopyComponent(components[i]);
			UnityEditorInternal.ComponentUtility.PasteComponentAsNew(root);
		}

		// At this point, the references on the new prefab are still pointing to the old prefab.
		// Update Pet script fields via reflection
		Pet pet = root.GetComponent<Pet>();
		foreach (FieldInfo field in pet.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
		{
            switch (field.Name)
            {
				case "m_sku":
					field.SetValue(pet, sku);
					break;

				case "m_pilot":
					field.SetValue(pet, root.GetComponent<AirPilot>());
					break;

				case "m_machine":
					field.SetValue(pet, root.GetComponent<MachineAir>());
					break;

				case "m_viewControl":
					field.SetValue(pet, root.GetComponent<ViewControl>());
					break;

				case "m_otherSpawnables":
					ISpawnable[] spawnables = new ISpawnable[4];
					spawnables[0] = root.GetComponent<AirPilot>();
					spawnables[1] = root.GetComponent<MachineAir>();
					spawnables[2] = root.GetComponent<ViewControl>();
					spawnables[3] = root.GetComponent<MachineEatBehaviour>();
					field.SetValue(pet, spawnables);
					break;
			}
		}

        // Update viewControl script fields via reflection
		ViewControl viewControl = root.GetComponent<ViewControl>();
		foreach (FieldInfo field in viewControl.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
		{
			switch (field.Name)
			{
				case "m_animator":
					field.SetValue(viewControl, view.GetComponent<Animator>());
					break;

				case "m_renderers":
					SkinnedMeshRenderer[] renderers = view.GetComponentsInChildren<SkinnedMeshRenderer>();
					field.SetValue(viewControl, renderers);
					break;

				case "m_transform":
					field.SetValue(viewControl, root.transform);
					break;

				case "m_view":
					field.SetValue(viewControl, view.transform);
					break;
			}
		}

		// Deactivate gameObject
		root.SetActive(false);

		// Export prefab
		Export(root, rootName, assetBundleGameplay, "Gameplay");

		// Clean-up
		DestroyImmediate(root);
	}

	GameObject FindHipBone(ref GameObject view)
	{
		GameObject rootNode = view.transform.FindObjectRecursive("Pet_Hip");
		if (rootNode == null)
			rootNode = view.transform.FindObjectRecursive("root_JT"); // Fallback

		return rootNode;
	}

    string GetRelativePath(string path)
    {
		return path.Substring(path.IndexOf("Assets/"));
	}

	void SetAnimationController(ref GameObject view, RuntimeAnimatorController controller)
	{
		Animator animator = view.GetComponent<Animator>();
		if (animator != null)
		{
			animator.runtimeAnimatorController = controller;
		}
	}

	void SetMaterial(ref GameObject view)
	{
		if (babyDragonMaterial != null)
		{
			SkinnedMeshRenderer[] skinnedMeshRenderer = view.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < skinnedMeshRenderer.Length; i++)
			{
				skinnedMeshRenderer[i].material = babyDragonMaterial;
			}
		}
	}

	void Export(GameObject root, string defaultName, string assetBundle = "", string saveDialogTitle = "")
	{
		string path = EditorUtility.SaveFilePanel("Baby Dragon " + saveDialogTitle, PET_BASE_PATH, defaultName, "prefab");
		if (!string.IsNullOrEmpty(path))
		{
			GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root, GetRelativePath(path), InteractionMode.UserAction);
			if (!string.IsNullOrEmpty(assetBundle))
			{
				string assetPath = AssetDatabase.GetAssetPath(prefab);
				AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(assetBundle, "");
			}

			AssetDatabase.Refresh();
			Debug.Log("Created prefab: " + GetRelativePath(path));
		}
	}
}
