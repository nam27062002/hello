using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

public class DragonMenuOrder : EditorWindow
{
    const string DRAGON_SELECTION_PREFAB = "Assets/Art/3D/Menu/DRAGON_SELECTION_Scene.prefab";
    const string BASE_DEFINITIONS_PATH = "Assets/Resources/Rules/";
    const string DRAGON_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "dragonDefinitions.xml";
    const string SPECIAL_DRAGON_DEFINITIONS_PATH = BASE_DEFINITIONS_PATH + "specialDragonDefinitions.xml";
    const string DRAGON_ICONS_PATH = "Assets/Art/UI/Metagame/Dragons/Disguises/";

    ReorderableList list;
    Vector2 scroll;
    MenuDragonSlot[] slots;
    GameObject prefab;
    int normalDragonsCount;
    readonly Dictionary<string, Texture2D> dragonIconCache = new Dictionary<string, Texture2D>();

    [Serializable]
    public class MenuDragons
    {
        public List<string> sku = new List<string>();
        public List<bool> isSpecialDragon = new List<bool>();
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
        dragonIconCache.Clear();

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
            dragons.isSpecialDragon.Add(i >= normalDragonsCount);
        }
    }

    void OnEnable()
    {
        // Prepare dragons sku
        normalDragonsCount = GetNormalDragonsCount();
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
        list.drawElementCallback += OnDrawElementCallback;
        list.onReorderCallbackWithDetails += OnReorderCallbackWithDetails;
    }

    void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
    {
        if (oldIndex >= normalDragonsCount && newIndex < normalDragonsCount)
        {
            LoadMenuDragons();
            EditorUtility.DisplayDialog("Error", "Cannot move a dragon from special to normal.\nAborting operation: reverting all changes", "Close");
        }
        else if (oldIndex < normalDragonsCount && newIndex >= normalDragonsCount)
        {
            LoadMenuDragons();
            EditorUtility.DisplayDialog("Error", "Cannot move a dragon from normal to special.\nAborting operation: reverting all changes", "Close");
        }
    }

    void OnDisable()
    {
        // Unsubscribe to reorderable list callbacks
        list.drawHeaderCallback -= OnDrawHeaderCallback;
        list.drawElementCallback += OnDrawElementCallback;
        list.onReorderCallbackWithDetails -= OnReorderCallbackWithDetails;
    }

    void OnDrawHeaderCallback(Rect rect)
    {
        // Reorderable list title
        EditorGUI.LabelField(rect, "Main menu dragons by order");
    }

    void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label);
        string sku = dragons.sku[index];
        string labelText = " " + (index + 1) + ". " + sku;
        
        // Change color and add special label text on special dragons
        if (index >= normalDragonsCount)
        {
            style.normal.textColor = Color.green;
            labelText += " [special]";
        }

        // Add dragon icon
        if (!dragonIconCache.TryGetValue(sku, out Texture2D icon))
        {
            icon = (Texture2D)AssetDatabase.LoadAssetAtPath(System.IO.Path.Combine(DRAGON_ICONS_PATH, dragons.sku[index], "icon_" + sku.Split('_')[1] + "_0.png"), typeof(Texture2D));
            dragonIconCache.Add(sku, icon);
        }

        GUI.Box(rect, new GUIContent(labelText, icon), style);
    }

    void OnGUI()
    {
        // Help box
        EditorGUILayout.HelpBox("Drag and drop the dragons below to change their order in the main menu.\nPlease do not mix normal and special dragons.", MessageType.Info, true);

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

    int GetNormalDragonsCount()
    {
        // Load dragon definitions XML file
        XmlDocument doc = new XmlDocument();
        doc.Load(DRAGON_DEFINITIONS_PATH);

        // Select XML node
        XmlNodeList nodeList = doc.SelectNodes("Definitions");
        return nodeList.Item(0).ChildNodes.Count;
    }

    void SaveChanges()
    {
        // Save dragon sku order to prefab
        for (int i = 0; i < dragons.sku.Count; i++)
        {
            MenuDragonLoader menuDragonLoader = slots[i].transform.GetChild(0).GetComponent<MenuDragonLoader>();
            menuDragonLoader.dragonSku = dragons.sku[i];

            // Update definitions order id
            UpdateXMLOrderId(dragons.sku[i], i, dragons.isSpecialDragon[i]);
        }

        // The current implementation of reading XMLs in content depends on the order in the XML file
        // We need to sort the node elements by order
        SortXML(DRAGON_DEFINITIONS_PATH);
        SortXML(SPECIAL_DRAGON_DEFINITIONS_PATH);

        // Save assets
        EditorUtility.SetDirty(prefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Save completed", "Prefab and definition files were updated successfully", "Close");
    }

    void SortXML(string xmlPath)
    {
        // Load dragon definitions XML file
        XDocument xDoc = XDocument.Load(xmlPath);

        // Sort by order attribute
        var xml = from ele in xDoc.Descendants("Definition")
                       orderby int.Parse(ele.Attribute("order").Value)
                       select ele;

        // Create XML writer settings
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t",
            OmitXmlDeclaration = true
        };

        // Save the changes to disk using the XML writer settings
        using (XmlWriter writer = XmlWriter.Create(xmlPath, settings))
        {
            XDocument doc = new XDocument(new XElement("Definitions", xml));
            doc.Save(writer);
        }
    }

    void UpdateXMLOrderId(string sku, int order, bool isSpecialDragon)
    {
        // Load dragon definitions XML file
        XmlDocument doc = new XmlDocument();
        string definitionsFile = DRAGON_DEFINITIONS_PATH;
        doc.Load(definitionsFile);

        // Check if the sku exists
        // If does not exists, check the special dragon definitions XML file
        XmlElement element = (XmlElement) doc.SelectSingleNode("/Definitions/Definition[@sku='" + sku + "']");
        if (element == null)
        {
            definitionsFile = SPECIAL_DRAGON_DEFINITIONS_PATH;
            doc.Load(definitionsFile);
            element = (XmlElement) doc.SelectSingleNode("/Definitions/Definition[@sku='" + sku + "']");
            if (element == null)
            {
                Debug.LogError("Dragon " + sku + " not found in specialDragonDefinitions");
                return;
            }
        }

        int newOrder = isSpecialDragon ? Mathf.Abs(normalDragonsCount - order) : order;
        element.SetAttribute("order", newOrder.ToString());

        // Create XML writer settings
        XmlWriterSettings settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "\t",
            OmitXmlDeclaration = true
        };

        // Save the changes to disk using the XML writer settings
        using (XmlWriter writer = XmlWriter.Create(definitionsFile, settings))
        {
            doc.Save(writer);
        }
    }
}
