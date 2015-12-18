using UnityEngine;
using System.Collections;

public class SoldierGunBehaviour : MonoBehaviour {


	public Object shootPrefab;
	public float shootRate = 1f; // Shoots per second
	public int numShoots = 1; // SHoots per wave
	public float shootCooldown = 2f; // Time between waves
	public bool autoTarget = true;

	GameObject player;
	DamageDealer_OLD damageDealer;
	Transform  cannon;

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
		cannon = transform.FindTransformRecursive("gun_0");

		damageDealer = GetComponent<DamageDealer_OLD>();
	}
	
	// Update is called once per frame
	void Update () {
	
		if ( state == State.SHOOTING){

			timer -= Time.deltaTime;
			if (timer <=0f){

				Vector3 d =  player.transform.position-cannon.position;
				d.Normalize();

				// Instantiate a bullet and give it properties
				GameObject sht = (GameObject)Object.Instantiate(shootPrefab);
				sht.transform.position = cannon.position;

				BulletBehaviour bullet = sht.GetComponent<BulletBehaviour>();
				bullet.dir = d;
				bullet.source = damageDealer;

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
