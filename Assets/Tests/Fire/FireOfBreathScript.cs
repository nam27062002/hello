using UnityEngine;
using System.Collections;

public class FireOfBreathScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        MeshRenderer mrenderer = GetComponent<MeshRenderer>();
        mrenderer.material.SetFloat("seed", Random.value);

        Animation anim = GetComponent<Animation>();
        anim.Play();
//        Debug.Log("FireOfBreathScript - Start()");
//        Destroy(gameObject, anim.clip.length);
	}
	
}
