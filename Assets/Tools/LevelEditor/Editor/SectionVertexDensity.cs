using System.Collections;
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
            public TransformNode(Transform tr)
            {
                m_Node = tr;
                if (tr != null)
                {
                    MeshFilter meshfilter = tr.GetComponent<MeshFilter>();
                    if (meshfilter != null)
                    {
                        Mesh mesh = meshfilter.sharedMesh;
                        if (mesh != null)
                        {
                            m_vertex = mesh.vertexCount;
                            m_polygons = mesh.triangles.Length / 3;
                        }
                    }
                }
                m_Childs = new List<TransformNode>();
            }

            public TransformNode(Transform tr, int vertex, int polygons)
            {
                m_Node = tr;
                m_vertex = vertex;
                m_polygons = polygons;
                m_Childs = new List<TransformNode>();
            }


            public bool checking;
            public int m_vertex;
            public int m_polygons;
            public Transform m_Node;
            public List<TransformNode> m_Childs;
        };

        List<TransformNode> m_nodeList = new List<TransformNode>();
        int m_totalVertex, m_totalPolygons;
        int m_hierarchyLevel;
        NodeDensity[] m_nodeDensity = null;

        public struct NodeDensity
        {
            public TransformNode node;
            public int numvertex;
            public int numpolygon;
        }

        void checkGameObjectHierarchy(TransformNode root, GameObject go)
        {
            MeshFilter mFilter = go.GetComponent<MeshFilter>();
            if (mFilter != null)
            {
                Mesh mesh = mFilter.sharedMesh;
                if (mesh != null)
                {
                    TransformNode node = new TransformNode(go.transform, mesh.vertexCount, mesh.triangles.Length / 3);
                    root.m_Childs.Add(node);
                    root = node;
                    m_nodeList.Add(node);
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
            TransformNode root = new TransformNode(null);
            m_nodeList.Add(root);

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (s.isLoaded)
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
            foreach(TransformNode tn in m_nodeList)
            {
                m_totalVertex += tn.m_vertex;
                m_totalPolygons += tn.m_polygons;
            }

            Debug.Log("Total transform nodes with mesh filter: " + m_nodeList.Count);
            Debug.Log("Total vertex in scene: " + m_totalVertex);
            Debug.Log("Total polygon in scene: " + m_totalPolygons);

            m_hierarchyLevel = 1;
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


        //--------------------------------------------------------------------//
        // INTERFACE IMPLEMENTATION											  //
        //--------------------------------------------------------------------//
        /// <summary>
        /// Initialize this section.
        /// </summary>
        public void Init()
        {
        }

        Vector2 scrollPos = Vector2.zero;

        /// <summary>
        /// Draw the section.
        /// </summary>
        /// 
        public void OnGUI()
        {
            // Title - encapsulate in a nice button to make it foldable
            GUI.backgroundColor = Colors.gray;
            bool folded = Prefs.GetBoolEditor("LevelEditor.SectionVertexDensity.folded", false);
            if (GUILayout.Button((folded ? "►" : "▼") + " Vertex density growth", LevelEditorWindow.styles.sectionHeaderStyle, GUILayout.ExpandWidth(true)))
            {
                folded = !folded;
                Prefs.SetBoolEditor("LevelEditor.SectionVertexDensity.folded", folded);
            }
            GUI.backgroundColor = Colors.white;

            // -Only show if unfolded
            if (!folded)
            {
                GUI.backgroundColor = Colors.paleGreen;
                if (GUILayout.Button("Gather scene info"))
                {
                    gatherHierarchyTree();
                    m_nodeDensity = getVertexDensityAtLevel(m_hierarchyLevel);
                }

                if (m_nodeList.Count > 0)
                {
                    
                    GUILayout.Label("Total transform nodes with mesh filter: " + m_nodeList.Count);
                    GUILayout.Label("Total vertex in scene: " + m_totalVertex);
                    GUILayout.Label("Total polygon in scene: " + m_totalPolygons);
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
                        scrollPos = GUILayout.BeginScrollView(scrollPos);
                        int nodeCount = m_nodeDensity.Length > 100 ? 100 : m_nodeDensity.Length;
                        EditorGUILayout.BeginVertical();
                        for (int c = 0; c < nodeCount; c++)
                        {
                            if (m_nodeDensity[c].node.m_Node != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                if(GUILayout.Button(m_nodeDensity[c].node.m_Node.gameObject.name))
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
            }
        }
    }
}