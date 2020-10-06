using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DragonWizardCreateDragonModule : IDragonWizard
{
	const string DRAGON_MODEL_PATH = "Assets/Art/3D/Gameplay/Dragons/Prefabs/Game/PF_DragonClassic.prefab";

	// Required
	UnityEngine.Object dragonFBX;
	UnityEngine.Object lastDragonFBX;
	Editor gameObjectEditor;
	string sku;
	float fbxScale = 1.0f;

	// Optional
	Material dragonMaterial;
	Material lastDragonMaterial;

	// GUI
	Texture2D previewBackgroundTexture = null;
	GUIStyle previewStyle = new GUIStyle();

	public string GetToolbarTitle()
    {
        return "Create dragon";
    }

    public void OnGUI()
    {
		EditorGUILayout.HelpBox("Prepare the required XML files when creating a new dragon", MessageType.Info, true);
		if (GUILayout.Button("Setup new dragon XML tables...", GUILayout.Height(40)))
		{
			CreateXMLTables();
		}

		EditorGUILayout.Space();
		EditorGUILayout.HelpBox("Creates 4 prefabs for implementing new dragons.\nIt will create the prefabs for Main Menu, Results, Corpse and Gameplay.", MessageType.Info, true);
		EditorGUILayout.LabelField("Required", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("View FBX:");
		dragonFBX = EditorGUILayout.ObjectField(dragonFBX, typeof(UnityEngine.Object), true);
		EditorGUILayout.EndHorizontal();
		sku = EditorGUILayout.TextField("Dragon SKU:", sku);

		// Optional
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Optional", EditorStyles.boldLabel);
		fbxScale = EditorGUILayout.FloatField("FBX scale:", fbxScale);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Material:");
		dragonMaterial = (Material)EditorGUILayout.ObjectField(dragonMaterial, typeof(Material), true);
		EditorGUILayout.EndHorizontal();

		// Create prefabs button
		EditorGUI.BeginDisabledGroup(dragonFBX == null || string.IsNullOrEmpty(sku));
		if (GUILayout.Button("Create prefabs...", GUILayout.Height(40)))
		{
			CreatePrefabs();
		}
		EditorGUI.EndDisabledGroup();

		// Preview FBX
		if (dragonFBX != null)
		{
			if (gameObjectEditor == null || lastDragonFBX != dragonFBX)
			{
				//AssignSKU();
				//AssignMaterial();
				//AssignAnimationControllers();
				//AssignClonePetBehaviour();

				CreatePreview();
			}
			else if (lastDragonMaterial != dragonMaterial)
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

			lastDragonFBX = dragonFBX;
			lastDragonMaterial = dragonMaterial;
		}
		else
		{
			gameObjectEditor = null;
		}
	}

	void CreateXMLTables()
	{
        /*
		DragonWizardXML window = GetWindow<DragonWizardXML>();
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Common/Icons/icon_tab_missions.png");
		window.titleContent = new GUIContent(" New dragon XML setup", icon);

		// Show window
		window.Init(sku);
		window.Show();
        */
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

	void CreatePreview()
	{
		GameObject preview = (GameObject)dragonFBX;
		Renderer[] renderers = preview.transform.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material = dragonMaterial;
		}

		gameObjectEditor = Editor.CreateEditor(preview);
	}

	void CreatePrefabs()
	{

	}
}
