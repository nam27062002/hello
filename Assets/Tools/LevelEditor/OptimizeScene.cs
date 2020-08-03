using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimizeScene : MonoBehaviour {

    public bool optimizeSpawner = true;

    public void DoOptimize() {
#if UNITY_EDITOR
        Queue<Transform> nodes = new Queue<Transform>();
        List<Transform> childs = new List<Transform>();

        
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; ++i) {
            List<AbstractSpawner> spawners = new List<AbstractSpawner>();
            List<AutoSpawnBehaviour> autoSpawners = new List<AutoSpawnBehaviour>();
            List<ActionPoint> actionPoints = new List<ActionPoint>();
            
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);

            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject go in roots) {
                SpawnerCollection spawnerCollection = go.GetComponent<SpawnerCollection>();
                if (spawnerCollection != null) {   
                    DestroyImmediate(spawnerCollection);
                }                
                nodes.Enqueue(go.transform);
            }

            while (nodes.Count > 0) {
                Transform me = nodes.Dequeue();                
                bool emptyNode = me.gameObject.GetComponents<Component>().Length == 1; //all nodes have a transform component

                //skip prefabs and objects with the level component
                if (!me.name.StartsWith("PF_", System.StringComparison.InvariantCulture)
                && me.gameObject.GetComponent<LevelEditor.Level>() == null) {
                    int childCount = me.childCount;
                    for (int c = 0; c < childCount; ++c) {
                        Transform child = me.GetChild(c);
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

                if (!emptyNode && optimizeSpawner) {
                    //is this a spawner
                    AbstractSpawner abstractSpawner = me.gameObject.GetComponent<AbstractSpawner>();
                    if (abstractSpawner != null) {
                        abstractSpawner.gameObject.SetActive(false);
                        spawners.Add(abstractSpawner);
                    }
                    AutoSpawnBehaviour autoSpawner = me.gameObject.GetComponent<AutoSpawnBehaviour>();
                    if (autoSpawner != null) {
                        autoSpawner.gameObject.SetActive(false);
                        autoSpawners.Add(autoSpawner);
                    }
                    ActionPoint actionPoint = me.gameObject.GetComponent<ActionPoint>();
                    if (actionPoint != null) {
                        actionPoint.gameObject.SetActive(false);
                        actionPoints.Add(actionPoint);
                    }
                }
               
            }

            if (optimizeSpawner
            && (spawners.Count > 0 || autoSpawners.Count > 0 || actionPoints.Count > 0)) {
                GameObject go = new GameObject("SpawnerCollection");
                SpawnerCollection sc = go.AddComponent<SpawnerCollection>();
                sc.abstractSpawners = spawners;
                sc.autoSpawners = autoSpawners;
                sc.actionPoints = actionPoints;
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
        }
#endif
    }
}
