﻿// BabyDragonWizard.cs
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
	const string ANIM_CONTROLLER_MAIN_MENU_PATH = "Assets/Art/3D/Gameplay/Pets/Assets/Shared/PetAnimationControllerBlendMenu.controller";

    // Clone pet behaviour relationship by baby dragon sku
	readonly Dictionary<string, string> clonePetBehaviourPerSku = new Dictionary<string, string>()
	{
		{ "baby_classic", "PF_PetPhoenix_33" },
		{ "baby_crocodile", "PF_PetGelato_66" },
		{ "baby_croc", "PF_PetGelato_66" },
		{ "baby_dark", "PF_PetNeutrin_38" },
		{ "baby_titan", "PF_PetHorseman_70" },
		{ "baby_jawfrey", "PF_PetSanta_58" },
		{ "baby_dino", "PF_PetAlien_59" },
		{ "baby_tony", "PF_PetXmasElf_60" },
		{ "baby_dante", "PF_PetGrillmonger_64" },
		{ "baby_alien", "PF_PetFireball_36" },
		{ "baby_hedgehog", "PF_PetUnicorn_67" },
		{ "baby_nibbler", "PF_PetHorseman_70" },
		{ "baby_reptile", "PF_PetCupido_74" },
        { "baby_snake", "PF_PetCupido_74" },
		{ "baby_bug", "PF_PetXmasElf_60" },
		{ "baby_chinese", "PF_PetUnicorn_67" },
		{ "baby_devil", "PF_PetGrillmonger_64" },
		{ "baby_balrog", "PF_PetBubby_65" },
		{ "baby_goldheist", "PF_PetShu_75" },
		{ "baby_skeleton", "PF_PetFaune_63" },
		{ "baby_skully", "PF_PetFaune_63" },
		{ "baby_light", "PF_PetCupido_74" },
		{ "baby_mecha", "PF_PetChinesePig_73" },
		{ "baby_icebreaker", "PF_PetFreeze_74" },
		{ "baby_ice", "PF_PetFreeze_74" }
	};

    // Required
    Object babyDragonFBX;
    Object lastBabyDragonFBX;
	Editor gameObjectEditor;
	string sku;
	float fbxScale = 2.5f;

	// Optional
	Material babyDragonMaterial;
	Material lastBabyDragonMaterial;

	// Main menu
	RuntimeAnimatorController runtimeAnimatorControllerMenu;
	static string[] assetBundleArray;
	static int assetBundleMainMenuIndex = 0;

	// Gameplay
	static string[] popupPetCloneArray;
	static string[] popupPetClonePathArray;
	int popupPetCloneIndex = 0;
	bool addEatBehaviour = true;
	RuntimeAnimatorController runtimeAnimatorControllerGameplay;
	static int assetBundleGameplayIndex = 0;
	StateMachine aiBrain;

	// GUI
	Texture2D previewBackgroundTexture = null;
	GUIStyle previewStyle = new GUIStyle();

	// Menu
	[MenuItem("Hungry Dragon/Tools/Creation/Baby Dragon Wizard...", false, -150)]
	static void Init()
	{
		// Prepare window docked next to Inspector tab
		System.Type inspectorType = System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
		System.Type[] desiredDockNextTo = new System.Type[] { inspectorType };
		EditorWindow window = GetWindow<BabyDragonWizard>(desiredDockNextTo);
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Common/Icons/icon_btn_pets.png");
		window.titleContent = new GUIContent(" Baby Dragon", icon);

		// Create GUI popups
		CreateWindowPopups();

		// Show window
		window.Show();
	}

    static void CreateWindowPopups()
    {
		// Create pets popup list
		string[] directory = Directory.GetDirectories(PET_BASE_PATH);
		List<string> popupPetClone = new List<string>();
		List<string> popupPetClonePath = new List<string>();
		for (int i = 0; i < directory.Length; i++)
		{
			string[] guid = AssetDatabase.FindAssets("PF_Pet", new[] { directory[i] });
			for (int x = 0; x < guid.Length; x++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid[x]);
				string petName = Path.GetFileNameWithoutExtension(path);
				if (!petName.Contains("Menu"))
				{
					popupPetClone.Add(petName);
					popupPetClonePath.Add(path);
				}
			}
		}

		popupPetCloneArray = popupPetClone.ToArray();
		popupPetClonePathArray = popupPetClonePath.ToArray();

		// Create asset bundles popup list
		string[] assetBundleNone = new string[] { "None" };
		string[] assetBundles = AssetDatabase.GetAllAssetBundleNames();
		assetBundleArray = new string[assetBundleNone.Length + assetBundles.Length];
		assetBundleNone.CopyTo(assetBundleArray, 0);
		assetBundles.CopyTo(assetBundleArray, assetBundleNone.Length);

        // Set default asset bundle if not set
		if (assetBundleMainMenuIndex == 0 && assetBundleGameplayIndex == 0)
		{
			string defaultAssetBundle = "pets_local";
			int defaultAssetBundleIdx = 0;
			for (int i = 0; i < assetBundles.Length; i++)
			{
				if (assetBundles[i] == defaultAssetBundle)
				{
					defaultAssetBundleIdx = i + 1;
					break;
				}
			}

			assetBundleMainMenuIndex = defaultAssetBundleIdx;
			assetBundleGameplayIndex = defaultAssetBundleIdx;
		}
	}

    // GUI 
    void OnGUI()
    {
        // Editor checks
        if (EditorApplication.isCompiling)
        {
			EditorGUILayout.HelpBox("Cannot be used while the editor is compiling scripts", MessageType.Warning, true);
			return;
		}

        if (EditorApplication.isUpdating)
        {
			EditorGUILayout.HelpBox("Cannot be used while refreshing the AssetDatabase", MessageType.Warning, true);
			return;
		}

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
			EditorGUILayout.HelpBox("Cannot be used in play mode", MessageType.Warning, true);
			return;
        }

        // If needed, re-create GUI popups after recompiling scripts
		if (assetBundleArray == null || popupPetCloneArray == null)
		{
			CreateWindowPopups();
		}

		// Required
		EditorGUILayout.HelpBox("This tool automatically creates 2 prefabs for Baby Dragons.\nIt will create the prefabs for Main Menu and Gameplay.", MessageType.Info, true);
		EditorGUILayout.LabelField("Required", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("View FBX:");
		babyDragonFBX = EditorGUILayout.ObjectField(babyDragonFBX, typeof(UnityEngine.Object), true);
		EditorGUILayout.EndHorizontal();
		sku = EditorGUILayout.TextField("Baby dragon SKU:", sku);

		// Optional
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
		fbxScale = EditorGUILayout.FloatField("FBX scale:", fbxScale);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Material:");
		babyDragonMaterial = (Material) EditorGUILayout.ObjectField(babyDragonMaterial, typeof(Material), true);
		EditorGUILayout.EndHorizontal();

		// Main menu settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Main menu prefab settings", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Animation Controller:");
		runtimeAnimatorControllerMenu = (RuntimeAnimatorController)EditorGUILayout.ObjectField(runtimeAnimatorControllerMenu, typeof(RuntimeAnimatorController), true);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		assetBundleMainMenuIndex = EditorGUILayout.Popup("Asset bundle:", assetBundleMainMenuIndex, assetBundleArray);
		EditorGUILayout.EndHorizontal();

        // Gameplay settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Gameplay prefab settings", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		popupPetCloneIndex = EditorGUILayout.Popup("Clone pet behaviour:", popupPetCloneIndex, popupPetCloneArray);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Override AI Brain:");
		aiBrain = (StateMachine)EditorGUILayout.ObjectField(aiBrain, typeof(StateMachine), true);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		addEatBehaviour = EditorGUILayout.Toggle("Add eat behaviour", addEatBehaviour);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Animation Controller:");
		runtimeAnimatorControllerGameplay = (RuntimeAnimatorController)EditorGUILayout.ObjectField(runtimeAnimatorControllerGameplay, typeof(RuntimeAnimatorController), true);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		assetBundleGameplayIndex = EditorGUILayout.Popup("Asset bundle:", assetBundleGameplayIndex, assetBundleArray);
		EditorGUILayout.EndHorizontal();

		// Create prefabs button
		EditorGUI.BeginDisabledGroup(babyDragonFBX == null || string.IsNullOrEmpty(sku));
		if (GUILayout.Button("Create prefabs...", GUILayout.Height(40)))
		{
			CreatePrefabs();
		}
		EditorGUI.EndDisabledGroup();

        // Preview FBX
		if (babyDragonFBX != null)
		{
			if (gameObjectEditor == null || lastBabyDragonFBX != babyDragonFBX)
			{
				AssignSKU();
				AssignMaterial();
				AssignAnimationControllers();
				AssignClonePetBehaviour();

				CreatePreview();
			}
            else if (lastBabyDragonMaterial != babyDragonMaterial)
            {
				CreatePreview();
			}

			if (previewBackgroundTexture == null)
			{
				CreatePreviewBackgroundTexture();
				previewStyle.normal.background = previewBackgroundTexture;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
			gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), previewStyle);

			lastBabyDragonFBX = babyDragonFBX;
			lastBabyDragonMaterial = babyDragonMaterial;
		}
        else
        {
			gameObjectEditor = null;
        }
    }

    // Create main menu and gameplay prefabs
    void CreatePrefabs()
	{ 
		if (fbxScale > 0f)
		{
			EditorUtility.DisplayProgressBar("Baby Dragon", "Setting FBX scale...", 0.25f);
			SetFBXScale();
		}

		EditorUtility.DisplayProgressBar("Baby Dragon", "Creating main menu prefab...", 0.5f);
		CreateMenuPrefab();

		EditorUtility.DisplayProgressBar("Baby Dragon", "Creating gameplay prefab...", 1.0f);
		CreateGameplayPrefab();

		EditorUtility.ClearProgressBar();

		if (!IsAvatarMaskCreated())
		{
			if (EditorUtility.DisplayDialog("Avatar masks", "Do you want to create the avatar masks for the animation controller?", "Yes", "No"))
			{
				CreateAvatarMasks();
			}
		}
    }

    void CreateMenuPrefab()
    {
        // View
        GameObject view = (GameObject) Instantiate(babyDragonFBX);
		view.name = "view";

        // Check hip bone
		GameObject rootNode = FindHipBone(ref view);
        if (rootNode == null)
        {
			Debug.LogError("Root bone on FBX not found");
			return;
        }

		view.AddComponent<DragonAnimationEventsMenu>();

		// Create root object
		string rootName = "PF_Baby" + GetSkuSuffix() + "Menu";
		GameObject root = new GameObject(rootName) { tag = "Pet" };

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

        // Create main menu animation controller if needed
		if (runtimeAnimatorControllerMenu == null)
		{
			if (EditorUtility.DisplayDialog("Animation controller - Main menu", "Do you want to create the main menu animation controller?", "Yes", "No"))
			{
				CreateMainMenuAnimationController();
				AssignAnimationControllers();
			}
		}

		// Set animation controller
		if (runtimeAnimatorControllerMenu != null)
			SetAnimationController(ref view, runtimeAnimatorControllerMenu);

		// Export prefab
		Export(root, rootName, assetBundleArray[assetBundleMainMenuIndex], "Main Menu");

        // Clean-up
		DestroyImmediate(root);
	}

    void CreateGameplayPrefab()
    {
		// View
		GameObject view = (GameObject)Instantiate(babyDragonFBX);
		view.name = "view";

		// Check hip bone
		GameObject rootNode = FindHipBone(ref view);
		if (rootNode == null)
		{
			Debug.LogError("Root bone on FBX not found");
			return;
		}

		view.AddComponent<DragonAnimationEvents>();

		// Create root object
		string rootName = "PF_Baby" + GetSkuSuffix();
		GameObject root = new GameObject(rootName) { tag = "Pet" };
		string petClonePath = popupPetClonePathArray[popupPetCloneIndex];
		GameObject basePetModel = (GameObject)AssetDatabase.LoadAssetAtPath(petClonePath, typeof(GameObject));

        // Set layer
		root.layer = basePetModel.layer;

		// Attach view as child
		view.transform.SetParentAndReset(root.transform);
		view.transform.localPosition = new Vector3(0.0f, -1.239f, -0.64f);

		// Set material
		SetMaterial(ref view);

		// Create gameplay animation controller if needed
        if (runtimeAnimatorControllerGameplay == null)
        {
			if (EditorUtility.DisplayDialog("Animation controller - Gameplay", "Do you want to create the gameplay animation controller?", "Yes", "No"))
			{
				CreateGameplayAnimationController(ref basePetModel);
				AssignAnimationControllers();
			}
		}

        // Set animation controller
		if (runtimeAnimatorControllerGameplay != null)
			SetAnimationController(ref view, runtimeAnimatorControllerGameplay);			

		// First copy components from original prefab to the new prefab
		Component[] components = basePetModel.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            // Ignore SkinnedMeshCombiner component. Baby Dragons only have a single SkinnedMesh
            if (components[i].GetType() == typeof(SkinnedMeshCombiner))
				continue;

			UnityEditorInternal.ComponentUtility.CopyComponent(components[i]);
			UnityEditorInternal.ComponentUtility.PasteComponentAsNew(root);
		}

		// Add eat behaviour if needed
		if (addEatBehaviour && root.GetComponent<MachineEatBehaviour>() == null)
		{
			int eatPetIndex = 0;
			// Find a pet with MachineEatBehaviour script to clone their script values
			for (int i = 0; i < popupPetCloneArray.Length; i++)
			{
				if (popupPetCloneArray[i] == "PF_PetDactylus_0")
				{
					eatPetIndex = i;
					break;
				}
			}

			string eatPetClonePath = popupPetClonePathArray[eatPetIndex];
			GameObject eatPetModel = (GameObject)AssetDatabase.LoadAssetAtPath(eatPetClonePath, typeof(GameObject));
			MachineEatBehaviour machineEatBehaviour = eatPetModel.GetComponent<MachineEatBehaviour>();

			UnityEditorInternal.ComponentUtility.CopyComponent(machineEatBehaviour);
			UnityEditorInternal.ComponentUtility.PasteComponentAsNew(root);
		}

        // Override AI brain if needed
        if (aiBrain != null)
        {
			AirPilot airPilot = root.GetComponent<AirPilot>();
            if (airPilot != null)
            {
			    foreach (FieldInfo field in airPilot.GetType().BaseType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
				{ 
					switch (field.Name)
					{
						case "m_brainResource":
							field.SetValue(airPilot, aiBrain);
							break;
					}
				}
            }
        }

		// At this point, the references on the new prefab are still pointing to the old prefab.
		// Update Pet script fields via reflection
		Pet pet = root.GetComponent<Pet>();
		if (pet != null)
		{
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
		}

        // Update viewControl script fields via reflection
		ViewControl viewControl = root.GetComponent<ViewControl>();
		if (viewControl != null)
		{
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
		}

		// Update AudioControllerMixer script fields via reflection
		AudioControllerMixerSetup audioControllerMixerSetup = root.GetComponent<AudioControllerMixerSetup>();
		AudioController audioController = root.GetComponent<AudioController>();
        if (audioControllerMixerSetup != null && audioController != null)
        {
			foreach (FieldInfo field in audioControllerMixerSetup.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				switch (field.Name)
				{
					case "m_controller":
						field.SetValue(audioControllerMixerSetup, audioController);
						break;
				}
			}
		}

		// Add PreyAnimationEvents if needed
		Transform basePetView = basePetModel.transform.FindTransformRecursive("view");
		if (basePetView != null)
		{
			PreyAnimationEvents preyAnimationEvents = basePetView.GetComponent<PreyAnimationEvents>();
            if (preyAnimationEvents != null)
            {
				view.AddComponent<PreyAnimationEvents>();
            }
		}

		// Correct SphereCollider position based on child renderers
		SphereCollider sphereCollider = root.GetComponent<SphereCollider>();
		if (sphereCollider != null)
		{
			CorrectSphereCollider(ref sphereCollider, view.transform);
		}

		// Deactivate gameObject
		root.SetActive(false);

		// Export prefab
		Export(root, rootName, assetBundleArray[assetBundleGameplayIndex], "Gameplay");

		// Clean-up
		DestroyImmediate(root);
	}

    void CloneAnimatorController(string animControllerName, string dest)
    {
		string[] guids = AssetDatabase.FindAssets(animControllerName);
		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (Path.GetFileNameWithoutExtension(path) == animControllerName)
			{
				File.Copy(path, dest);
				break;
			}
		}
	}

    void CreateGameplayAnimationController(ref GameObject petModel)
    {
		Animator petAnimator = petModel.GetComponentInChildren<Animator>();
		if (petAnimator == null)
			return;

		RuntimeAnimatorController petAnimController = petAnimator.runtimeAnimatorController;
		AnimatorOverrideController animatorOverrideController = petAnimController as AnimatorOverrideController;
        if (animatorOverrideController == null)
        {
			// Clone animator controller
			string dest = Path.Combine(GetFBXPath(), "AC_Baby" + GetSkuSuffix() + ".controller");
			CloneAnimatorController(petAnimController.name, dest);
        }
        else
        {
			// Clone animator controller
			RuntimeAnimatorController baseAnimationController = animatorOverrideController.runtimeAnimatorController;
			string baseControllerPath = Path.Combine(GetFBXPath(), "AC_Baby" + GetSkuSuffix() + "Controller.controller");
			CloneAnimatorController(baseAnimationController.name, baseControllerPath);

			// Clone override controller
			string overrideControllerPath = Path.Combine(GetFBXPath(), "AC_Baby" + GetSkuSuffix() + ".overrideController");
			CloneAnimatorController(petAnimController.name, overrideControllerPath);
			
            // Refresh changes
			AssetDatabase.Refresh();

			// Assign animator controller to override controller
			AnimatorOverrideController newAnimOverrideController = (AnimatorOverrideController) AssetDatabase.LoadAssetAtPath(overrideControllerPath, typeof(AnimatorOverrideController));
			RuntimeAnimatorController newAnimController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath(baseControllerPath, typeof(RuntimeAnimatorController));
			newAnimOverrideController.runtimeAnimatorController = newAnimController;
        }

        // Refresh changes
		AssetDatabase.Refresh();
    }

    void CreateMainMenuAnimationController()
    {
		// Create new animator override controller
		AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
		RuntimeAnimatorController animController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath(ANIM_CONTROLLER_MAIN_MENU_PATH, typeof(RuntimeAnimatorController));

        // Assign main menu reference animator controller to new override controller
        animatorOverrideController.runtimeAnimatorController = animController;

        // Create asset
        string path = Path.Combine(GetFBXPath(), "AC_Baby" + GetSkuSuffix() + "Menu.overrideController");
		AssetDatabase.CreateAsset(animatorOverrideController, path);

        // Refresh changes
		AssetDatabase.Refresh();
	}

    string GetFBXPath()
    {
		string fbxPath = AssetDatabase.GetAssetPath(babyDragonFBX);
		DirectoryInfo dir = Directory.GetParent(fbxPath);

		// Navigate path one-time backwards
		dir = Directory.GetParent(dir.ToString());

		return dir.ToString();
	}

    void CreateAvatarMasks()
    {
		GameObject babyFBX = (GameObject) babyDragonFBX;

		// Avatar mask mouth
		AvatarMask avatarMaskMouth = new AvatarMask();
		avatarMaskMouth.AddTransformPath(babyFBX.transform);

        // Avatar mask no mouth
		AvatarMask avatarMaskNoMouth = new AvatarMask();
		avatarMaskNoMouth.AddTransformPath(babyFBX.transform);

		// New avatar mask paths
		string path = GetFBXPath();
		string avatarPathMouth = Path.Combine(path, "Baby" + GetSkuSuffix() + "_Mouth.mask");
		string avatarPathNoMouth = Path.Combine(path, "Baby" + GetSkuSuffix() + "_NoMouth.mask");

        // Create avatar masks
		AssetDatabase.CreateAsset(avatarMaskMouth, avatarPathMouth);
		AssetDatabase.CreateAsset(avatarMaskNoMouth, avatarPathNoMouth);

        // Refresh
		AssetDatabase.Refresh();
	}

	bool IsAvatarMaskCreated()
	{
		string mouthPath = Path.Combine(GetFBXPath(), "Baby" + GetSkuSuffix() + "_Mouth.mask");
		string noMouthPath = Path.Combine(GetFBXPath(), "Baby" + GetSkuSuffix() + "_NoMouth.mask");

		return File.Exists(mouthPath) && File.Exists(noMouthPath);
	}

	void CreatePreview()
    {
		GameObject preview = (GameObject)babyDragonFBX;
		Renderer[] renderers = preview.transform.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material = babyDragonMaterial;
		}

		gameObjectEditor = Editor.CreateEditor(preview);
	}

	void AssignSKU()
	{
		string[] fbxName = babyDragonFBX.name.ToString().Split('_');
		if (fbxName.Length >= 1)
		{
			sku = "baby_" + fbxName[1].ToLower();
		}
	}

	void AssignMaterial()
	{
		// Try to find material path
		string fbxPath = AssetDatabase.GetAssetPath(babyDragonFBX);
		DirectoryInfo dir = Directory.GetParent(fbxPath);
		string materialPath = GetRelativePath(dir.Parent.ToString()) + Path.DirectorySeparatorChar + "Materials";

		if (Directory.Exists(materialPath))
		{
			// Find assets of material type on materialPath
			string[] guid = AssetDatabase.FindAssets("t:Material", new[] { materialPath });
			for (int i = 0; i < guid.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid[i]);
				Material mat = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
				if (mat != null)
				{
					// Assign first material found on materialPath folder
					babyDragonMaterial = mat;
					break;
				}
			}
		}
	}

	void AssignAnimationControllers()
	{
		// Find animation controllers path
		string fbxPath = AssetDatabase.GetAssetPath(babyDragonFBX);
		DirectoryInfo dir = Directory.GetParent(fbxPath);
		string animationControllerPath = GetRelativePath(dir.Parent.ToString());

		if (Directory.Exists(animationControllerPath))
		{
			// Find animation controllers
			string[] guid = AssetDatabase.FindAssets("AC_", new[] { animationControllerPath });
			for (int i = 0; i < guid.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid[i]);
				RuntimeAnimatorController animController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath(path, typeof(RuntimeAnimatorController));
				if (animController != null)
				{
                    if (animController.name.EndsWith("Menu"))
                    {
						runtimeAnimatorControllerMenu = animController;
                    }
                    else if (!animController.name.EndsWith("Controller"))
					{
						runtimeAnimatorControllerGameplay = animController;
                    }
				}
			}
		}
	}

	void AssignClonePetBehaviour()
	{
		if (clonePetBehaviourPerSku.TryGetValue(sku, out string petName))
        {
            for (int i = 0; i < popupPetCloneArray.Length; i++)
            {
                if (popupPetCloneArray[i] == petName)
                {
					popupPetCloneIndex = i;
					break;
                }
            }
        }
	}

    void CreatePreviewBackgroundTexture()
	{
		Color backgroundColor = new Color(0.1568f, 0.1568f, 0.1568f);
		previewBackgroundTexture = MakeTexture(backgroundColor);
	}

	Texture2D MakeTexture(Color color, int width = 1, int height = 1)
	{
		Color[] pixels = new Color[width * height];

		for (int i = 0; i < pixels.Length; i++)
			pixels[i] = color;

		Texture2D result = new Texture2D(width, height, TextureFormat.RGB24, false);
		result.SetPixels(pixels);
		result.Apply();

		return result;
	}

	void SetFBXScale()
	{
		string fbxPath = AssetDatabase.GetAssetPath(babyDragonFBX);
		string path = Path.GetDirectoryName(fbxPath);

		foreach (string file in Directory.EnumerateFiles(path, "*.fbx", SearchOption.AllDirectories))
		{
			ModelImporter fbxModel = AssetImporter.GetAtPath(file) as ModelImporter;
			if (fbxModel != null && fbxModel.globalScale != fbxScale)
			{
				fbxModel.globalScale = fbxScale;
				fbxModel.SaveAndReimport();
			}
		}
	}

	string GetSkuSuffix()
	{
		string suffix = sku;
		string[] skuSplit = sku.Split('_');
		if (skuSplit.Length > 1)
		{
			suffix = skuSplit[1];
			suffix = char.ToUpper(suffix[0]) + suffix.Substring(1);
		}

		return suffix;
	}

	void CorrectSphereCollider(ref SphereCollider sphereCollider, Transform view)
    {
		bool hasBounds = false;
		Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

		for (int i = 0; i < view.childCount; ++i)
		{
			Renderer childRenderer = view.GetChild(i).GetComponent<Renderer>();
			if (childRenderer != null)
			{
				if (hasBounds)
				{
					bounds.Encapsulate(childRenderer.bounds);
				}
				else
				{
					bounds = childRenderer.bounds;
					hasBounds = true;
				}
			}
		}

		sphereCollider.center = bounds.center - sphereCollider.transform.position;
		sphereCollider.radius = 1.0f;
	}

	GameObject FindHipBone(ref GameObject view)
	{
		GameObject rootNode = view.transform.FindObjectRecursive("Pet_Hip");
		if (rootNode == null)
			rootNode = view.transform.FindObjectRecursive("root_JT"); // Fallback

		if (rootNode == null)
			rootNode = view.transform.FindObjectRecursive("Root"); // Fallback

		return rootNode;
	}

    string GetRelativePath(string path)
    {
		int startIndex = path.IndexOf("Assets" + Path.DirectorySeparatorChar);
        if (startIndex >= 0)
		    return path.Substring(startIndex);

		return string.Empty;
	}

	void SetAnimationController(ref GameObject view, RuntimeAnimatorController controller)
	{
		Animator animator = view.GetComponent<Animator>();
		if (animator != null)
			animator.runtimeAnimatorController = controller;
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

	void Export(GameObject root, string defaultName, string assetBundle = "None", string saveDialogTitle = "")
	{
		string path = EditorUtility.SaveFilePanel("Baby Dragon " + saveDialogTitle, PET_BASE_PATH, defaultName, "prefab");
		if (!string.IsNullOrEmpty(path))
		{
			GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(root, GetRelativePath(path), InteractionMode.UserAction);
			if (assetBundle == "None")
				assetBundle = "";
			
			string assetPath = AssetDatabase.GetAssetPath(prefab);
			AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(assetBundle, "");
			
			AssetDatabase.Refresh();
			Debug.Log("Created prefab: " + GetRelativePath(path));
		}
	}
}
