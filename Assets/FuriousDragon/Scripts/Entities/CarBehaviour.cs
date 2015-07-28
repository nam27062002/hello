using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameEntity))]
public class CarBehaviour : MonoBehaviour {

	public float mass = 1.5f;

	FlamableHeli flamable;
	GameEntity entity;
		
	enum State{
		
		IDLE,
		EXPLODE,
		RELEASED,
		DEAD
	}
	
	State state = State.IDLE;
	float timer = 0;
	Transform burntCar;
	bool initialized = false;
	SpawnableBehaviour spawn;
	Vector3 releaseSpeed = Vector3.zero;
	Vector3 releaseTorque;
	int groundMask;

	// Use this for initialization
	void Initialize () {

		entity = gameObject.GetComponent<GameEntity>();
		flamable = GetComponent<FlamableHeli>();
		spawn = GetComponent<SpawnableBehaviour>();
		GetComponent<GrabableBehaviour>().grabDelegate = GrabDelegate;
		GetComponent<GrabableBehaviour>().releaseDelegate = ReleaseDelegate;

		GameObject.Find("Spawn").GetComponent<SpawnZonesController>().Add(this.gameObject);

		burntCar = transform.FindChild ("burnt");
		if (burntCar != null)
			burntCar.GetComponent<MeshRenderer>().enabled = false;

		initialized = true;
	}
	
	void Update () {
	
		if (!initialized)
			Initialize();
	
		if (spawn.state == SpawnableBehaviour.State.SPAWN)
			Spawn ();

		// Check damage by fire
		if (entity.health <= 0f && state == State.IDLE){
			FinalExplosion();

			// [AOC] If it has the slow motion component, activate it
			SlowMotionController sloMo = GetComponent<SlowMotionController>();
			if(sloMo != null) {
				sloMo.StartSlowMotion();
			}

			timer = 0f;

		}else if (state == State.EXPLODE){

			timer += Time.deltaTime;
			if (timer > 3f){

				if (burntCar != null)
					burntCar.GetComponent<MeshRenderer>().enabled = false;

				GetComponent<BoxCollider>().isTrigger = true;

				state = State.DEAD;
			}
		}else if (state == State.RELEASED){

			releaseSpeed += Vector3.down*(800f*mass)*Time.deltaTime;
			transform.position = transform.position+releaseSpeed*Time.deltaTime;
			transform.Rotate(releaseTorque*Time.deltaTime);

			RaycastHit ground;
			if (Physics.Linecast( transform.position, transform.position + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))){
				float flyHeight =  transform.position.y - ground.point.y;
				if (flyHeight < 80f){
					FinalExplosion();
					spawn.state = SpawnableBehaviour.State.NONE;
				}
			}
		}
	}

	void Spawn(){ 
	
		GetComponent<BoxCollider>().enabled = true;
		GetComponent<BoxCollider>().isTrigger = false;
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<GrabableBehaviour>().enabled = true;

		if (burntCar != null){
			GetComponent<MeshRenderer>().enabled = true;
			burntCar.GetComponent<MeshRenderer>().enabled = false;
		}

		state = State.IDLE;
		spawn.state = SpawnableBehaviour.State.NONE;

		transform.position 		= spawn.position;
		transform.rotation 		= spawn.rotation;
		transform.localScale 	= spawn.scale;

		entity.RestoreHealth();
	}

	void FinalExplosion(){
		
		CompositeExplosion explosion = GetComponent<CompositeExplosion>();
		if(explosion != null) {
			explosion.Explode();
		}

		ExplosionExpansion exp = ((GameObject)Object.Instantiate(Resources.Load ("ExplosionExpansion"))).GetComponent<ExplosionExpansion>();
		exp.finalRadius = 500f;
		Vector3 p = transform.position;
		p.z = 0f;
		exp.center = p;


		GetComponent<BoxCollider>().isTrigger = false;
		GetComponent<Rigidbody>().isKinematic = false;

		if (burntCar != null){
			GetComponent<MeshRenderer>().enabled = false;
			burntCar.GetComponent<MeshRenderer>().enabled = true;
		}

		Vector3 force = new Vector3(Random.Range(-0.4f,0.4f),1f,0f);
		force *= Random.Range (20f,40f)*1000f;
		GetComponent<Rigidbody>().AddForce(force);
		GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-1f,1f)*130000f,Random.Range(-1f,1f)*130000f,Random.Range(-1f,1f)*130000f));

		Camera.main.GetComponent<CameraController>().Shake ();
		GetComponent<GrabableBehaviour>().enabled = false;

		state = State.EXPLODE;

	}

	public void GrabDelegate(){
			
		// When grabbed we don't want the spawn zones controller to eliminate this car when out of range
		GetComponent<SpawnableBehaviour>().state = SpawnableBehaviour.State.INVALID;
	}


	public void ReleaseDelegate(Vector3 momentum){

		releaseSpeed = momentum*0.75f*mass;
		releaseTorque = new Vector3(Random.value*300f-150f,Random.value*300f-150f,Random.value*300f);

		state = State.RELEASED;
	}

}
