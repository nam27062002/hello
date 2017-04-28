using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class MemoryProfiler
{          
    public void Clear()
    {       
        if (All_MemorySample != null)
        {
            All_MemorySample.Clear();
        }

        Scene_Clear();
        GO_Clear();
    }    

    #region size
    private MemorySample.ESizeStrategy mSizeStrategy = MemorySample.ESizeStrategy.Profiler;
    public MemorySample.ESizeStrategy SizeStrategy
    {
        get
        {
            return mSizeStrategy;
        }

        set
        {
            mSizeStrategy = value;
        }
    }
    #endregion

    #region all
    /// <summary>
    /// Sample containing information about all object currently in scene
    /// </summary>
    private MemorySample All_MemorySample { get; set; }
    
    public MemorySample All_TakeASample()
    {
        if (All_MemorySample == null)
        {
            All_MemorySample = new MemorySample("Sample per type", SizeStrategy);
        }
        else
        {
            All_MemorySample.Clear();
        }

        HideFlags hideFlagMask = HideFlags.HideInInspector | HideFlags.HideAndDontSave;
        HideFlags hideFlagMask1 = HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.DontUnloadUnusedAsset;

        // Gets all objects in memory
        Object[] all = Resources.FindObjectsOfTypeAll<Object>();
        Object o;
        int count = all.Length;
        for (int i = 0; i < count; i++)
        {
            o = all[i];

            // Discard the ones that don't belong to the scene (internal editor stuff)
            if (o.hideFlags == HideFlags.HideAndDontSave || o.hideFlags == hideFlagMask || o.hideFlags == hideFlagMask1)
                continue;

            All_MemorySample.AddObject(o);            
        }

        All_MemorySample.Analyze();

        return All_MemorySample;
    }
    #endregion

    #region scene
    // This region is responsible for storing the details of the memory used by the scene. They are classified by category (dragon, npcs,...). There's a 
    // sample by category    

    private const string SCENE_SAMPLE_NAME = "Scene";

    protected Dictionary<string, List<GameObject>> Scene_GOs { get; set; }

    private void Scene_Clear()
    {       
        if (Scene_GOs != null)
        {
            Scene_GOs.Clear();
        }
    }

    public void Scene_AddGO(string key, GameObject go)
    {
        if (go != null)
        {
            if (Scene_GOs == null)
            {
                Scene_GOs = new Dictionary<string, List<GameObject>>();
            }

            if (!Scene_GOs.ContainsKey(key))
            {
                Scene_GOs.Add(key, new List<GameObject>());
            }

            if (!Scene_GOs[key].Contains(go))
            {
                Scene_GOs[key].Add(go);
            }
        }
    }   

    /// <summary>
    /// Takes a single sample with all Objects in the scene 
    /// </summary>
    public virtual AbstractMemorySample Scene_TakeASample()
    {
        MemorySample returnValue = new MemorySample(SCENE_SAMPLE_NAME, SizeStrategy);

        if (Scene_GOs != null)
        {
            int count;            
            List<Object> dependencies = new List<Object>();
            foreach (KeyValuePair<string, List<GameObject>> pair in Scene_GOs)
            {                                
                count = pair.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    GO_AnalyzeGO(pair.Value[i], ref dependencies);
                }
            }

            count = dependencies.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue.AddObject(dependencies[i]);
            }

            returnValue.Analyze();
        }

        return returnValue;
    }

    /// <summary>
    /// Takes a sample of every category
    /// </summary>
    public virtual AbstractMemorySample Scene_TakeASampleWithCategories(string categorySetName)
    {
        MemorySampleCollection returnValue = new MemorySampleCollection(SCENE_SAMPLE_NAME, SizeStrategy);

        if (Scene_GOs != null)
        {
            int count;
            MemorySample sample;
            List<Object> dependencies = new List<Object>();
            foreach (KeyValuePair<string, List<GameObject>> pair in Scene_GOs)
            {
                dependencies.Clear();

                sample = new MemorySample(pair.Key, SizeStrategy);
                returnValue.AddSample(pair.Key, sample);
                count = pair.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    GO_AnalyzeGO(pair.Value[i], ref dependencies);
                }

                count = dependencies.Count;
                for (int i = 0; i < count; i++)
                {
                    sample.AddObject(dependencies[i]);
                }

                sample.Analyze();
            }
        }

        return returnValue;
    }
    #endregion

    #region go
    private const BindingFlags GO_BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// GOS to ignore when anlyzing. It's used to prevent the assets of some game objects from taking into consideration.
    /// </summary>
    private List<string> GO_BannedGOs;

    private void GO_Clear()
    {
        if (GO_BannedGOs != null)
        {
            GO_BannedGOs.Clear();
        }
    }

    public void GO_BanGOByName(string name)
    {
        if (GO_BannedGOs == null)
        {
            GO_BannedGOs = new List<string>();
        }

        if (!GO_BannedGOs.Contains(name))
        {
            GO_BannedGOs.Add(name);
        }
    }

    /// <summary>
    /// Returns the details of the memory used by a GameObject
    /// </summary>
    /// <returns></returns>
    public MemorySample GO_TakeASample(GameObject go, string name=null, MemorySample.ESizeStrategy sizeStrategy=MemorySample.ESizeStrategy.Profiler)
    {
        if (name == null)
        {
            name = go.name;
        }

        MemorySample returnValue = new MemorySample(name, sizeStrategy);               
        List<Object> dependencies = new List<Object>();        
        GO_AnalyzeGO(go, ref dependencies);

        int count = dependencies.Count;
        for (int i = 0; i < count; i++)
        {
            returnValue.AddObject(dependencies[i]);
        }        

        returnValue.Analyze();

        return returnValue;
    }

    private void GO_AddElement(Object o, ref List<Object> list, bool checkIfExists)
    {
        if (o != null && 
            (!checkIfExists || !list.Contains(o)))
        {
            list.Add(o);           

            if (o is Animator)
            {
                GO_AddAnimator(o as Animator, ref list);
            }
            else if (o is Material)
            {
                Material material = (o as Material);
                if (material != null)
                {                        
#if UNITY_EDITOR
                    GO_AddTexturesFromMaterialInEditor(material, ref list);
#else
                    AnalyzeTextureFromShadersSettings(go, ref info);        
#endif                    
                }
            }
            else if (o is Sprite)
            {
                Sprite sprite = o as Sprite;
                if (sprite != null && sprite.texture != null)
                {
                    GO_AddElement(sprite.texture, ref list, true);
                }
            }
        }
    }

    private void GO_AnalyzeGO(GameObject go, ref List<Object> list)
    {
        // Makes sure it hasn't already been analyzed
        if (go != null && !list.Contains(go))
        {
            if (GO_BannedGOs != null && GO_BannedGOs.Contains(go.name))
                return;
            
            GO_AddElement(go, ref list, false);

            // Loops through all components of this game object
            foreach (var component in go.GetComponents<Component>())
            {
                GO_AnalyzeComponent(component, ref list);
            }

            // Loops through all children of this game objectq
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                GO_AnalyzeGO(t.GetChild(i).gameObject, ref list);
            }
        }
    }

    private void GO_AnalyzeComponent(Component c, ref List<Object> list)
    {
        // Makes sure this component hasn't already been analyzed
        if (c != null && !list.Contains(c))
        {
            GO_AddElement(c, ref list, false);

            // Loops through all fields of this component            
            FieldInfo[] fields = c.GetType().GetFields(GO_BINDING_FLAGS);
            foreach (FieldInfo field in fields)
            {
                GO_AnalyzeField(c, field, ref list);
            }

            PropertyInfo[] properties = c.GetType().GetProperties(GO_BINDING_FLAGS);
            foreach (PropertyInfo property in properties)
            {
                GO_AnalyzeProperty(c, property, ref list);
            }
        }
    }

    private void GO_AnalyzeField(object o, FieldInfo field, ref List<Object> list)
    {
        Type fieldType = field.FieldType;
        object value = field.GetValue(o);
        if (value != null)
        {
            // Check if it's a collection
            if (GO_IsMemberACollection(fieldType))
            {                
                IEnumerable fieldList = (IEnumerable)field.GetValue(o);
                if (fieldList != null)
                {
                    // If has to loop through all fields of the collection
                    foreach (var item in fieldList)
                    {
                        FieldInfo[] fields = item.GetType().GetFields(GO_BINDING_FLAGS);
                        if (fields != null)
                        {
                            int fieldsCount = fields.Length;
                            for (int j = 0; j < fieldsCount; j++)
                            {
                                GO_AnalyzeField(item, fields[j], ref list);
                            }
                        }
                    }
                }
            }
            // Checks if it's a custom class field
            else if (fieldType.IsSerializable && !fieldType.IsPrimitive)
            {
                FieldInfo[] fields = fieldType.GetFields(GO_BINDING_FLAGS);
                if (fields != null)
                {
                    // Loos through all fields of the custom class                    
                    int count = fields.Length;
                    for (int i = 0; i < count; i++)
                    {
                        GO_AnalyzeField(value, fields[i], ref list);
                    }
                }
            }
            else
            {
                GO_AnalyzeMember(value, ref list);
            }          
        }
    }

    private void GO_AnalyzeProperty(object o, PropertyInfo property, ref List<Object> list)
    {
        if (property.PropertyType.IsSubclassOf(typeof(UnityEngine.Object)) &&
            !property.IsDefined(typeof(ObsoleteAttribute), true) &&         // Accessing properties marked as obsolete by Unity triggers an exception
             property.Name != "material" && property.Name != "materials" && // Accessing material or materials properties triggers an exception                    
             property.Name != "mesh")                                       // Accessing mesh triggers an exception                    
        { 
            Type propertyType = property.PropertyType;
            object value = property.GetValue(o, null);
            if (value != null)
            {
                if (GO_IsMemberACollection(propertyType))
                {
                    IEnumerable propertyList = (IEnumerable)value;
                    if (propertyList != null)
                    {
                        foreach (var item in propertyList)
                        {
                            PropertyInfo[]properties = item.GetType().GetProperties(GO_BINDING_FLAGS);
                            if (properties != null)
                            {
                                int fieldsCount = properties.Length;
                                for (int j = 0; j < fieldsCount; j++)
                                {
                                    GO_AnalyzeProperty(item, properties[j], ref list);
                                }
                            }
                        }
                    }
                }
                else if (propertyType.IsSerializable && !propertyType.IsPrimitive)
                {
                    PropertyInfo[] properties = propertyType.GetProperties(GO_BINDING_FLAGS);
                    if (properties != null)
                    {                        
                        int count = properties.Length;
                        for (int i = 0; i < count; i++)
                        {
                            GO_AnalyzeProperty(value, properties[i], ref list);
                        }
                    }
                }
                else 
                {
                    GO_AnalyzeMember(value, ref list);                    
                }
            }
        }
    }

    private void GO_AnalyzeMember(object value, ref List<Object> list)
    {
        if (value is GameObject || value is Component)
        {
            GameObject go = null;
            if (value is Component)
            {
                Component component = value as Component;
                if (component != null)
                {
                    go = component.gameObject;
                }
            }
            else
            {
                go = value as GameObject;
            }

            if (go != null && GO_IsAPrefab(go))
            {
                GO_AnalyzeGO(go, ref list);
            }
        }
        else if (value is Object)
        {
            GO_AddElement(value as Object, ref list, true);
        }
    }   
   
    private bool GO_IsAPrefab(GameObject go)
    {        
#if UNITY_EDITOR
        PrefabType prefabType = PrefabUtility.GetPrefabType(go);
        return prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab;
#else
        return string.IsNullOrEmpty(go.scene.name);
#endif
    }

    public bool GO_IsMemberACollection(Type type)
    {        
        return type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>));       
    }

    private void GO_AddAnimator(Animator animator, ref List<Object> list)
    {                
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip anim in animator.runtimeAnimatorController.animationClips)
            {
                GO_AddElement(anim, ref list, true);
            }
        }                
    }

    private void GO_AddTexturesFromMaterialInEditor(Material material, ref List<Object> list)
    {
        if (material != null)
        {
            Shader shader = material.shader;

            if (shader != null)
            {
                GO_AddElement(shader, ref list, true);

                string subtype;
                Texture texture;
                int propertiesCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertiesCount; i++)
                {
                    if (UnityEditor.ShaderUtil.GetPropertyType(shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        subtype = UnityEditor.ShaderUtil.GetPropertyName(shader, i);
                        texture = material.GetTexture(subtype);
                        if (texture != null)
                        {                           
                            GO_AddElement(texture, ref list, true);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region category
    public class CategoryConfig
    {
        public string Name { get; set; }
        public float MaxMemory { get; set; }

        public CategoryConfig(string name, float maxMemory)
        {
            Name = name;
            MaxMemory = maxMemory;
        }
    }

    public class CategorySet
    {
        public string Name { get; set; }
        public List<CategoryConfig> CategoryConfigs { get; set; }

        public void AddCategory(string name, float maxMemory)
        {
            if (CategoryConfigs == null)
            {
                CategoryConfigs = new List<CategoryConfig>();
            }

            CategoryConfig categoryConfig = new CategoryConfig(name, maxMemory);
            CategoryConfigs.Add(categoryConfig);
        }

        public float TotalMaxMemory {
            get {
                float returnValue = 0;
                if (CategoryConfigs != null) {
                    int count = CategoryConfigs.Count;
                    for (int i = 0; i < count; i++) {
                        returnValue += CategoryConfigs[i].MaxMemory;
                    }
                }

                return returnValue;
            }
        }
    }

    /// <summary>
    /// Dictionary containing all category sets that are setup
    /// </summary>
    private Dictionary<string, CategorySet> CategorySet_Catalog;

    public void CategorySet_AddToCatalog(CategorySet value)
    {
        if (CategorySet_Catalog == null)
        {
            CategorySet_Catalog = new Dictionary<string, CategorySet>();
        }

        if (!CategorySet_Catalog.ContainsValue(value))
        {
            CategorySet_Catalog.Add(value.Name, value);
        }
    }

    public CategorySet CategorySet_Get(string categorySetName)
    {
        CategorySet returnValue = null;
        if (CategorySet_Catalog != null && CategorySet_Catalog.ContainsKey(categorySetName))
        {
            returnValue = CategorySet_Catalog[categorySetName];
        }

        return returnValue;
    }

    public List<string> CategorySet_GetNames()
    {
        List<string> returnValue = new List<string>();
        if (CategorySet_Catalog != null)
        {
            foreach (KeyValuePair<string, CategorySet> pair in CategorySet_Catalog)
            {
                returnValue.Add(pair.Key);
            }
        }

        return returnValue;
    }
    #endregion
}
