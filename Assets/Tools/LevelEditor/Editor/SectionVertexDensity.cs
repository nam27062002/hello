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
            }
        }
    }
}