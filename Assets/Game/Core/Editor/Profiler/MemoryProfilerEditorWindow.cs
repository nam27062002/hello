using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Object = UnityEngine.Object;

/*
public class TextureDetails : IEquatable<TextureDetails>
{
    public bool isCubeMap;
    public int memSizeKB;
    public Texture texture;
    public TextureFormat format;
    public int mipMapCount;
    public List<Object> FoundInMaterials = new List<Object>();
    public List<Object> FoundInRenderers = new List<Object>();
    public List<Object> FoundInAnimators = new List<Object>();
    public List<Object> FoundInScripts = new List<Object>();
    public List<Object> FoundInGraphics = new List<Object>();
    public bool isSky;
    public bool instance;
    public bool isgui;
    public TextureDetails()
    {

    }

    public bool Equals(TextureDetails other)
    {
        return texture != null && other.texture != null &&
            texture.GetNativeTexturePtr() == other.texture.GetNativeTexturePtr();
    }

    public override int GetHashCode()
    {
        return (int)texture.GetNativeTexturePtr();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as TextureDetails);
    }
};

public class MaterialDetails
{

    public Material material;

    public List<Renderer> FoundInRenderers = new List<Renderer>();
    public List<Graphic> FoundInGraphics = new List<Graphic>();
    public bool instance;
    public bool isgui;
    public bool isSky;

    public MaterialDetails()
    {
        instance = false;
        isgui = false;
        isSky = false;
    }
};

public class MeshDetails
{
    public long memSizeBytes;
    public float memSizeKB
    {
        get
        {
            return memSizeBytes / 1024f;
        }
    }

    private Mesh mesh;
    public Mesh Mesh
    {
        get { return mesh; }
        set
        {
            mesh = value;
            memSizeBytes = UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(mesh);
        }
    }

    public List<GameObject> FoundInGos = new List<GameObject>();
    public List<MeshFilter> FoundInMeshFilters = new List<MeshFilter>();
    public List<SkinnedMeshRenderer> FoundInSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();

    public bool instance;

    public MeshDetails()
    {
        instance = false;
    }
};

public class MissingGraphic
{
    public Transform Object;
    public string type;
    public string name;
}
*/

public class GroupDef
{
    public string Name { get; set; }

    // In bytes    
    public long Max { get; set; }

    public void Setup(string name, long max)
    {
        Name = name;       
        Max = max;
    }
}

public class GroupDefExt
{
    public GroupDef GroupDef { get; set; }
    public long Amount { get; set; }

    public void Setup(GroupDef groupDef, long amount)
    {
        GroupDef = groupDef;
        Amount = amount;
    }
}

/// <summary>
/// This class is used to represent the memory spent on a particular group (textures, animationClips and so on)
/// </summary>
public class GroupWidget
{
    #region static
    private GUIStyle smLabelStyleOk;
    private GUIStyle smLabelStyleAlert;
    private GUIStyle smLabelStyleNeutral;

    private void InitStatic()
    {
        if (smLabelStyleOk == null)
        {
            smLabelStyleOk = new GUIStyle();
            smLabelStyleOk.alignment = TextAnchor.MiddleCenter;
            smLabelStyleOk.normal.textColor = Color.green;

            smLabelStyleAlert = new GUIStyle();
            smLabelStyleAlert.alignment = TextAnchor.MiddleCenter;            
            smLabelStyleAlert.normal.textColor = Color.red;

            smLabelStyleNeutral = new GUIStyle();
            smLabelStyleNeutral.alignment = TextAnchor.MiddleCenter;
            smLabelStyleNeutral.normal.textColor = Color.white;
        }
    }
    #endregion

    public string Label { get; set; }

    /// <summary>
    /// Max in bytes
    /// </summary>
    public long Max { get; set; }

    /// <summary>
    /// Amount in bytes
    /// </summary>
    public long Amount { get; set; }
    public bool ShowOnlyAmount { get; set; }
  
    public void Setup(string label, long amount, long max, bool showOnlyAmount)
    {
        Label = label;
        Amount = amount;
        Max = max;
        ShowOnlyAmount = showOnlyAmount;
    }

    public void OnGUI(Rect area)
    {
        InitStatic();        

        GUILayout.BeginArea(area);        
        float barHeight = 2f *area.height / 3f;
        float labelHeight = area.height - barHeight;

        float amountInMB = Util.BytesToMegaBytes(Amount);
        float maxInMb = Util.BytesToMegaBytes(Max);
        if (ShowOnlyAmount)
        {            
            EditorGUI.LabelField(new Rect(0, 0, area.width, labelHeight), amountInMB.ToString("F2") + " MB", smLabelStyleNeutral);
        }
        else
        {
            GUIStyle style = (Amount <= Max) ? smLabelStyleOk : smLabelStyleAlert;
            EditorGUI.LabelField(new Rect(0, 0, area.width, labelHeight), amountInMB.ToString("F2") + "/" + maxInMb.ToString("F2") + "MB", style);
        }

        float percent = (float)Amount / (float)Max;
        EditorGUI. ProgressBar(new Rect(0, labelHeight, area.width, barHeight), percent, Label);
        GUILayout.EndArea();        
    }
}


/// <summary>
/// This class is used to represent the memory spent on a particular category (hud, level, npcs and so on). Every category holds several <c>GroupWidget</c> objects
/// </summary>
public class CategoryWidget
{   
    public string Name
    {
        get
        {
            return TotalWidget.Label;
        }

        set
        {
            TotalWidget.Label = value;
        }
    }

    private GroupWidget TotalWidget { get; set; }
    private List<GroupWidget> GroupWidgets { get; set; }

    public float Width
    {
        get
        {
            return WidthPerGroupWidget + 2 * WidthPadding;
        }
    }

    private float WidthPadding
    {
        get
        {
            return 10f;
        }
    }

    private float WidthPerGroupWidget
    {
        get
        {
            return 180f;
        }
    }

    public float Height
    {
        get
        {
            float groupWidgetHeight = HeightPerGroupWidget;

            // Height because of the GroupHeight used for the total
            float returnValue = 2*HeightSeparator + HeightHeadline;

            // We need to add the height of all group widgets
            if (GroupWidgets != null)
            {
                returnValue += GroupWidgets.Count * groupWidgetHeight + HeightSeparator * GroupWidgets.Count;                
            }

            return returnValue;
        }
    }
    
    private float HeightHeadline
    {
        get
        {
            return HeightPerGroupWidget * 1.5f;
        }
    }

    private float HeightPerGroupWidget
    {
        get
        {
            return 30f;
        }
    }

    private float HeightSeparator
    {
        get
        {
            return 5f;
        }
    }

    public void Setup(string name, List<GroupDefExt> groups, long amount, long max)
    {
        ClearGroups();
        if (groups != null)
        {
            if (GroupWidgets == null)
            {
                GroupWidgets = new List<GroupWidget>();
            }

            int count = groups.Count;
            for (int i = 0; i < count; i++)
            {               
                GroupWidget groupWidget = new GroupWidget();
                groupWidget.Setup(groups[i].GroupDef.Name, groups[i].Amount, amount, true);
                GroupWidgets.Add(groupWidget);
            }
        }

        if (TotalWidget == null)
        {
            TotalWidget = new GroupWidget();
        }

        TotalWidget.Setup(name, amount,  max, false);             
    }

    public long Max
    {
        get
        {
            return TotalWidget.Max;
        }

        set
        {
            TotalWidget.Max = value;
        }
    }           

    private void ClearGroups()
    {
        if (GroupWidgets != null)
        {
            GroupWidgets.Clear();
        }
    }       

    public void OnGUI(float x, float y)
    {        
        Rect area = new Rect(x, y, Width, Height);

        GUILayout.BeginArea(area);
        if (GroupWidgets != null)
        {            
            // The background
            GUILayout.Box("", GUILayout.Width(Width), GUILayout.Height(Height));

            float heightPerWidget = HeightPerGroupWidget;

            // The total
            float currentY = HeightSeparator;
            TotalWidget.OnGUI(new Rect(WidthPadding, HeightSeparator, WidthPerGroupWidget, HeightHeadline));            

            int count = GroupWidgets.Count;                        
            currentY += HeightHeadline + HeightSeparator;
            for (int i = 0; i < count; i++)
            {
                GroupWidgets[i].OnGUI(new Rect(WidthPadding, currentY, WidthPerGroupWidget, heightPerWidget));
                currentY += heightPerWidget + HeightSeparator;
            }
        }
        GUILayout.EndArea();
    }
}

/// <summary>
/// This class is used to represent the memory spent on a particular sample
/// </summary>
public class SampleWidget
{
    private static GUIStyle smLabelStyle;    

    private static void InitStatic()
    {
        if (smLabelStyle == null)
        {
            smLabelStyle = new GUIStyle();
            smLabelStyle.alignment = TextAnchor.MiddleCenter;
            smLabelStyle.normal.textColor = Color.white;
            smLabelStyle.fontStyle = FontStyle.Bold;
            smLabelStyle.fontSize = 30;       
        }

    }

    private string Name { get; set; }
    private List<MemorySample> Samples { get; set; }
    private List<long> SamplesMax { get; set; }    
    private List<GroupDef> Groups { get; set; }

    public List<CategoryWidget> CategoryWidgets { get; set; }

    public void Clear()
    {
        ClearSamples();
        Groups = null;
        Name = "";
        Build();
    }

    public void Setup(string name, Dictionary<string, List<string>> groups, long maxInBytes, List<MemorySample> samples, List<long> samplesMaxInBytes)
    {
        Name = name;        

        SetGroups(groups, maxInBytes);
        
        SetSamples(samples, samplesMaxInBytes, true);        
    } 
    
    private void SetGroups(Dictionary<string, List<string>> groups, long maxInBytes)
    {
        if (Groups == null)
        {
            Groups = new List<GroupDef>();
        }
        else
        {
            Groups.Clear();
        }

        GroupDef group;
        foreach (KeyValuePair<string, List<string>> pair in groups)
        {
            group = new GroupDef();
            group.Setup(pair.Key, maxInBytes);
            Groups.Add(group);
        }
    }

    private void SetSamples(List<MemorySample> samples, List<long> samplesMaxInBytes, bool build)
    {
        ClearSamples();
        Samples = samples;
        SamplesMax = samplesMaxInBytes;
        
        if (build)
        {
            Build();
        } 
    }

    private void ClearSamples()
    {
        Samples = null;
        if (CategoryWidgets != null)
        {
            CategoryWidgets.Clear();
        }
    }

    private void Build()
    {        
        if (Samples != null)
        {            
            if (CategoryWidgets == null)
            {
                CategoryWidgets = new List<CategoryWidget>();
            }

            int groupsCount = Groups.Count;
            List<GroupDefExt> groupsExt = null;
            GroupDefExt groupDefExt;
            long memPerType;
            
            CategoryWidget widget;
            int samplesCount = Samples.Count;
            for (int i = 0; i < samplesCount; i++)
            {
                widget = new CategoryWidget();               

                groupsExt = new List<GroupDefExt>();                
                for (int j = 0; j < groupsCount; j++)
                {
                    groupDefExt = new GroupDefExt();
                    memPerType = Samples[i].GetMemorySizePerTypeGroup(Groups[j].Name);
                    groupDefExt.Setup(Groups[j], memPerType);
                    groupsExt.Add(groupDefExt);                    
                }               
                
                widget.Setup(Samples[i].Name, groupsExt, Samples[i].GetTotalMemorySize(), SamplesMax[i]);
                CategoryWidgets.Add(widget);
            }
        }
    }

    /*public void SetupTest()
    {
        Name = "Test";

        long maxInBytes = (long)Util.MegaBytesToBytes(141.0f);
        Groups = new List<GroupDef>();
        GroupDef group = new GroupDef();
        group.Setup("Textures", maxInBytes);
        Groups.Add(group);

        group = new GroupDef();
        group.Setup("Meshes", maxInBytes);
        Groups.Add(group);

        group = new GroupDef();
        group.Setup("AnimationClip", maxInBytes);
        Groups.Add(group);

        if (CategoryWidgets == null)
        {
            CategoryWidgets = new List<CategoryWidget>();
        }

        CategoryWidget widget;
        int count = 1;
        for (int i = 0; i < count; i++)
        {
            widget = new CategoryWidget();
            widget.Setup("Name " + i, Groups, maxInBytes + i*10000, maxInBytes);
            CategoryWidgets.Add(widget);
        }

        Build();
    }*/

    public void OnGUI(float x, float y, float width, float height)
    {
        InitStatic();

        EditorGUI.LabelField(new Rect(x, y, width, 30f), Name + "", smLabelStyle);

        y += 30f;

        if (CategoryWidgets != null && CategoryWidgets.Count > 0)
        {
            int count = CategoryWidgets.Count;

            // Width
            float widgetWidth = CategoryWidgets[0].Width;            
            int widgetsPerRow = (int)(width / widgetWidth);
            if (widgetsPerRow > count)
            {
                widgetsPerRow = count;
            }

            float startX = x;
            float separatorX = 0f;
            float remainingWidth = width - widgetsPerRow * widgetWidth;
            if (remainingWidth > 0)
            {
                if (widgetsPerRow == 1)
                {
                    separatorX = remainingWidth / 2f;
                    startX += separatorX;
                }
                else
                {
                    separatorX = remainingWidth / 3;
                    startX += separatorX;
                    separatorX = separatorX / (widgetsPerRow - 1);
                }
            }

            // Height
            float widgetHeight = CategoryWidgets[0].Height;            
            int widgetsPerColumn = count / widgetsPerRow;
            if (count % widgetsPerRow > 0)
            {
                widgetsPerColumn++;
            }

            float remainingHeight = height - y - widgetsPerColumn * widgetHeight;
            float startY = y;
            float separatorY = 0f;
            if (remainingHeight > 0)
            {
                if (widgetsPerColumn == 1)
                {
                    separatorY = remainingHeight / 2f;
                    startY += separatorY;
                }
                else
                {
                    separatorY = remainingHeight / 3;
                    startY += separatorY;
                    separatorY = separatorY / (widgetsPerColumn - 1);
                }
            }                        

            int row, column;
            for (int i = 0; i < count; i++)
            {
                row = i / widgetsPerRow;
                column = i % widgetsPerRow;
                CategoryWidgets[i].OnGUI(startX + column * widgetWidth + column * separatorX, startY + row * widgetHeight + row * separatorY);
            }            
        }

        /*
        Rect area = new Rect(x, y, Width, Height);

        GUILayout.BeginArea(area);
        if (CategoryWidgets != null)
        {
            // The background
            GUILayout.Box("", GUILayout.Width(Width), GUILayout.Height(Height));

            float heightPerWidget = HeightPerGroupWidget;

            // The total
            float currentY = HeightSeparator;
            TotalWidget.OnGUI(new Rect(WidthPadding, HeightSeparator, WidthPerGroupWidget, HeightHeadline));

            int count = GroupWidgets.Count;
            currentY += HeightHeadline + HeightSeparator;
            for (int i = 0; i < count; i++)
            {
                GroupWidgets[i].OnGUI(new Rect(WidthPadding, currentY, WidthPerGroupWidget, heightPerWidget));
                currentY += heightPerWidget + HeightSeparator;
            }
        }
        GUILayout.EndArea();
        */
    }
}

public class MemoryProfilerEditorWindow : EditorWindow
{
    /*
    string[] inspectToolbarStrings = { "Textures", "Materials", "Meshes" };
    string[] inspectToolbarStrings2 = { "Textures", "Materials", "Meshes", "Missing" };

    enum InspectType
    {
        Textures, Materials, Meshes, Missing
    };

    bool IncludeDisabledObjects = true;
    bool IncludeSpriteAnimations = true;
    bool IncludeScriptReferences = true;
    bool IncludeGuiElements = true;
    bool thingsMissing = false;

    InspectType ActiveInspectType = InspectType.Textures;

    float ThumbnailWidth = 40;
    float ThumbnailHeight = 40;

    List<TextureDetails> ActiveTextures = new List<TextureDetails>();
    List<MaterialDetails> ActiveMaterials = new List<MaterialDetails>();
    List<MeshDetails> ActiveMeshDetails = new List<MeshDetails>();
    List<MissingGraphic> MissingObjects = new List<MissingGraphic>();

    Vector2 textureListScrollPos = new Vector2(0, 0);
    Vector2 materialListScrollPos = new Vector2(0, 0);
    Vector2 meshListScrollPos = new Vector2(0, 0);
    Vector2 missingListScrollPos = new Vector2(0, 0);

    int TotalTextureMemory = 0;
    int TotalMeshVertices = 0;
    int TotalMeshMemory = 0;

    bool ctrlPressed = false;        

    bool collectedInPlayingMode;    
    */

    Color defColor;
    static int MinWidth = 475;
    private GroupWidget TotalWidget { get; set; }

    private bool HasBeenSetup { get; set; }

    private GUIStyle LabelStyle { get; set; }    

    public static void Init()
    {
        MemoryProfilerEditorWindow window = (MemoryProfilerEditorWindow)EditorWindow.GetWindow(typeof(MemoryProfilerEditorWindow));
        //window.CheckResources();        
        window.Open();
    }

    public MemoryProfilerEditorWindow()
    {
        Setup();
    }

    private void Open()
    {

    }

    private void Setup()
    {
        long maxInBytes = (long)Util.MegaBytesToBytes(145f);
        minSize = new Vector2(MinWidth, 475);
        TotalWidget = new GroupWidget();
        TotalWidget.Setup("Tot ", (long)Util.MegaBytesToBytes(150f), maxInBytes, false);
    }

    private void InitStyles()
    {
        if (LabelStyle == null)
        {
            LabelStyle = new GUIStyle();
            LabelStyle.alignment = TextAnchor.MiddleCenter;
            LabelStyle.normal.textColor = Color.white;
        }
    }

    void OnGUI()
    {
        InitStyles();

        defColor = GUI.color;
        /*
        IncludeDisabledObjects = GUILayout.Toggle(IncludeDisabledObjects, "Include disabled objects", GUILayout.Width(300));
        IncludeSpriteAnimations = GUILayout.Toggle(IncludeSpriteAnimations, "Look in sprite animations", GUILayout.Width(300));
        GUI.color = new Color(0.8f, 0.8f, 1.0f, 1.0f);
        IncludeScriptReferences = GUILayout.Toggle(IncludeScriptReferences, "Look in behavior fields", GUILayout.Width(300));
        GUI.color = new Color(1.0f, 0.95f, 0.8f, 1.0f);

        IncludeGuiElements = GUILayout.Toggle(IncludeGuiElements, "Look in GUI elements", GUILayout.Width(300));                              
        */

        GUI.color = defColor;

        int margin = 5;
        int width = 120;
        int height = 65;
        int currentX = margin;
        int currentY = margin;
        GUILayout.BeginArea(new Rect(currentX, currentY, width, height));
        if (GUILayout.Button("Take a sample", GUILayout.Width(width), GUILayout.Height(40)))
        {
            AbstractMemorySample sample = MP_TakeASampleFromScene(false);
            SamplePanel_Show(sample);
        }

        if (GUILayout.Button("CleanUp", GUILayout.Width(width), GUILayout.Height(20)))
        {
            MP_Clear();
            SamplePanel_Clear();
        }
        GUILayout.EndArea();

        // Size Strategy
        currentX += width;
        GUILayout.BeginArea(new Rect(currentX, currentY, width, height));
        GUILayout.Label("Size Strategy", LabelStyle);
        SizeStrategy_Init();
        string currentValue = MP_MemoryProfiler.SizeStrategy.ToString();
        if (GUILayout.Button(currentValue, GUILayout.Width(width), GUILayout.Height(40)))
        {
            SelectionPopupWindow.Show(m_sizeStrategyOptions.ToArray(), SizeStrategy_OnValueChanged);
        }
        GUILayout.EndArea();

        // Categories
        currentX += width;
        GUILayout.BeginArea(new Rect(currentX, currentY, width, height));
        CategorySet_Init();
        GUILayout.Label("Categories", LabelStyle);
        if (GUILayout.Button(CategorySet_Current, GUILayout.Width(width), GUILayout.Height(40)))
        {
            SelectionPopupWindow.Show(CategorySet_Names.ToArray(), CategorySet_OnValueChanged);
        }
        GUILayout.EndArea();

        // Save/Load sample
        currentX += width + margin;
        height = 85;
        GUILayout.BeginArea(new Rect(currentX, currentY, width, height));

        // Save sample button 
        GUI.enabled = SamplePanel_CurrentSample != null;
        if (GUILayout.Button("Save sample", GUILayout.Width(width), GUILayout.Height(40)))
        {
            File_SaveSample();
        }

        GUI.enabled = false;
        if (GUILayout.Button("Load sample", GUILayout.Width(width), GUILayout.Height(40)))
        {
            File_LoadSample();
        }

        GUI.enabled = true;
        GUILayout.EndArea();

        /*GUILayout.BeginArea(new Rect(position.width - 85, 65, 100, 25));
        EditorGUI.ProgressBar(new Rect(0, 0, 100, 25), 0.5f, "Total");
        GUILayout.EndArea();
        */

        //RemoveDestroyedResources();        

        //GUILayout.BeginArea(new Rect(position.width - 85, 65, 100, 25));
        //TotalWidget.OnGUI(new Rect(0, 70, 100, 30));
        //GUILayout.EndArea();

        GUILayout.Space(30);

        currentY += height;
        /*EverythingCategoryWidget.OnGUI(0, currentY);
        currentY += EverythingCategoryWidget.Height;
        */

        /*
        if (SampleWidget != null)
        {
            SampleWidget.OnGUI(0, currentY, position.width, position.height);
        }
        */


        if (SamplePanelWidget != null)
        {
            SamplePanelWidget.OnGUI(0, currentY, position.width, position.height);
        }

        /*
        if (thingsMissing == true)
        {
            EditorGUI.HelpBox(new Rect(8, 75, 300, 25), "Some GameObjects are missing graphical elements.", MessageType.Error);
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("Textures " + ActiveTextures.Count + " - " + FormatSizeString(TotalTextureMemory));
        GUILayout.Label("Materials " + ActiveMaterials.Count);
        //GUILayout.Label("Meshes "+ActiveMeshDetails.Count+" - "+TotalMeshVertices+" verts");
        GUILayout.Label("Meshes " + ActiveMeshDetails.Count + " - " + FormatSizeString((int)(TotalMeshMemory / 1024f)));
        GUILayout.EndHorizontal();
        if (thingsMissing == true)
        {
            ActiveInspectType = (InspectType)GUILayout.Toolbar((int)ActiveInspectType, inspectToolbarStrings2);
        }
        else
        {
            ActiveInspectType = (InspectType)GUILayout.Toolbar((int)ActiveInspectType, inspectToolbarStrings);
        }

        ctrlPressed = Event.current.control || Event.current.command;

        switch (ActiveInspectType)
        {
            case InspectType.Textures:
                ListTextures();
                break;
            case InspectType.Materials:
                ListMaterials();
                break;
            case InspectType.Meshes:
                ListMeshes();
                break;
            case InspectType.Missing:
                ListMissing();
                break;
        }
        */
    }

    /*private void RemoveDestroyedResources()
    {
        if (collectedInPlayingMode != Application.isPlaying)
        {
            ActiveTextures.Clear();
            ActiveMaterials.Clear();
            ActiveMeshDetails.Clear();
            MissingObjects.Clear();
            thingsMissing = false;
            collectedInPlayingMode = Application.isPlaying;
        }

        ActiveTextures.RemoveAll(x => !x.texture);
        ActiveTextures.ForEach(delegate (TextureDetails obj)
        {
            obj.FoundInAnimators.RemoveAll(x => !x);
            obj.FoundInMaterials.RemoveAll(x => !x);
            obj.FoundInRenderers.RemoveAll(x => !x);
            obj.FoundInScripts.RemoveAll(x => !x);
            obj.FoundInGraphics.RemoveAll(x => !x);
        });

        ActiveMaterials.RemoveAll(x => !x.material);
        ActiveMaterials.ForEach(delegate (MaterialDetails obj)
        {
            obj.FoundInRenderers.RemoveAll(x => !x);
            obj.FoundInGraphics.RemoveAll(x => !x);
        });

        ActiveMeshDetails.RemoveAll(x => !x.Mesh);
        ActiveMeshDetails.ForEach(delegate (MeshDetails obj)
        {
            obj.FoundInGos.RemoveAll(x => !x);
            obj.FoundInMeshFilters.RemoveAll(x => !x);
            obj.FoundInSkinnedMeshRenderer.RemoveAll(x => !x);
        });

        TotalTextureMemory = 0;
        foreach (TextureDetails tTextureDetails in ActiveTextures) TotalTextureMemory += tTextureDetails.memSizeKB;

        TotalMeshVertices = 0;
        TotalMeshMemory = 0;
        foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
        {
            TotalMeshVertices += tMeshDetails.Mesh.vertexCount;
            TotalMeshMemory += UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(tMeshDetails.Mesh);
        }
    }*/

    int GetBitsPerPixel(TextureFormat format)
    {
        switch (format)
        {
            case TextureFormat.Alpha8: //	 Alpha-only texture format.
                return 8;
            case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
                return 16;
            case TextureFormat.RGBA4444: //	 A 16 bits/pixel texture format.
                return 16;
            case TextureFormat.RGB24:   // A color texture format.
                return 24;
            case TextureFormat.RGBA32:  //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.ARGB32:  //Color with an alpha channel texture format.
                return 32;
            case TextureFormat.RGB565:  //	 A 16 bit color texture format.
                return 16;
            case TextureFormat.DXT1:    // Compressed color texture format.
                return 4;
            case TextureFormat.DXT5:    // Compressed color with alpha channel texture format.
                return 8;
            /*
			case TextureFormat.WiiI4:	// Wii texture format.
			case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
			case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
			case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
			case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
			case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
			case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
			case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
				return 0;  //Not supported yet
			*/
            case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
                return 2;
            case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
                return 2;
            case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
                return 4;
            case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
                return 4;
            case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
                return 4;
            case TextureFormat.ETC2_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
                return 8;
            case TextureFormat.BGRA32://	 Format returned by iPhone camera
                return 32;
        }
        return 0;
    }

    int CalculateTextureSizeBytes(Texture tTexture)
    {

        int tWidth = tTexture.width;
        int tHeight = tTexture.height;
        if (tTexture is Texture2D)
        {
            Texture2D tTex2D = tTexture as Texture2D;
            int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
            int mipMapCount = tTex2D.mipmapCount;
            int mipLevel = 1;
            int tSize = 0;
            while (mipLevel <= mipMapCount)
            {
                tSize += tWidth * tHeight * bitsPerPixel / 8;
                tWidth = tWidth / 2;
                tHeight = tHeight / 2;
                mipLevel++;
            }
            return tSize;
        }
        if (tTexture is Texture2DArray)
        {
            Texture2DArray tTex2D = tTexture as Texture2DArray;
            int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
            int mipMapCount = 10;
            int mipLevel = 1;
            int tSize = 0;
            while (mipLevel <= mipMapCount)
            {
                tSize += tWidth * tHeight * bitsPerPixel / 8;
                tWidth = tWidth / 2;
                tHeight = tHeight / 2;
                mipLevel++;
            }
            return tSize * ((Texture2DArray)tTex2D).depth;
        }
        if (tTexture is Cubemap)
        {
            Cubemap tCubemap = tTexture as Cubemap;
            int bitsPerPixel = GetBitsPerPixel(tCubemap.format);
            return tWidth * tHeight * 6 * bitsPerPixel / 8;
        }
        return 0;
    }


    void SelectObject(Object selectedObject, bool append)
    {
        if (append)
        {
            List<Object> currentSelection = new List<Object>(Selection.objects);
            // Allow toggle selection
            if (currentSelection.Contains(selectedObject)) currentSelection.Remove(selectedObject);
            else currentSelection.Add(selectedObject);

            Selection.objects = currentSelection.ToArray();
        }
        else Selection.activeObject = selectedObject;
    }

    void SelectObjects(List<Object> selectedObjects, bool append)
    {
        if (append)
        {
            List<Object> currentSelection = new List<Object>(Selection.objects);
            currentSelection.AddRange(selectedObjects);
            Selection.objects = currentSelection.ToArray();
        }
        else Selection.objects = selectedObjects.ToArray();
    }

    /*void ListTextures()
    {
        textureListScrollPos = EditorGUILayout.BeginScrollView(textureListScrollPos);

        foreach (TextureDetails tDetails in ActiveTextures)
        {
            GUILayout.BeginHorizontal();
            Texture tex = new Texture();
            tex = tDetails.texture;
            if (tDetails.texture.GetType() == typeof(Texture2DArray) || tDetails.texture.GetType() == typeof(Cubemap))
            {
                tex = AssetPreview.GetMiniThumbnail(tDetails.texture);
            }
            GUILayout.Box(tex, GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));

            if (tDetails.instance == true)
                GUI.color = new Color(0.8f, 0.8f, defColor.b, 1.0f);
            if (tDetails.isgui == true)
                GUI.color = new Color(defColor.r, 0.95f, 0.8f, 1.0f);
            if (tDetails.isSky)
                GUI.color = new Color(0.9f, defColor.g, defColor.b, 1.0f);
            if (GUILayout.Button(tDetails.texture.name, GUILayout.Width(150)))
            {
                SelectObject(tDetails.texture, ctrlPressed);
            }
            GUI.color = defColor;

            string sizeLabel = "" + tDetails.texture.width + "x" + tDetails.texture.height;
            if (tDetails.isCubeMap) sizeLabel += "x6";
            if (tDetails.texture.GetType() == typeof(Texture2DArray))
                sizeLabel += "[]\n" + ((Texture2DArray)tDetails.texture).depth + "depths";
            sizeLabel += " - " + tDetails.mipMapCount + "mip\n" + FormatSizeString(tDetails.memSizeKB) + " - " + tDetails.format;

            GUILayout.Label(sizeLabel, GUILayout.Width(120));

            if (GUILayout.Button(tDetails.FoundInMaterials.Count + " Mat", GUILayout.Width(50)))
            {
                SelectObjects(tDetails.FoundInMaterials, ctrlPressed);
            }

            HashSet<Object> FoundObjects = new HashSet<Object>();
            foreach (Renderer renderer in tDetails.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
            foreach (Animator animator in tDetails.FoundInAnimators) FoundObjects.Add(animator.gameObject);
            foreach (Graphic graphic in tDetails.FoundInGraphics) FoundObjects.Add(graphic.gameObject);
            foreach (MonoBehaviour script in tDetails.FoundInScripts) FoundObjects.Add(script.gameObject);
            if (GUILayout.Button(FoundObjects.Count + " GO", GUILayout.Width(50)))
            {
                SelectObjects(new List<Object>(FoundObjects), ctrlPressed);
            }

            GUILayout.EndHorizontal();
        }
        if (ActiveTextures.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            //GUILayout.Box(" ",GUILayout.Width(ThumbnailWidth),GUILayout.Height(ThumbnailHeight));
            if (GUILayout.Button("Select \n All", GUILayout.Width(ThumbnailWidth * 2)))
            {
                List<Object> AllTextures = new List<Object>();
                foreach (TextureDetails tDetails in ActiveTextures) AllTextures.Add(tDetails.texture);
                SelectObjects(AllTextures, ctrlPressed);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    void ListMaterials()
    {
        materialListScrollPos = EditorGUILayout.BeginScrollView(materialListScrollPos);

        foreach (MaterialDetails tDetails in ActiveMaterials)
        {
            if (tDetails.material != null)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Box(AssetPreview.GetAssetPreview(tDetails.material), GUILayout.Width(ThumbnailWidth), GUILayout.Height(ThumbnailHeight));

                if (tDetails.instance == true)
                    GUI.color = new Color(0.8f, 0.8f, defColor.b, 1.0f);
                if (tDetails.isgui == true)
                    GUI.color = new Color(defColor.r, 0.95f, 0.8f, 1.0f);
                if (tDetails.isSky)
                    GUI.color = new Color(0.9f, defColor.g, defColor.b, 1.0f);
                if (GUILayout.Button(tDetails.material.name, GUILayout.Width(150)))
                {
                    SelectObject(tDetails.material, ctrlPressed);
                }
                GUI.color = defColor;

                string shaderLabel = tDetails.material.shader != null ? tDetails.material.shader.name : "no shader";
                GUILayout.Label(shaderLabel, GUILayout.Width(200));

                if (GUILayout.Button((tDetails.FoundInRenderers.Count + tDetails.FoundInGraphics.Count) + " GO", GUILayout.Width(50)))
                {
                    List<Object> FoundObjects = new List<Object>();
                    foreach (Renderer renderer in tDetails.FoundInRenderers) FoundObjects.Add(renderer.gameObject);
                    foreach (Graphic graphic in tDetails.FoundInGraphics) FoundObjects.Add(graphic.gameObject);
                    SelectObjects(FoundObjects, ctrlPressed);
                }


                GUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void ListMeshes()
    {
        meshListScrollPos = EditorGUILayout.BeginScrollView(meshListScrollPos);

        foreach (MeshDetails tDetails in ActiveMeshDetails)
        {
            if (tDetails.Mesh != null)
            {
                GUILayout.BeginHorizontal();
                string name = tDetails.Mesh.name;
                if (name == null || name.Count() < 1)
                    name = tDetails.FoundInMeshFilters[0].gameObject.name;
                if (tDetails.instance == true)
                    GUI.color = new Color(0.8f, 0.8f, defColor.b, 1.0f);
                if (GUILayout.Button(name, GUILayout.Width(150)))
                {
                    SelectObject(tDetails.Mesh, ctrlPressed);
                }
                GUI.color = defColor;
                string sizeLabel = "" + tDetails.Mesh.vertexCount + " vert";
                GUILayout.Label(sizeLabel, GUILayout.Width(100));

                sizeLabel = "" + FormatSizeString(tDetails.memSizeKB);
                GUILayout.Label(sizeLabel, GUILayout.Width(100));

                if (GUILayout.Button(tDetails.FoundInMeshFilters.Count + " GO", GUILayout.Width(50)))
                {
                    List<Object> FoundObjects = new List<Object>();
                    foreach (MeshFilter meshFilter in tDetails.FoundInMeshFilters) FoundObjects.Add(meshFilter.gameObject);
                    SelectObjects(FoundObjects, ctrlPressed);
                }

                if (GUILayout.Button(tDetails.FoundInGos.Count + " Collider", GUILayout.Width(50)))
                {
                    List<Object> FoundObjects = new List<Object>();
                    foreach (MeshFilter meshFilter in tDetails.FoundInMeshFilters) FoundObjects.Add(meshFilter.gameObject);
                    SelectObjects(FoundObjects, ctrlPressed);
                }

                if (tDetails.FoundInSkinnedMeshRenderer.Count > 0)
                {
                    if (GUILayout.Button(tDetails.FoundInSkinnedMeshRenderer.Count + " skinned mesh GO", GUILayout.Width(140)))
                    {
                        List<Object> FoundObjects = new List<Object>();
                        foreach (SkinnedMeshRenderer skinnedMeshRenderer in tDetails.FoundInSkinnedMeshRenderer)
                            FoundObjects.Add(skinnedMeshRenderer.gameObject);
                        SelectObjects(FoundObjects, ctrlPressed);
                    }
                }
                else
                {
                    GUI.color = new Color(defColor.r, defColor.g, defColor.b, 0.5f);
                    GUILayout.Label("   0 skinned mesh");
                    GUI.color = defColor;
                }


                GUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void ListMissing()
    {
        missingListScrollPos = EditorGUILayout.BeginScrollView(missingListScrollPos);
        foreach (MissingGraphic dMissing in MissingObjects)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(dMissing.name, GUILayout.Width(150)))
                SelectObject(dMissing.Object, ctrlPressed);
            GUILayout.Label("missing ", GUILayout.Width(48));
            switch (dMissing.type)
            {
                case "mesh":
                    GUI.color = new Color(0.8f, 0.8f, defColor.b, 1.0f);
                    break;
                case "sprite":
                    GUI.color = new Color(defColor.r, 0.8f, 0.8f, 1.0f);
                    break;
                case "material":
                    GUI.color = new Color(0.8f, defColor.g, 0.8f, 1.0f);
                    break;
            }
            GUILayout.Label(dMissing.type);
            GUI.color = defColor;
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
    */

    string FormatSizeString(float memSizeKB)
    {
        if (memSizeKB < 1024f) return "" + memSizeKB + "k";
        else
        {
            float memSizeMB = ((float)memSizeKB) / 1024.0f;
            return memSizeMB.ToString("0.00") + "Mb";
        }
    }


    /*TextureDetails FindTextureDetails(Texture tTexture)
    {
        foreach (TextureDetails tTextureDetails in ActiveTextures)
        {
            if (tTextureDetails.texture == tTexture) return tTextureDetails;
        }
        return null;

    }

    MaterialDetails FindMaterialDetails(Material tMaterial)
    {
        foreach (MaterialDetails tMaterialDetails in ActiveMaterials)
        {
            if (tMaterialDetails.material == tMaterial) return tMaterialDetails;
        }
        return null;

    }
    */

    private string GetAssetPath(UnityEngine.Object go)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(go);
#else
	    return "/PathUnknown/" + go.name;
#endif
    }

    /*
    MeshDetails FindMeshDetails(Mesh tMesh)
    {
        foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
        {
            if (tMeshDetails.Mesh.GetInstanceID() == tMesh.GetInstanceID()) return tMeshDetails;
        }
        return null;

    }
    */


    void CheckResources()
    {
        /*
        ActiveTextures.Clear();
        ActiveMaterials.Clear();
        ActiveMeshDetails.Clear();
        MissingObjects.Clear();
        thingsMissing = false;
        */

        /*Renderer[] renderers = FindObjects<Renderer>();

		MaterialDetails skyMat = new MaterialDetails ();
		skyMat.material = RenderSettings.skybox;
		skyMat.isSky = true;
		ActiveMaterials.Add (skyMat);

        //Debug.Log("Total renderers "+renderers.Length);
        foreach (Renderer renderer in renderers)
		{
			//Debug.Log("Renderer is "+renderer.name);
			foreach (Material material in renderer.sharedMaterials)
			{

				MaterialDetails tMaterialDetails = FindMaterialDetails(material);
				if (tMaterialDetails == null)
				{
					tMaterialDetails = new MaterialDetails();
					tMaterialDetails.material = material;
					ActiveMaterials.Add(tMaterialDetails);
				}
				tMaterialDetails.FoundInRenderers.Add(renderer);
			}

			if (renderer is SpriteRenderer)
			{
				SpriteRenderer tSpriteRenderer = (SpriteRenderer)renderer;

				if (tSpriteRenderer.sprite != null) {
					var tSpriteTextureDetail = GetTextureDetail (tSpriteRenderer.sprite.texture, renderer);
					if (!ActiveTextures.Contains (tSpriteTextureDetail)) {
						ActiveTextures.Add (tSpriteTextureDetail);
					}
				} else if (tSpriteRenderer.sprite == null) {
					MissingGraphic tMissing = new MissingGraphic ();
					tMissing.Object = tSpriteRenderer.transform;
					tMissing.type = "sprite";
					tMissing.name = tSpriteRenderer.transform.name;
					MissingObjects.Add (tMissing);
					thingsMissing = true;
				}
			}
		}

		if (IncludeGuiElements)
		{
			Graphic[] graphics = FindObjects<Graphic>();

			foreach(Graphic graphic in graphics)
			{
				if (graphic.mainTexture)
				{
					var tSpriteTextureDetail = GetTextureDetail(graphic.mainTexture, graphic);
					if (!ActiveTextures.Contains(tSpriteTextureDetail))
					{
						ActiveTextures.Add(tSpriteTextureDetail);
					}
				}

				if (graphic.materialForRendering)
				{
					MaterialDetails tMaterialDetails = FindMaterialDetails(graphic.materialForRendering);
					if (tMaterialDetails == null)
					{
						tMaterialDetails = new MaterialDetails();
						tMaterialDetails.material = graphic.materialForRendering;
						tMaterialDetails.isgui = true;
						ActiveMaterials.Add(tMaterialDetails);
					}
					tMaterialDetails.FoundInGraphics.Add(graphic);
				}
			}
		}

		foreach (MaterialDetails tMaterialDetails in ActiveMaterials)
		{
			Material tMaterial = tMaterialDetails.material;
			if (tMaterial != null)
			{
				var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { tMaterial });
				foreach (Object obj in dependencies)
				{
					if (obj is Texture)
					{
						Texture tTexture = obj as Texture;
						var tTextureDetail = GetTextureDetail(tTexture, tMaterial, tMaterialDetails);
						tTextureDetail.isSky = tMaterialDetails.isSky;
						tTextureDetail.instance = tMaterialDetails.instance;
						tTextureDetail.isgui = tMaterialDetails.isgui;
						ActiveTextures.Add(tTextureDetail);
					}
				}

				//if the texture was downloaded, it won't be included in the editor dependencies
				if (tMaterial.HasProperty ("_MainTex")) {
					if (tMaterial.mainTexture != null && !dependencies.Contains (tMaterial.mainTexture)) {
						var tTextureDetail = GetTextureDetail (tMaterial.mainTexture, tMaterial, tMaterialDetails);
						ActiveTextures.Add (tTextureDetail);
					}
				}
			}
		}
        */

        /*
        HideFlags hideFlagMask = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
        HideFlags hideFlagMask1 = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontUnloadUnusedAsset;
        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
        int count = textures.Length;
        Texture t;
        for (int i = 0; i < count; i++)
        {
            t = textures[i];
            if (t.hideFlags == HideFlags.HideAndDontSave || t.hideFlags == hideFlagMask || t.hideFlags == hideFlagMask1)
                continue;

            TextureDetails tDetails = GetTextureDetail(t);
            if (!ActiveTextures.Contains(tDetails))
            {
                ActiveTextures.Add(tDetails);
            }
        }

        Mesh[] meshes = Resources.FindObjectsOfTypeAll<Mesh>();
        count = meshes.Length;
        Mesh m;
        for (int i = 0; i < count; i++)
        {
            m = meshes[i];
            if (m.hideFlags == HideFlags.HideAndDontSave || m.hideFlags == hideFlagMask || m.hideFlags == hideFlagMask1)
                continue;

            MeshDetails mDetails = FindMeshDetails(m);
            if (mDetails == null)
            {
                mDetails = new MeshDetails();
                mDetails.Mesh = meshes[i];
                ActiveMeshDetails.Add(mDetails);
            }
        }
        */

        /*GameObject[] gos = GetAllRootGameObjectInScene();
        if (gos != null)
        {
            count = gos.Length;            
            Object[] os = new Object[count];
            for (int i = 0; i < count; i++)
            {
                os[i] = gos[i] as Object;                
            }
            
            Object[] dependencies = EditorUtility.CollectDependencies(os);
            if (dependencies != null)
            {
                count = dependencies.Length;
                for (int i = 0; i < count; i++)
                {
                    if (dependencies[i] is Texture)
                    {
                        if (textures.Contains(dependencies[i] as Texture))
                        {
                            TextureDetails tDetails = GetTextureDetail(textures[i]);
                            if (!ActiveTextures.Contains(tDetails))
                            {
                                ActiveTextures.Add(tDetails);
                            }
                        }
                    }
                }
            }
        }
        */

        /*
        Mesh[] meshes = Resources.FindObjectsOfTypeAll<Mesh>();
        count = meshes.Length;
        for (int i = 0; i < count; i++)
        {
            if (meshes[i] is TextMesh)
                continue;

            MeshDetails tMeshDetails = FindMeshDetails(meshes[i]);
            if (tMeshDetails == null)
            {
                tMeshDetails = new MeshDetails();
                tMeshDetails.Mesh = meshes[i];
                ActiveMeshDetails.Add(tMeshDetails);
            }          
        }*/

        /*Object[] objs = Resources.FindObjectsOfTypeAll(typeof(Object));
        int count = objs.Length;
        for (int i = 0; i < count; i++)
        {
            if (objs[i] is Texture)
            {
                TextureDetails tDetails = GetTextureDetail(objs[i] as Texture);
                if (!ActiveTextures.Contains(tDetails))
                {
                    ActiveTextures.Add(tDetails);
                }
            }           
        }*/

        /*Texture[] textures = FindObjects<Texture>();
        int count = textures.Length;
        for (int i = 0; i < count; i++)
        {          
            TextureDetails tDetails = GetTextureDetail(textures[i]);
            if (!ActiveTextures.Contains(tDetails))
            {
                ActiveTextures.Add(tDetails);
            }            
        } */

        /*MeshFilter[] meshFilters = FindObjects<MeshFilter>();

        int count = meshFilters.Length;
        MeshFilter tMeshFilter;
        Mesh tMesh;
        for (int i = 0; i < count; i++)
		{
            tMeshFilter = meshFilters[i];
			tMesh = tMeshFilter.sharedMesh;
			if (tMesh != null)
			{
				MeshDetails tMeshDetails = FindMeshDetails(tMesh);
				if (tMeshDetails == null)
				{
					tMeshDetails = new MeshDetails();
					tMeshDetails.Mesh = tMesh;
					ActiveMeshDetails.Add(tMeshDetails);
				}
                else if (tMeshDetails.FoundInMeshFilters.Count > 1)
                {
                    string path = GetAssetPath(tMesh);
                    if (string.IsNullOrEmpty(path))
                    {
                        string txt = "meshFilters count = " + count + " misma mesh: ";
                        for (int j = 0; j < tMeshDetails.FoundInMeshFilters.Count; j++)
                        {
                            txt += tMeshDetails.FoundInMeshFilters[j].gameObject.name + ",";
                        }
                        Debug.Log(txt);
                    }
                        
                    //return null;
                }
                tMeshDetails.FoundInMeshFilters.Add(tMeshFilter);
			} else if (tMesh == null && tMeshFilter.transform.GetComponent("TextContainer")== null) {
				MissingGraphic tMissing = new MissingGraphic ();
				tMissing.Object = tMeshFilter.transform;
				tMissing.type = "mesh";
				tMissing.name = tMeshFilter.transform.name;
				MissingObjects.Add (tMissing);
				thingsMissing = true;
			}
           
            MeshRenderer meshRenderer = tMeshFilter.transform.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.sharedMaterial == null) {
				MissingGraphic tMissing = new MissingGraphic ();
				tMissing.Object = tMeshFilter.transform;
				tMissing.type = "material";
				tMissing.name = tMeshFilter.transform.name;
				MissingObjects.Add (tMissing);
				thingsMissing = true;
			}
		}

        MeshCollider[] meshColliders = FindObjects<MeshCollider>();

        count = meshColliders.Length;
        MeshCollider tMeshCollider;
        for (int i = 0; i < count; i++)
        {
            tMeshCollider = meshColliders[i];
            tMesh = tMeshCollider.sharedMesh;
            if (tMesh != null)
            {
                MeshDetails tMeshDetails = FindMeshDetails(tMesh);
                if (tMeshDetails == null)
                {
                    tMeshDetails = new MeshDetails();
                    tMeshDetails.Mesh = tMesh;
                    ActiveMeshDetails.Add(tMeshDetails);
                }                
                tMeshDetails.FoundInGos.Add(tMeshCollider.gameObject);
            }
        }

        SkinnedMeshRenderer[] skinnedMeshRenderers = FindObjects<SkinnedMeshRenderer>();

        count = skinnedMeshRenderers.Length;
        SkinnedMeshRenderer tSkinnedMeshRenderer;
        for (int i = 0; i < count; i++)
        {
            tSkinnedMeshRenderer = skinnedMeshRenderers[i];            
			tMesh = tSkinnedMeshRenderer.sharedMesh;
			if (tMesh != null)
			{
				MeshDetails tMeshDetails = FindMeshDetails(tMesh);
				if (tMeshDetails == null)
				{
					tMeshDetails = new MeshDetails();
					tMeshDetails.Mesh = tMesh;
					ActiveMeshDetails.Add(tMeshDetails);
				}
				tMeshDetails.FoundInSkinnedMeshRenderer.Add(tSkinnedMeshRenderer);
			} else if (tMesh == null) {
				MissingGraphic tMissing = new MissingGraphic ();
				tMissing.Object = tSkinnedMeshRenderer.transform;
				tMissing.type = "mesh";
				tMissing.name = tSkinnedMeshRenderer.transform.name;
				MissingObjects.Add (tMissing);
				thingsMissing = true;
			}
			if (tSkinnedMeshRenderer.sharedMaterial == null) {
				MissingGraphic tMissing = new MissingGraphic ();
				tMissing.Object = tSkinnedMeshRenderer.transform;
				tMissing.type = "material";
				tMissing.name = tSkinnedMeshRenderer.transform.name;
				MissingObjects.Add (tMissing);
				thingsMissing = true;
			}
		}

		if (IncludeSpriteAnimations)
		{
			Animator[] animators = FindObjects<Animator>();
			foreach (Animator anim in animators)
			{
				#if UNITY_4_6 || UNITY_4_5 || UNITY_4_4 || UNITY_4_3
				UnityEditorInternal.AnimatorController ac = anim.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
				#elif UNITY_5
				UnityEditor.Animations.AnimatorController ac = anim.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
				#endif

				//Skip animators without layers, this can happen if they don't have an animator controller.
				if (!ac || ac.layers == null || ac.layers.Length == 0)
					continue;

				for (int x = 0; x < anim.layerCount; x++)
				{
					#if UNITY_4_6 || UNITY_4_5 || UNITY_4_4 || UNITY_4_3
					UnityEditorInternal.StateMachine sm = ac.GetLayer(x).stateMachine;
					int cnt = sm.stateCount;
					#elif UNITY_5
					UnityEditor.Animations.AnimatorStateMachine sm = ac.layers[x].stateMachine;
					int cnt = sm.states.Length;
					#endif

					for (int i = 0; i < cnt; i++)
					{
						#if UNITY_4_6 || UNITY_4_5 || UNITY_4_4 || UNITY_4_3
						UnityEditorInternal.State state = sm.GetState(i);
						Motion m = state.GetMotion();
						#elif UNITY_5
						UnityEditor.Animations.AnimatorState state = sm.states[i].state;
						Motion m = state.motion;
						#endif
                        if (m != null)
						{
							AnimationClip clip = m as AnimationClip;

						    if (clip != null)
						    {
						        EditorCurveBinding[] ecbs = AnimationUtility.GetObjectReferenceCurveBindings(clip);

						        foreach (EditorCurveBinding ecb in ecbs)
						        {
						            if (ecb.propertyName == "m_Sprite")
						            {
						                foreach (ObjectReferenceKeyframe keyframe in AnimationUtility.GetObjectReferenceCurve(clip, ecb))
						                {
						                    Sprite tSprite = keyframe.value as Sprite;

						                    if (tSprite != null)
						                    {
						                        var tTextureDetail = GetTextureDetail(tSprite.texture, anim);
						                        if (!ActiveTextures.Contains(tTextureDetail))
						                        {
						                            ActiveTextures.Add(tTextureDetail);
						                        }
						                    }
						                }
						            }
						        }
						    }
						}
					}
				}

			}
		}

		if (IncludeScriptReferences)
		{
			MonoBehaviour[] scripts = FindObjects<MonoBehaviour>();
			foreach (MonoBehaviour script in scripts)
			{
				BindingFlags flags = BindingFlags.Public | BindingFlags.Instance; // only public non-static fields are bound to by Unity.
				FieldInfo[] fields = script.GetType().GetFields(flags);

				foreach (FieldInfo field in fields)
				{
					System.Type fieldType = field.FieldType;
					if (fieldType == typeof(Sprite))
					{
						Sprite tSprite = field.GetValue(script) as Sprite;
						if (tSprite != null)
						{
							var tSpriteTextureDetail = GetTextureDetail(tSprite.texture, script);
							if (!ActiveTextures.Contains(tSpriteTextureDetail))
							{
								ActiveTextures.Add(tSpriteTextureDetail);
							}
						}
					}if (fieldType == typeof(Mesh))
					{
						tMesh = field.GetValue(script) as Mesh;
						if (tMesh != null)
						{
							MeshDetails tMeshDetails = FindMeshDetails(tMesh);
							if (tMeshDetails == null)
							{
								tMeshDetails = new MeshDetails();
								tMeshDetails.Mesh = tMesh;
								tMeshDetails.instance = true;
								ActiveMeshDetails.Add(tMeshDetails);
							}
						}
					}if (fieldType == typeof(Material))
					{
						Material tMaterial = field.GetValue(script) as Material;
						if (tMaterial != null)
						{
							MaterialDetails tMatDetails = FindMaterialDetails(tMaterial);
							if (tMatDetails == null)
							{
								tMatDetails = new MaterialDetails();
								tMatDetails.instance = true;
								tMatDetails.material = tMaterial;
								if(!ActiveMaterials.Contains(tMatDetails))
									ActiveMaterials.Add(tMatDetails);
							}
							if (tMaterial.mainTexture)
							{
								var tSpriteTextureDetail = GetTextureDetail(tMaterial.mainTexture);
								if (!ActiveTextures.Contains(tSpriteTextureDetail))
								{
									ActiveTextures.Add(tSpriteTextureDetail);
								}
							}
							var dependencies = EditorUtility.CollectDependencies(new UnityEngine.Object[] { tMaterial });
							foreach (Object obj in dependencies)
							{
								if (obj is Texture)
								{
									Texture tTexture = obj as Texture;
									var tTextureDetail = GetTextureDetail(tTexture, tMaterial, tMatDetails);
									if(!ActiveTextures.Contains(tTextureDetail))
										ActiveTextures.Add(tTextureDetail);
								}
							}
						}
					}
				}
			}
		}        
        */

        /*
        TotalTextureMemory = 0;
        foreach (TextureDetails tTextureDetails in ActiveTextures) TotalTextureMemory += tTextureDetails.memSizeKB;

        TotalMeshVertices = 0;
        TotalMeshMemory = 0;
        foreach (MeshDetails tMeshDetails in ActiveMeshDetails)
        {
            TotalMeshVertices += tMeshDetails.Mesh.vertexCount;
            TotalMeshMemory += UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(tMeshDetails.Mesh);
        }

        // Sort by size, descending
        ActiveTextures.Sort(delegate (TextureDetails details1, TextureDetails details2) { return details2.memSizeKB - details1.memSizeKB; });
        ActiveTextures = ActiveTextures.Distinct().ToList();
        ActiveMeshDetails.Sort(delegate (MeshDetails details1, MeshDetails details2) { return (int)(details2.memSizeBytes - details1.memSizeBytes); });

        collectedInPlayingMode = Application.isPlaying;
        */
    }

    private static GameObject[] GetAllRootGameObjectInScene()
    {
        List<GameObject> allGo = new List<GameObject>();
        for (int sceneIdx = 0; sceneIdx < UnityEngine.SceneManagement.SceneManager.sceneCount; ++sceneIdx)
        {
            allGo.AddRange(UnityEngine.SceneManagement.SceneManager.GetSceneAt(sceneIdx).GetRootGameObjects().ToArray());
        }

        GameObject singletons = GameObject.Find("Singletons");
        if (singletons != null)
            allGo.Add(singletons);

        return allGo.ToArray();
    }

    private static GameObject[] GetAllRootGameObjects()
    {
#if !UNITY_5_3_OR_NEWER
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToArray();
#else
        /*List<GameObject> allGo = new List<GameObject>();
        for (int sceneIdx = 0; sceneIdx < UnityEngine.SceneManagement.SceneManager.sceneCount; ++sceneIdx){
            allGo.AddRange( UnityEngine.SceneManagement.SceneManager.GetSceneAt(sceneIdx).GetRootGameObjects().ToArray() );
        }

        GameObject singletons = GameObject.Find("Singletons");
        if (singletons != null)
            allGo.Add(singletons);
            */
        //List<GameObject> allGo = GameObjectExt.FindAllObjectsInScene(true);
        List<GameObject> allGo = new List<GameObject>();

        // Resources.FindObjectsOfTypeAll() function is used because GameObject.FindObjectsOfType() doesn't return inactive game objects and
        // SceneManager.GetScene() doesn't retrun game objects marked as Don'tDestroyOnLoad
        GameObject[] gos = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
        if (gos != null)
        {
            int count = gos.Length;
            GameObject go;
            for (int i = 0; i < count; i++)
            {
                go = gos[i];
                // Resources.FindObjectsOfTypeAll() also returns internal stuff so we need to be extra careful about the game ojects returned by this
                // function
                //if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                // if (go.hideFlags != HideFlags.None)
                //continue;

                //if (go.activeInHierarchy || _includeInactive)
                //objectsInScene.Add(go);
                allGo.Add(go);

                allGo.ToArray();
            }
        }

        return allGo.ToArray();
#endif
    }

    /*private T[] FindObjects<T>() where T : Object
    {
        if (IncludeDisabledObjects)
        {
            List<T> meshfilters = new List<T>();
            GameObject[] allGo = GetAllRootGameObjects();
            foreach (GameObject go in allGo)
            {
                Transform[] tgo = go.GetComponentsInChildren<Transform>(true).ToArray();
                foreach (Transform tr in tgo)
                {
                    if (tr.GetComponent<T>())
                        meshfilters.Add(tr.GetComponent<T>());
                }
            }
            return (T[])meshfilters.ToArray();
        }
        else
            return (T[])FindObjectsOfType(typeof(T));
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Material tMaterial, MaterialDetails tMaterialDetails)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInMaterials.Add(tMaterial);
        foreach (Renderer renderer in tMaterialDetails.FoundInRenderers)
        {
            if (!tTextureDetails.FoundInRenderers.Contains(renderer)) tTextureDetails.FoundInRenderers.Add(renderer);
        }
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Renderer renderer)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInRenderers.Add(renderer);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Animator animator)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInAnimators.Add(animator);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, Graphic graphic)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInGraphics.Add(graphic);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture, MonoBehaviour script)
    {
        TextureDetails tTextureDetails = GetTextureDetail(tTexture);

        tTextureDetails.FoundInScripts.Add(script);
        return tTextureDetails;
    }

    private TextureDetails GetTextureDetail(Texture tTexture)
    {
        TextureDetails tTextureDetails = FindTextureDetails(tTexture);
        if (tTextureDetails == null)
        {
            tTextureDetails = new TextureDetails();
            tTextureDetails.texture = tTexture;
            tTextureDetails.isCubeMap = tTexture is Cubemap;

            int memSize = CalculateTextureSizeBytes(tTexture);

            TextureFormat tFormat = TextureFormat.RGBA32;
            int tMipMapCount = 1;
            if (tTexture is Texture2D)
            {
                tFormat = (tTexture as Texture2D).format;
                tMipMapCount = (tTexture as Texture2D).mipmapCount;
            }
            if (tTexture is Cubemap)
            {
                tFormat = (tTexture as Cubemap).format;
                memSize = 8 * tTexture.height * tTexture.width;
            }
            if (tTexture is Texture2DArray)
            {
                tFormat = (tTexture as Texture2DArray).format;
                tMipMapCount = 10;
            }

            tTextureDetails.memSizeKB = memSize / 1024;
            tTextureDetails.format = tFormat;
            tTextureDetails.mipMapCount = tMipMapCount;

        }

        return tTextureDetails;
    }
    */

    #region memory_profiler
    private HDMemoryProfiler mMPMemoryProfiler;
    private HDMemoryProfiler MP_MemoryProfiler
    {
        get
        {
            if (mMPMemoryProfiler == null)
            {
                mMPMemoryProfiler = new HDMemoryProfiler();
            }

            return mMPMemoryProfiler;
        }
    }

    private void MP_Clear()
    {
        MP_MemoryProfiler.Clear(true);
    }

    private AbstractMemorySample MP_TakeASampleFromScene(bool reuseAnalysis)
    {
        HDMemoryProfiler profiler = MP_MemoryProfiler;
        AbstractMemorySample sample = profiler.Scene_TakeAGameSampleWithCategories(reuseAnalysis, CategorySet_Current);
        return sample;
    }
    #endregion

    #region sample_panel
    private SampleWidget SamplePanelWidget { get; set; }

    private AbstractMemorySample SamplePanel_CurrentSample { get; set; }

    private void SamplePanel_Clear()
    {
        SamplePanelWidget.Clear();
        SamplePanel_CurrentSample = null;
    }

    private void SamplePanel_Show(AbstractMemorySample sample)
    {
        SamplePanel_CurrentSample = sample;

        if (SamplePanelWidget == null)
        {
            SamplePanelWidget = new SampleWidget();
        }

        AbstractMemorySample.ESizeStrategy sizeStrategy = MP_MemoryProfiler.SizeStrategy;
        List<long> samplesMax = new List<long>();
        MemoryProfiler.CategorySet category = MP_MemoryProfiler.CategorySet_Get(CategorySet_Current);
        long totalMemInBytes = (long)category.GetTotalMaxMemory(sizeStrategy);
        List<MemoryProfiler.CategoryConfig> categoryConfigs = category.CategoryConfigs;
        int count = categoryConfigs.Count;
        for (int i = 0; i < count; i++)
        {
            samplesMax.Add(categoryConfigs[i].GetMaxMemory(sizeStrategy));
        }

        List<MemorySample> samples = new List<MemorySample>();
        if (sample is MemorySample)
        {
            samples.Add(sample as MemorySample);
        }
        else if (sample is MemorySampleCollection)
        {
            Dictionary<string, AbstractMemorySample> collection = (sample as MemorySampleCollection).Samples;
            for (int j = 0; j < count; j++)
            {
                if (collection != null && collection.ContainsKey(categoryConfigs[j].Name))
                {
                    samples.Add(collection[categoryConfigs[j].Name] as MemorySample);
                }
                else
                {
                    MemorySample thisSample = new MemorySample(categoryConfigs[j].Name, sizeStrategy);
                    samples.Add(thisSample);
                }
            }
        }

        count = samples.Count;
        Dictionary<string, List<string>> groupTypes = SamplePanel_TypeGroups;
        MemorySample memorySample;
        for (int i = 0; i < count; i++)
        {
            memorySample = samples[i];
            memorySample.TypeGroups_Apply(groupTypes);
        }

        Dictionary<string, List<string>> viewGroupTypes = new Dictionary<string, List<string>>();
        foreach (KeyValuePair<string, List<string>> pair in groupTypes)
        {
            viewGroupTypes.Add(pair.Key, pair.Value);
        }
        viewGroupTypes.Add(MemorySample.TYPE_GROUPS_OTHER, null);

        SamplePanelWidget.Setup(sample.Name, viewGroupTypes, totalMemInBytes, samples, samplesMax);
    }

    private Dictionary<string, List<string>> SamplePanel_TypeGroups
    {
        get
        {
            return MP_MemoryProfiler.GameTypeGroups;
        }
    }

    private void SamplePanel_Refresh()
    {
        SamplePanel_CurrentSample = MP_TakeASampleFromScene(true);
        SamplePanel_Show(SamplePanel_CurrentSample);
    }
    #endregion

    #region categories    

    private string CategorySet_Current { get; set; }

    private List<string> CategorySet_Names { get; set; }

    private void CategorySet_Init()
    {
        if (CategorySet_Names == null)
        {
            CategorySet_Names = MP_MemoryProfiler.CategorySet_GetNames();


            // We set the default category
            CategorySet_Current = HDMemoryProfiler.CATEGORY_SET_NAME_EVERYTHING;
        }
    }

    private void CategorySet_OnValueChanged(int newValue)
    {
        if (CategorySet_Names != null && newValue > -1 && newValue < CategorySet_Names.Count)
        {
            CategorySet_Current = CategorySet_Names[newValue];

            // If a sample was taken then we need to take another one to make the UI show the latest information
            if (SamplePanel_CurrentSample != null)
            {
                SamplePanel_Refresh();
            }
        }
        else
        {
            Debug.LogError("Not valid index for category set: " + newValue);
        }
    }
    #endregion

    #region sizeStrategy
    private List<string> m_sizeStrategyOptions;

    private List<MemorySample.ESizeStrategy> m_sizeStrategyValues;
    private MemorySample.ESizeStrategy m_sizeStrategyCurrent;

    private void SizeStrategy_Init()
    {
        // Options for the prefab drop down have to be created        
        if (m_sizeStrategyOptions == null)
        {
            m_sizeStrategyOptions = new List<string>();
            m_sizeStrategyValues = new List<MemorySample.ESizeStrategy>();

            int count = 0;
            foreach (var value in System.Enum.GetValues(typeof(MemorySample.ESizeStrategy)))
            {
                m_sizeStrategyValues.Add((MemorySample.ESizeStrategy)value);
                m_sizeStrategyOptions.Add(m_sizeStrategyValues[count].ToString());
                count++;
            }

            // We set the default size strategy
            SizeStrategy_SetValue(MemorySample.ESizeStrategy.DeviceHalf);
        }
    }

    private void SizeStrategy_OnValueChanged(int newValue)
    {
        if (m_sizeStrategyValues != null && newValue > -1 && newValue < m_sizeStrategyValues.Count)
        {
            SizeStrategy_SetValue(m_sizeStrategyValues[newValue]);
        }
        else
        {
            Debug.LogError("Not valid index for size strategy: " + newValue);
        }
    }

    private void SizeStrategy_SetValue(MemorySample.ESizeStrategy value)
    {
        if (MP_MemoryProfiler.SizeStrategy != value)
        {
            MP_MemoryProfiler.SizeStrategy = value;

            if (SamplePanel_CurrentSample != null)
            {
                SamplePanel_CurrentSample.SizeStrategy = value;
                SamplePanel_Show(SamplePanel_CurrentSample);
            }
        }
    }
    #endregion

    #region file
    private static string FILE_PATH = "profiler/develop/";

    private void File_SaveSample()
    {
        string xml = SamplePanel_CurrentSample.ToXML(null, null, SamplePanel_TypeGroups).OuterXml;

        string path = EditorUtility.SaveFilePanel("Select a file containing a sample", FILE_PATH, "memorySample", "xml");
        if (path.Length != 0)
        {
            File.WriteAllText(path, xml);
        }
    }

    private void File_LoadSample()
    {
        string path = EditorUtility.SaveFilePanel("Select a file containing a sample", FILE_PATH, "memorySample", "xml");
        if (path.Length != 0)
        {
            //JSONNode node = JSONNode.LoadFromFile(path);
        }
    }
    #endregion
}

