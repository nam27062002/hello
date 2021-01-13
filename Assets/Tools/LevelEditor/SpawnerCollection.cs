using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerCollection : MonoBehaviour {

    [SerializeField] public List<AbstractSpawner> abstractSpawners;
    [SerializeField] public List<AutoSpawnBehaviour> autoSpawners;
    [SerializeField] public List<ActionPoint> actionPoints;

    // Use this for initialization
    void Start() {
        if (abstractSpawners != null) {
            if (Application.isPlaying) {
                if (abstractSpawners != null) {
                    foreach (AbstractSpawner abs in abstractSpawners) {                        
                        abs.DoStart();
                    }
                }

                if (autoSpawners != null) {
                    foreach (AutoSpawnBehaviour aus in autoSpawners) {
                        aus.gameObject.SetActive(true);
                        aus.DoStart();
                    }
                }

                if (actionPoints != null) {
                    foreach (ActionPoint aps in actionPoints) {
                        aps.gameObject.SetActive(true);
                        aps.DoStart();
                    }
                }
            }
        }
	}
}
