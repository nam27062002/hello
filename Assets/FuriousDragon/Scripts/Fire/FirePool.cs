using UnityEngine;
using System.Collections;

public class FirePool : MonoBehaviour {

	const int maxFireParticles = 256;
	GameObject[] fire = new GameObject[maxFireParticles];


	void Start () {
	
		Transform instances = GameObject.Find ("Instances").transform;
		
		Object firePrefab = Resources.Load("Proto/FireSprite");
		for(int i=0;i<maxFireParticles;i++){
			GameObject fireObj = (GameObject)Object.Instantiate(firePrefab);
			fireObj.transform.parent = instances;
			fireObj.transform.localPosition = Vector3.zero;
			fireObj.SetActive(false);
			fire[i] = fireObj;
		}
	}

	public GameObject GetParticle(){
	
		foreach(GameObject fireObj in fire){
			if (!fireObj.activeInHierarchy){
				return fireObj;
			}
		}

		return null;
	}
}
