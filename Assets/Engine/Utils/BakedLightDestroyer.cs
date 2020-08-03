using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakedLightDestroyer : MonoBehaviour {

    void Awake()
    {
        Light[] lights = GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            if (light.type != LightType.Directional)
            {
                Destroy(light.gameObject);
            }
        }
    }
    // Use this for initialization
    void Start () {
        Destroy(this);
	}
	
}
