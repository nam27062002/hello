using UnityEngine;
using System.Collections;

public class BulletBehaviour : MonoBehaviour {

	public float speed = 800f;
	float timer = 0f;
	public int damage = 10;

	[HideInInspector] public Vector3 dir;
	[HideInInspector] public DamageDealer_OLD source;

	DragonMotion player;

	Vector3 pos;
	float impactDist;

	void Start(){
		player = GameObject.Find ("Player").GetComponent<DragonMotion>();
		impactDist = 100f*100f;
	}

	// Update is called once per frame
	void Update () {

		timer += Time.deltaTime;
		if (timer > 3f)
			DestroyObject(this.gameObject);


		pos = transform.position;
		pos += dir*(speed*Time.deltaTime);

		transform.position = pos;

		if ((pos - player.transform.position).sqrMagnitude < impactDist){

			player.OnImpact(pos,damage, 100f, source);
			DestroyObject(this.gameObject);
		}
	}
}
