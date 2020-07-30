using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

public class DragonMenuOrder : EditorWindow
{
    const string DRAGON_SELECTION_PREFAB = "Assets/Art/3D/Menu/DRAGON_SELECTION_Scene.prefab";

    ReorderableList list;
    Vector2 scroll;
    MenuDragonSlot[] slots;
    GameObject prefab;

    [Serializable]
    public class MenuDragons
    {
        public List<string> sku = new List<string>();
    }

    [SerializeField]
    MenuDragons dragons = new MenuDragons();

	// Menu
	[MenuItem("Hungry Dragon/Tools/Gameplay/Dragon Menu Order...", false, -150)]
	static void Init()
	{
		// Prepare window docked next to Inspector tab
		Type inspectorType = System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
		Type[] desiredDockNextTo = new Type[] { inspectorType };
		EditorWindow window = GetWindow<DragonMenuOrder>(desiredDockNextTo);
		Texture icon = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Art/UI/Common/Icons/icon_btn_animoji.png");
		window.titleContent = new GUIContent(" Dragon Menu Order", icon);

		// Show window
		window.Show();
	}

    void LoadMenuDragons()
    {
        dragons.sku.Clear();

        // Load prefab
        prefab = AssetDatabase.LoadAssetAtPath(DRAGON_SELECTION_PREFAB, typeof(GameObject)) as GameObject;
        if (prefab == null)
        {
            Debug.LogError("Prefab not found: " + DRAGON_SELECTION_PREFAB);
            return;
        }

        // Get main menu dragon slots
        Transform dragonsTransform = prefab.FindTransformRecursive("Dragons");
        slots = dragonsTransform.GetComponentsInChildren<MenuDragonSlot>();

        // Add dragons sku to list
        for (int i = 0; i < slots.Length; i++)
        {
            MenuDragonLoader menuDragonLoader = slots[i].transform.GetChild(0).GetComponent<MenuDragonLoader>();
            dragons.sku.Add(menuDragonLoader.dragonSku);
        }
    }

    void OnEnable()
    {
        // Prepare dragons sku
        LoadMenuDragons();

        // Prepare reorderable list
        list = new ReorderableList(
            dragons.sku,
            typeof(string),
            draggable: true,
            displayHeader: true,
            displayAddButton: false,
            displayRemoveButton: false
        );

        // Subscribe to reorderable list callbacks
        list.drawHeaderCallback += OnDrawHeaderCallback;
    }

    void OnDisable()
    {
        // Unsubscribe to reorderable list callbacks
        list.drawHeaderCallback -= OnDrawHeaderCallback;
    }

    void OnDrawHeaderCallback(Rect rect)
    {
        // Reorderable list title
        EditorGUI.LabelField(rect, "Main menu dragons by order");
    }

    void OnGUI()
    {
        // Scroll view
        scroll = EditorGUILayout.BeginScrollView(scroll);

        // Dragon sku list
        list.DoLayoutList();

        EditorGUILayout.BeginHorizontal();

        // Reset button
        if (GUILayout.Button("Reset", GUILayout.Height(40)))
            LoadMenuDragons();

        // Save changes button
        if (GUILayout.Button("Save changes", GUILayout.Height(40)))
            SaveChanges();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    void SaveChanges()
    {
        // Save dragon sku order to prefab
        for (int i = 0; i < dragons.sku.Count; i++)
        {
            MenuDragonLoader menuDragonLoader = slots[i].transform.GetChild(0).GetComponent<MenuDragonLoader>();
            menuDragonLoader.dragonSku = dragons.sku[i];
        }

        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Save completed", "Changes were applied", "Close");
    }
}
