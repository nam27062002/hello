using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public struct NodeDensity
        {
            public TransformNode node;
            public int numvertex;
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
        }


        int getNodeVertexDensityAtLevel(TransformNode node, int currentLevel, int level, List<int> results)
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


        /// <summary>
        /// Draw the section.
        /// </summary>
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
                }
            }
        }
    }
}