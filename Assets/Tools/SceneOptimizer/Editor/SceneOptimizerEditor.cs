using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class SceneOptimizerEditor {
        
	public static string PATH = "Assets" + Path.DirectorySeparatorChar + "Game" + Path.DirectorySeparatorChar + "Scenes" + 
		Path.DirectorySeparatorChar + "Generated";

    public static void BatchOptimization() {
		//clean old folder
		EditorFileUtils.DeleteFileOrDirectory(PATH);
		
		Stack <DirectoryInfo> directories = new Stack<DirectoryInfo>();
        directories.Push(new DirectoryInfo("Assets/Game/Scenes/Levels/"));
        
        while (directories.Count > 0) {
            DirectoryInfo current = directories.Pop();
            DirectoryInfo[] children = current.GetDirectories();

            foreach (DirectoryInfo child in children) {
                directories.Push(child);
            }

            string assetsToken = "Assets" + Path.DirectorySeparatorChar;
            FileInfo[] files = current.GetFiles();

            foreach (FileInfo file in files) {
                if (file.Extension.Equals(".unity")) {
                    string srcPath = file.FullName;
                    string srcFilePath = srcPath.Substring(srcPath.IndexOf(assetsToken, System.StringComparison.Ordinal));
                    AssetImporter srcAI = AssetImporter.GetAtPath(srcFilePath);
                    if (!string.IsNullOrEmpty(srcAI.assetBundleName)) {
                        try {
                            UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(srcPath, UnityEditor.SceneManagement.OpenSceneMode.Single);
                            DoOptimize(scene);

							string dstPath = srcPath.Substring(0, srcPath.IndexOf(assetsToken, System.StringComparison.Ordinal + 1));
                            dstPath +=  PATH + Path.DirectorySeparatorChar + file.Name;
							UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, dstPath);

                            string dstFilePath = dstPath.Substring(dstPath.IndexOf(assetsToken, System.StringComparison.Ordinal));
                            AssetDatabase.ImportAsset(dstFilePath, ImportAssetOptions.ForceSynchronousImport);

                            AssetImporter dstAI = AssetImporter.GetAtPath(dstFilePath);
                            dstAI.assetBundleName = srcAI.assetBundleName;
                        } catch(System.Exception _e) {
                            Debug.LogError("[SceneOptimizerEditor] " + file.Name + " > " + _e.StackTrace);
                        }
                    }
                }
            }
        }
                
        AssetDatabase.SaveAssets();
    }

    private static void DoOptimize(UnityEngine.SceneManagement.Scene _scene) {
        Queue<Transform> nodes = new Queue<Transform>();
        List<Transform> childs = new List<Transform>();
        List<ActionPoint> actionPoints = new List<ActionPoint>();
        List<AbstractSpawner> spawners = new List<AbstractSpawner>();
        List<AutoSpawnBehaviour> autoSpawners = new List<AutoSpawnBehaviour>();
        
        GameObject[] roots = _scene.GetRootGameObjects();

        foreach (GameObject go in roots) {
            SpawnerCollection spawnerCollection = go.GetComponent<SpawnerCollection>();
            if (spawnerCollection != null) {
                Object.DestroyImmediate(spawnerCollection);
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
                    Object.DestroyImmediate(me.gameObject);
                }
            }

            if (!emptyNode) {
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

        if (spawners.Count > 0 || autoSpawners.Count > 0 || actionPoints.Count > 0) {
            GameObject go = new GameObject("SpawnerCollection");
            SpawnerCollection sc = go.AddComponent<SpawnerCollection>();
            sc.abstractSpawners = spawners;
            sc.autoSpawners = autoSpawners;
            sc.actionPoints = actionPoints;
        }
    }   

    public static string GetGeneratedGUID(string sceneName)
    {        
        if (!sceneName.EndsWith(".unity")) {
            sceneName += ".unity";
        }

        string path = PATH + "/" + sceneName;
        return AssetDatabase.AssetPathToGUID(path);        
    }
}
 