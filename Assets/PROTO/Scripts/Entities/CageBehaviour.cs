using UnityEngine;
using System.Collections;

public class CageBehaviour : MonoBehaviour {

	FlamableHeli flamable;
	GameEntity entity;

	public GameObject normalView;   // View of the cage object damaged
	public GameObject damagedView;   // View of the cage object damaged
	public GameObject[] breakParts;  // Parts that come out of the cage when it breaks
	public Object[] insideObjects;  // Entities that are spawned when the cage breaks

	enum State{
		
		IDLE,
		EXPLODE,
		BROKEN,
		RELEASED,
		DEAD
	}

	enum DamageState{
		NORMAL,
		DAMAGED,
		BROKEN
	}


	State state = State.IDLE;
	DamageState damageState = DamageState.NORMAL;
	float timer = 0;
	bool initialized = false;
	SpawnableBehaviour spawn;
	Vector3 releaseSpeed = Vector3.zero;
	Vector3 releaseTorque;
	int groundMask;
	float weight;
	float releaseHeight;
	float maxHealth;
	
	// Use this for initialization
	void Initialize () {
		
		entity = gameObject.GetComponent<GameEntity>();
		flamable = GetComponent<FlamableHeli>();
		spawn = GetComponent<SpawnableBehaviour>();
		GetComponent<GrabableBehaviour>().grabDelegate = GrabDelegate;
		GetComponent<GrabableBehaviour>().releaseDelegate = ReleaseDelegate;
		weight = GetComponent<GrabableBehaviour>().weight;
		GetComponent<HittableBehaviour>().hitDelegate = HitDelegate;
		maxHealth = entity.health;
		GameObject.Find("Spawn").GetComponent<SpawnZonesController>().Add(this.gameObject);
		
		if (damagedView != null)
			damagedView.GetComponent<MeshRenderer>().enabled = false;

		for(int i=0;i< breakParts.Length; i++)
			breakParts[i].GetComponent<MeshRenderer>().enabled = false;

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
				
				if (damagedView != null)
					damagedView.GetComponent<MeshRenderer>().enabled = false;
				
				GetComponent<BoxCollider>().isTrigger = true;
				
				state = State.DEAD;
			}
		}else if (state == State.RELEASED){
			
			//releaseSpeed += Vector3.down*(800f*weight)*Time.deltaTime;
			//transform.position = transform.position+releaseSpeed*Time.deltaTime;
			//transform.Rotate(releaseTorque*Time.deltaTime);
			
			RaycastHit ground;
			if (Physics.Linecast( transform.position, transform.position + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))){
				float flyHeight =  transform.position.y - ground.point.y;
				if (flyHeight < 80f){
					entity.health -= releaseHeight*0.1f;
					UpdateDamage ();
					if (entity.health > 0f){
						GetComponent<GrabableBehaviour>().ResetGrab();
						state = State.IDLE;
					}
				}
			}
		}
	}
	
	void Spawn(){ 
		
		GetComponent<BoxCollider>().enabled = true;
		GetComponent<BoxCollider>().isTrigger = false;
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<GrabableBehaviour>().enabled = true;
		GetComponent<GrabableBehaviour>().ResetGrab();

		if (damagedView != null){
			normalView.GetComponent<MeshRenderer>().enabled = true;
			damagedView.GetComponent<MeshRenderer>().enabled = false;
		}

		for(int i=0;i< breakParts.Length; i++){
			breakParts[i].GetComponent<BrokenPart>().OnSpawn();
		}
		
		state = State.IDLE;
		damageState = DamageState.NORMAL;
		spawn.state = SpawnableBehaviour.State.NONE;
		
		transform.position 		= spawn.position;
		transform.rotation 		= spawn.rotation;
		transform.localScale 	= spawn.scale;
		
		entity.RestoreHealth();

		maxHealth = entity.health;
	}
	
	void FinalExplosion(){
		
		CompositeExplosion explosion = GetComponent<CompositeExplosion>();
		if(explosion != null) {
			explosion.Explode();
		}
		
		ExplosionExpansion exp = ((GameObject)Object.Instantiate(Resources.Load ("PROTO/Effects/ExplosionExpansion"))).GetComponent<ExplosionExpansion>();
		exp.finalRadius = 500f;
		Vector3 p = transform.position;
		p.z = 0f;
		exp.center = p;
		
		
		GetComponent<BoxCollider>().isTrigger = false;
		GetComponent<Rigidbody>().isKinematic = false;
		
		if (damagedView != null){
			normalView.GetComponent<MeshRenderer>().enabled = false;
			damagedView.GetComponent<MeshRenderer>().enabled = true;
		}
		
		Vector3 force = new Vector3(Random.Range(-0.4f,0.4f),1f,0f);
		force *= Random.Range (20f,40f)*1000f;
		GetComponent<Rigidbody>().AddForce(force);
		GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-1f,1f)*130000f,Random.Range(-1f,1f)*130000f,Random.Range(-1f,1f)*130000f));
		
		Camera.main.GetComponent<CameraController_OLD>().Shake ();
		GetComponent<GrabableBehaviour>().enabled = false;
		
		state = State.EXPLODE;
		
	}
	
	public void GrabDelegate(){
		
		// When grabbed we don't want the spawn zones controller to eliminate this car when out of range
		//GetComponent<SpawnableBehaviour>().state = SpawnableBehaviour.State.INVALID;
	}

	public void HitDelegate(){

		if (state == State.IDLE)
			UpdateDamage ();
	}

	void UpdateDamage(){

		if (entity.health < maxHealth*0.5f && damageState == DamageState.NORMAL){

			damageState = DamageState.DAMAGED;
			normalView.GetComponent<MeshRenderer>().enabled = false;
			damagedView.GetComponent<MeshRenderer>().enabled = true;
		}
		if (entity.health < 0 && (damageState == DamageState.NORMAL || damageState == DamageState.DAMAGED)){

			//romper
			damageState = DamageState.BROKEN;
			state = State.BROKEN;
			damagedView.GetComponent<MeshRenderer>().enabled = false;
			GetComponent<Rigidbody>().isKinematic = true;
			GetComponent<BoxCollider>().isTrigger = true;
			GetComponent<GrabableBehaviour>().enabled = false;
			for(int i=0;i< breakParts.Length; i++){
				breakParts[i].GetComponent<MeshRenderer>().enabled = true;
				breakParts[i].GetComponent<BrokenPart>().Break(transform.position);
			}


			Bounds spawnBounds = GetComponent<BoxCollider>().bounds;
			for(int i=0; i< insideObjects.Length;i++){
				GameObject obj = (GameObject)Object.Instantiate(insideObjects[i]);
				obj.GetComponent<SpawnableBehaviour>().Spawn(spawnBounds);
			}
		}
	}
	
	public void ReleaseDelegate(Vector3 momentum){

			releaseSpeed = momentum/(weight*0.5f);
			RaycastHit ground;
			if (Physics.Linecast( transform.position, transform.position + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))){
				releaseHeight =  transform.position.y - ground.point.y;
				Debug.Log ("release height " + releaseHeight.ToString());
			}

			state = State.RELEASED;
	}
}
