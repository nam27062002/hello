using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizeScene : MonoBehaviour {

    public void DoOptimize() {
        Queue<Transform> nodes = new Queue<Transform>();
        List<Transform> childs = new List<Transform>();

        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; ++i) {
            GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).GetRootGameObjects();
            foreach (GameObject go in roots) {
                nodes.Enqueue(go.transform);
            }
        }

        while(nodes.Count > 0) {
            Transform me = nodes.Dequeue();
            bool emptyNode = me.gameObject.GetComponents<Component>().Length == 1; //all nodes have a transform component

            //skip prefabs and objects with the level component
            if (!me.name.StartsWith("PF_", System.StringComparison.InvariantCulture)
            &&   me.gameObject.GetComponent<LevelEditor.Level>() == null) {
                int childCount = me.childCount;
                for (int i = 0; i < childCount; ++i) {
                    Transform child = me.GetChild(i);
                    childs.Add(child);
                    nodes.Enqueue(child);
                }

                if (emptyNode) {
                    foreach (Transform child in childs) {
                        child.SetParent(me.parent, true);
                    }
                }

                childs.Clear();

                if (emptyNode && me.childCount == 0) {
                    DestroyImmediate(me.gameObject);
                }
            }
        }
    }
}
