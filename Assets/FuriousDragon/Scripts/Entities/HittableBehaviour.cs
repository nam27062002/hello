using UnityEngine;
using System.Collections;

public class HittableBehaviour : MonoBehaviour {

	GameEntity entity;
	float timer = 0f;

	public delegate void HitDelegate();

	public HitDelegate hitDelegate;

	// Use this for initialization
	void Start () {
		entity = GetComponent<GameEntity>();
	}



	virtual public bool OnHit(float energy){

		if (Mathf.Abs(Time.time-timer) > 1f){

			Debug.Log ("Hit with "+energy.ToString()+" energy");

			entity.health -= energy;
			timer = Time.time;

			if (hitDelegate != null)
				hitDelegate();

			return true;
		}

		return false;
	}

}
