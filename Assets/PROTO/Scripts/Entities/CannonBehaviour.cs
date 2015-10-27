using UnityEngine;
using System.Collections;

public class CannonBehaviour : MonoBehaviour {


	public Object shootPrefab;
	public float shootRate = 2f; // Shoots per second
	public int numShoots = 3; // SHoots per wave
	public float shootCooldown = 2f; // Time between waves
	public bool autoTarget = true;

	SpriteRenderer flash;
	GameObject player;
	DamageDealer_OLD damageDealer;

	float timer;
	int shootsFired;

	enum State{
		IDLE,
		SHOOTING,
		COOLDOWN
	}

	State state = State.IDLE;

	// Use this for initialization
	void Start () {

		player = GameObject.Find ("Player");
		damageDealer = GetComponent<DamageDealer_OLD>();
		GetComponent<Animator>().SetTrigger("flash");
		flash = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
	
		if ( state == State.SHOOTING){
			timer -= Time.deltaTime;
			if (timer <=0f){

				Vector3 d =  player.transform.position-transform.position;
				d.Normalize();
				if (Mathf.Abs(d.x) > Mathf.Abs(d.y)){  // This avoids shooting too vertical

					// Instantiate a bullet and give it properties
					GameObject sht = (GameObject)Object.Instantiate(shootPrefab);
					sht.transform.position = transform.position;

					BulletBehaviour bullet = sht.GetComponent<BulletBehaviour>();
					bullet.dir = d;
					bullet.source = damageDealer;

					GetComponent<Animator>().SetTrigger("flash");
				}

				shootsFired--;
				if (shootsFired == 0){
					state = State.COOLDOWN;
					timer = shootCooldown;
				}else{
					timer = 1f/shootRate;
				}
			}
		}else if (state == State.COOLDOWN){

			timer -= Time.deltaTime;
			if (timer <= 0f){
				state = State.SHOOTING;
				shootsFired = numShoots;
				timer = 1f/shootRate;
			}
		}
	}


	public void Shoot(){
		if (state == State.IDLE){
			state = State.SHOOTING;
			shootsFired = numShoots;
			timer = 1f/shootRate;
		}
	}

	public void StopShooting(){
		state = State.IDLE;
	}
}
