﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace LevelEditor
{
    public class SectionVertexDensity : ILevelEditorSection
    {

        public class TransformNode
        {
            public TransformNode(GameObject go)
            {
                if (go != null)
                {
                    m_Node = go.transform;
                    m_meshFilter = go.GetComponent<MeshFilter>();
                    if (m_meshFilter != null)
                    {
                        m_mesh = m_meshFilter.sharedMesh;
                    }
                    m_renderer = go.GetComponent<Renderer>();
                    m_smRenderer = go.GetComponent<SkinnedMeshRenderer>();
                    if (m_smRenderer != null && m_mesh == null)
                    {
                        m_mesh = m_smRenderer.sharedMesh;
                    }
                    if (m_mesh != null)
                    {
                        m_vertex = m_mesh.vertexCount;
                        m_polygons = m_mesh.triangles.Length / 3;
                    }
                    m_pSystem = go.GetComponent<ParticleSystem>();
                    m_staticFlags = GameObjectUtility.GetStaticEditorFlags(go);
                    m_scale = go.transform.lossyScale.magnitude * ((m_mesh != null) ? m_mesh.bounds.size.magnitude: 1.0f);
                }
                else
                {
                    m_Node = null;
                    m_meshFilter = null;
                    m_mesh = null;
                    m_renderer = null;
                    m_smRenderer = null;
                    m_pSystem = null;
                    m_vertex = 0;
                    m_polygons = 0;
                    m_scale = 1.0f;
                    m_staticFlags = 0;
                }
                m_Childs = new List<TransformNode>();
            }
            /*
                        public TransformNode(Transform tr, int vertex, int polygons)
                        {
                            m_Node = tr;
                            m_vertex = vertex;
                            m_polygons = polygons;
                            m_Childs = new List<TransformNode>();
                        }
            */
            public StaticEditorFlags m_staticFlags;
            public MeshFilter m_meshFilter;
            public Mesh m_mesh;
            public Renderer m_renderer;
            public SkinnedMeshRenderer m_smRenderer;
            public ParticleSystem m_pSystem;
            public int m_vertex;
            public int m_polygons;
            public float m_scale;
            public Transform m_Node;
            public List<TransformNode> m_Childs;
        };

        public List<TransformNode> m_nodeList = new List<TransformNode>();
        int m_totalRenderers, m_totalOptimizedRenderers;
//        int m_lightmapStaticRenderers, m_batchingStaticRenderers;
        int m_totalVertex, m_totalPolygons;
        public int m_hierarchyLevel;
        NodeDensity[] m_nodeDensity = null;

        List<GameObject> m_missingRenderers = new List<GameObject>();
        List<GameObject> m_batchingStaticRenderers = new List<GameObject>();
        List<GameObject> m_lightmapStaticRenderers = new List<GameObject>();
        List<TransformNode> m_minimumScaleNodes = new List<TransformNode>();

        public struct NodeDensity
        {
            public TransformNode node;
            public int numvertex;
            public int numpolygon;
        }

        void checkGameObjectHierarchy(TransformNode root, GameObject go)
        {
            TransformNode currentNode = new TransformNode(go);

            Mesh mesh = currentNode.m_mesh;
            if (mesh != null)
            {
                root.m_Childs.Add(currentNode);
                root = currentNode;
                m_nodeList.Add(currentNode);
            }


            Renderer rend = currentNode.m_renderer;
            if (rend != null && (rend.sharedMaterial == null || mesh == null))
            {
                ParticleSystem ps = currentNode.m_pSystem;
                if (ps == null) //not a particle system
                {
                    m_missingRenderers.Add(rend.gameObject);
                }
            }

            foreach (Transform tr in go.transform)
            {
                checkGameObjectHierarchy(root, tr.gameObject);
            }
        }

        void gatherHierarchyTree()
        {
            m_nodeList.Clear();
            m_missingRenderers.Clear();
            TransformNode root = new TransformNode(null);
            m_nodeList.Add(root);

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (s.isLoaded && !s.name.Contains("SC_LevelEditor"))
                {
                    GameObject[] allGameObjects = s.GetRootGameObjects();
                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        var go = allGameObjects[j];
                        checkGameObjectHierarchy(root, go);
                    }
                }
            }

            m_totalVertex = 0;
            m_totalPolygons = 0;
            m_totalRenderers = 0;

            foreach(TransformNode tn in m_nodeList)
            {
                m_totalVertex += tn.m_vertex;
                m_totalPolygons += tn.m_polygons;
                if (tn.m_renderer != null)
                    m_totalRenderers++;
            }

            optimizeRenderers(false);

            checkStaticRenderers();

            m_minimumScaleNodes = new List<TransformNode>(m_nodeList);

            for (int a = 0; a < m_minimumScaleNodes.Count - 1; a++)
            {
                for (int b = a + 1; b < m_minimumScaleNodes.Count; b++)
                {
                    if (m_minimumScaleNodes[a].m_scale > m_minimumScaleNodes[b].m_scale)
                    {
                        TransformNode tem = m_minimumScaleNodes[a];
                        m_minimumScaleNodes[a] = m_minimumScaleNodes[b];
                        m_minimumScaleNodes[b] = tem;
                    }
                }
            }
/*
            Debug.Log("Total transform nodes with mesh filter: " + m_nodeList.Count);
            Debug.Log("Total vertex in scene: " + m_totalVertex);
            Debug.Log("Total polygon in scene: " + m_totalPolygons);
*/
            m_hierarchyLevel = 1;
        }

        void checkStaticRenderers()
        {
            m_lightmapStaticRenderers.Clear();
            m_batchingStaticRenderers.Clear();

            foreach (TransformNode tn in m_nodeList)
            {
                if ((tn.m_staticFlags & StaticEditorFlags.LightmapStatic) != 0)
                    m_lightmapStaticRenderers.Add(tn.m_Node.gameObject);

                if ((tn.m_staticFlags & StaticEditorFlags.BatchingStatic) != 0)
                    m_batchingStaticRenderers.Add(tn.m_Node.gameObject);
            }
        }

        void optimizeRenderers(bool changeProperty = true)
        {
            m_totalOptimizedRenderers = 0;
            for (int c = 0; c < m_nodeList.Count; c++)
            {
                TransformNode node = m_nodeList[c];
                if (node.m_Node != null)
                {
                    Renderer rend = node.m_renderer;
                    if (rend != null)
                    {
                        SerializedObject so = new SerializedObject(rend);
                        so.Update();

                        SerializedProperty sp;

                        bool modified = false;

                        sp = so.FindProperty("m_DynamicOccludee");
                        if (sp.boolValue)
                        {
                            if (changeProperty)
                                sp.boolValue = false;
                            modified = true;
                        }

                        sp = so.FindProperty("m_MotionVectors");
                        if (sp.enumValueIndex != (int)MotionVectorGenerationMode.ForceNoMotion)
                        {
                            if (changeProperty)
                                sp.enumValueIndex = (int)MotionVectorGenerationMode.ForceNoMotion;
                            modified = true;
                        }

                        sp = so.FindProperty("m_LightProbeUsage");
                        if (sp.intValue != (int)UnityEngine.Rendering.LightProbeUsage.Off)
                        {
                            if (changeProperty)
                                sp.intValue = (int)UnityEngine.Rendering.LightProbeUsage.Off;
                            modified = true;
                        }

                        sp = so.FindProperty("m_ReflectionProbeUsage");
                        if (sp.intValue != (int)UnityEngine.Rendering.ReflectionProbeUsage.Off)
                        {
                            if (changeProperty)
                                sp.intValue = (int)UnityEngine.Rendering.ReflectionProbeUsage.Off;
                            modified = true;
                        }

                        if (modified)
                        {
                            if (changeProperty)
                            {
                                so.ApplyModifiedProperties();
                                EditorSceneManager.MarkSceneDirty(node.m_Node.gameObject.scene);
                            }
                            m_totalOptimizedRenderers++;
                        }
                    }
                }
            }

            Debug.Log("Changing " + m_totalOptimizedRenderers + " objects");
        }

        void setStaticRenderers(bool lightmap, bool value)
        {
            StaticEditorFlags sf = lightmap ? StaticEditorFlags.LightmapStatic : StaticEditorFlags.BatchingStatic;
            int modifiedObjects = 0;

            for (int c = 0; c < m_nodeList.Count; c++)
            {
                TransformNode node = m_nodeList[c];
                if (node.m_Node != null)
                {
                    Renderer rend = node.m_renderer;
                    if (rend != null)
                    {
                        bool modified = false;
                        StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(node.m_Node.gameObject);

                        if (value)
                        {
                            if (((int)staticFlags & (int)sf) == 0)
                            {
                                staticFlags |= sf;
                                modified = true;
                            }
                        }
                        else
                        {
                            if (((int)staticFlags & (int)sf) != 0)
                            {
                                staticFlags &= ~sf;
                                modified = true;
                            }
                        }

                        if (modified)
                        {
                            GameObjectUtility.SetStaticEditorFlags(node.m_Node.gameObject, staticFlags);
                            node.m_staticFlags = staticFlags;
                            EditorSceneManager.MarkSceneDirty(node.m_Node.gameObject.scene);
                            modifiedObjects++;
                        }
                    }
                }
            }

            Debug.Log("Changing " + modifiedObjects + " objects");

            checkStaticRenderers();
        }

        int getNodeVertexDensityAtLevel(TransformNode node, int currentLevel, int level, List<NodeDensity> results)
        {
            if (currentLevel < level)
            {
                if (node.m_Childs.Count == 0) return 0;

                foreach(TransformNode childNode in node.m_Childs)
                {
                    getNodeVertexDensityAtLevel(childNode, currentLevel + 1, level, results);
                }
            }
            else if (currentLevel == level)
            {
                NodeDensity nd = new NodeDensity();

                nd.node = node;
                if (node.m_Childs.Count == 0)
                {
                    nd.numvertex = node.m_vertex;
                }
                else
                {
                    int count = 0;
                    foreach (TransformNode childNode in node.m_Childs)
                    {
                        count += getNodeVertexDensityAtLevel(childNode, currentLevel + 1, level, results);
                    }
                    nd.numvertex = count;
                }
                results.Add(nd);
            }
            else
            {
                if (node.m_Childs.Count == 0)
                {
                    return node.m_vertex;
                }
                else
                {
                    int count = 0;
                    foreach (TransformNode childNode in node.m_Childs)
                    {
                        count += getNodeVertexDensityAtLevel(childNode, currentLevel + 1, level, results);
                    }
                    return count;
                }
            }
            return 0;
        }


        NodeDensity[] getVertexDensityAtLevel(int level)
        {
            List<NodeDensity> result = new List<NodeDensity>();

            getNodeVertexDensityAtLevel(m_nodeList[0], 0, level, result);

            for (int a = 0; a < result.Count - 1; a++)
            {
                for (int b = a + 1; b < result.Count; b++)
                {
                    if (result[a].numvertex < result[b].numvertex)
                    {
                        NodeDensity temp = result[a];
                        result[a] = result[b];
                        result[b] = temp;
                    }
                }
            }

            return result.ToArray();
        }

        private int m_currentTab;

        public delegate void TabAction(EditorTabWidget tabWidget);

        public class EditorTabWidget
        {
            public EditorTabWidget(string tabTitle, TabAction tabAction)
            {
                m_tabTitle = tabTitle;
                m_tabAction = tabAction;
                m_scrollPos = Vector2.zero;
            }
            public string m_tabTitle;
            public TabAction m_tabAction;
            public Vector2 m_scrollPos;
        }

        private List<EditorTabWidget> m_tabWidgets = new List<EditorTabWidget>();

        public void nodeListDensity(EditorTabWidget tabWidget)
        {

            GUI.backgroundColor = Colors.paleYellow;

            EditorGUI.BeginChangeCheck();
            m_hierarchyLevel = EditorGUILayout.IntField("Hierarchy level: ", m_hierarchyLevel);
            if (EditorGUI.EndChangeCheck())
            {
                m_nodeDensity = getVertexDensityAtLevel(m_hierarchyLevel);
            }
            if (m_nodeDensity != null)
            {
                GUILayout.Label(m_nodeDensity.Length.ToString() + " nodes at level " + m_hierarchyLevel);
                EditorGUILayout.Separator();
                tabWidget.m_scrollPos = GUILayout.BeginScrollView(tabWidget.m_scrollPos);
                int nodeCount = m_nodeDensity.Length > 100 ? 100 : m_nodeDensity.Length;
                EditorGUILayout.BeginVertical();
                for (int c = 0; c < nodeCount; c++)
                {
                    if (m_nodeDensity[c].node.m_Node != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(m_nodeDensity[c].node.m_Node.gameObject.name, GUILayout.Width(250.0f)))
                        {
                            Selection.activeGameObject = m_nodeDensity[c].node.m_Node.gameObject;
                        }
                        GUILayout.Label(" vertex: " + m_nodeDensity[c].numvertex);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }

        public void missingRenderers(EditorTabWidget tabWidget)
        {
            GUI.backgroundColor = Colors.paleYellow;
            EditorGUILayout.BeginVertical();
            if (m_missingRenderers.Count > 0)
            {
                GUILayout.Label("Missing renderers: " + m_missingRenderers.Count);
                if (GUILayout.Button("Remove missing renderers"))
                {
                    for (int c = 0; c < m_missingRenderers.Count; c++)
                    {
                        if (m_missingRenderers[c].transform.childCount > 0)
                        {
                            Debug.Log("GameObject: " + m_missingRenderers[c].name + " child count: " + m_missingRenderers[c].transform.childCount);
                        }
                        else
                        {
                            EditorSceneManager.MarkSceneDirty(m_missingRenderers[c].scene);
                            Object.DestroyImmediate(m_missingRenderers[c]);
                        }
                    }

                    m_missingRenderers.Clear();
                }

                EditorGUILayout.Separator();
                tabWidget.m_scrollPos = GUILayout.BeginScrollView(tabWidget.m_scrollPos);

                int nodeCount = (m_missingRenderers.Count > 100) ? 100 : m_missingRenderers.Count;

                for (int c = 0; c < nodeCount; c++)
                {
                    if (GUILayout.Button(m_missingRenderers[c].name))
                    {
                        Selection.activeGameObject = m_missingRenderers[c];
                    }
                }

                GUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

        }

        public void batchingStaticRenderers(EditorTabWidget tabWidget)
        {
            GUI.backgroundColor = Colors.paleYellow;
            EditorGUILayout.BeginVertical();
            if (m_batchingStaticRenderers.Count > 0)
            {
                tabWidget.m_scrollPos = GUILayout.BeginScrollView(tabWidget.m_scrollPos);

                int nodeCount = (m_batchingStaticRenderers.Count > 100) ? 100 : m_batchingStaticRenderers.Count;

                for (int c = 0; c < nodeCount; c++)
                {
                    if (GUILayout.Button(m_batchingStaticRenderers[c].name))
                    {
                        Selection.activeGameObject = m_batchingStaticRenderers[c];
                    }
                }

                GUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }

        public void lightmapStaticRenderers(EditorTabWidget tabWidget)
        {
            GUI.backgroundColor = Colors.paleYellow;
            EditorGUILayout.BeginVertical();
            if (m_lightmapStaticRenderers.Count > 0)
            {
                tabWidget.m_scrollPos = GUILayout.BeginScrollView(tabWidget.m_scrollPos);

                int nodeCount = (m_lightmapStaticRenderers.Count > 100) ? 100 : m_lightmapStaticRenderers.Count;

                for (int c = 0; c < nodeCount; c++)
                {
                    if (GUILayout.Button(m_lightmapStaticRenderers[c].name))
                    {
                        Selection.activeGameObject = m_lightmapStaticRenderers[c];
                    }
                }

                GUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

        }

        public void minimumScaleObjects(EditorTabWidget tabWidget)
        {
            GUI.backgroundColor = Colors.paleYellow;
            EditorGUILayout.BeginVertical();
            if (m_minimumScaleNodes.Count > 0)
            {
                tabWidget.m_scrollPos = GUILayout.BeginScrollView(tabWidget.m_scrollPos);

                int nodeCount = (m_minimumScaleNodes.Count > 100) ? 100 : m_minimumScaleNodes.Count;

                for (int c = 0; c < nodeCount; c++)
                {
                    if (m_minimumScaleNodes[c].m_Node != null && GUILayout.Button(m_minimumScaleNodes[c].m_Node.gameObject.name))
                    {
                        Selection.activeGameObject = m_minimumScaleNodes[c].m_Node.gameObject;
                    }
                }

                GUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

        }


        //--------------------------------------------------------------------//
        // INTERFACE IMPLEMENTATION											  //
        //--------------------------------------------------------------------//
        /// <summary>
        /// Initialize this section.
        /// </summary>
        public void Init()
        {
            m_tabWidgets.Clear();
            m_tabWidgets.Add(new EditorTabWidget("Node list density", nodeListDensity));
            m_tabWidgets.Add(new EditorTabWidget("Missing renderers", missingRenderers));
            m_tabWidgets.Add(new EditorTabWidget("Batching static renderers", batchingStaticRenderers));
            m_tabWidgets.Add(new EditorTabWidget("Lightmap static renderers", lightmapStaticRenderers));
            m_tabWidgets.Add(new EditorTabWidget("Minimum scale GameObject", minimumScaleObjects));

            m_currentTab = Prefs.GetIntEditor("LevelEditor.SectionVertexDensity.currentTab", 0);
        }

    Vector2 scrollPos = Vector2.zero;
        Vector2 scrollPos2 = Vector2.zero;
        Vector2 scrollPos3 = Vector2.zero;
        Vector2 scrollPos4 = Vector2.zero;



        /// <summary>
        /// Draw the section.
        /// </summary>
        /// 
        public void OnGUI()
        {
            // Title - encapsulate in a nice button to make it foldable
            GUI.backgroundColor = Colors.gray;

            bool folded = Prefs.GetBoolEditor("LevelEditor.SectionVertexDensity.folded", false);
            if (GUILayout.Button((folded ? "►" : "▼") + " Vertex density tool", LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true)))
            {
                folded = !folded;
                Prefs.SetBoolEditor("LevelEditor.SectionVertexDensity.folded", folded);
            }
            GUI.backgroundColor = Colors.white;

            // -Only show if unfolded
            if (!folded)
            {
                GUI.backgroundColor = Colors.orange;
                if (GUILayout.Button("Gather scene info"))
                {
                    gatherHierarchyTree();
                    m_nodeDensity = getVertexDensityAtLevel(m_hierarchyLevel);
                }

                if (m_nodeList.Count > 0)
                {
                    GUI.backgroundColor = Colors.lime;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Optimize renderers"))
                    {
                        optimizeRenderers();
                    }

                    bool setBatchingStatic = m_batchingStaticRenderers.Count < (m_totalRenderers / 2);
                    if (GUILayout.Button(setBatchingStatic ? "Set Batching static" : "Clear Batching static"))
                    {
                        setStaticRenderers(false, setBatchingStatic);
                    }
                    bool setLightmapStatic = m_lightmapStaticRenderers.Count < (m_totalRenderers / 2);
                    if (GUILayout.Button(setLightmapStatic ? "Set lightmap static" : "Clear lightmap static"))
                    {
                        setStaticRenderers(true, setLightmapStatic);
                    }
                    GUILayout.EndVertical();

                    GUILayout.Label("Total transform nodes with mesh: " + m_nodeList.Count);
                    GUILayout.Label("Total vertex in scene: " + m_totalVertex);
                    GUILayout.Label("Total polygon in scene: " + m_totalPolygons);
 //                   EditorGUILayout.Separator();
                    GUILayout.Label("Total renderers: " + m_totalRenderers);
                    GUILayout.Label("Optimizable renderers: " + m_totalOptimizedRenderers);
                    GUILayout.Label("Batching static renderers: " + m_batchingStaticRenderers.Count);
                    GUILayout.Label("Lightmap static renderers: " + m_lightmapStaticRenderers.Count);


                    GUILayout.BeginHorizontal();
                    for (int c = 0; c < m_tabWidgets.Count; c++)
                    {
                        GUI.backgroundColor = m_currentTab == c ? Colors.transparentWhite : Colors.gray;
                        if (GUILayout.Button(m_tabWidgets[c].m_tabTitle, LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true)))
                        {
                            m_currentTab = c;
                            Prefs.SetIntEditor("LevelEditor.SectionVertexDensity.currentTab", m_currentTab);
                        }

                    }
                    GUILayout.EndHorizontal();

                    EditorTabWidget tabWidget = m_tabWidgets[m_currentTab];
                    tabWidget.m_tabAction(tabWidget);
                }
            }
        }
    }
}

