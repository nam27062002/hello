using UnityEngine;
using System.Collections;

public class PersonBehaviour : MonoBehaviour {

	public enum State{
	
		IDLE,
		FLEE,
		AFRAID,
		BURN,
		EXPLODE,
		DEAD
	}


	public float panicDistance = 400f;
	public float runSpeed = 300f;

	[HideInInspector]
	public State state = State.IDLE;

	float timer = 0f;
	Animator anim;
	GameEntity entity;
	EdibleBehaviour edible;
	SpawnableBehaviour spawn;
	Transform player;
	Vector3 originScale;
	GameObject fire;

	Vector3 dir = Vector3.zero;
	Vector3 pos = Vector3.zero;
	int groundMask;
	bool initialized = false;
	float panicDistanceSqr;


	void Initialize() {

		initialized = true;
		entity = GetComponent<GameEntity>();
		edible = GetComponent<EdibleBehaviour>();
		originScale = transform.localScale;
		anim = transform.FindChild ("view").GetComponent<Animator>();
		spawn = GetComponent<SpawnableBehaviour>();
		player = GameObject.Find ("Player").transform;

		groundMask = 1 << LayerMask.NameToLayer("Ground");
		panicDistanceSqr = panicDistance*panicDistance*Random.Range (0.75f,1.25f);
		runSpeed *= Random.Range (0.75f,1.25f);
		pos = transform.position;

	}

	void Update () {
	
		if (!initialized)
			Initialize();

		if (edible.state != EdibleBehaviour.State.NONE){
			if (fire != null){
				fire.SetActive(false);
				fire = null;
			}
			return;
		}

		if (spawn.state == SpawnableBehaviour.State.SPAWN)
			Spawn ();

		bool dragonClose = (pos - player.position).sqrMagnitude < panicDistanceSqr*1.5f;
		bool dragonRight = pos.x < player.position.x;

		if (state == State.IDLE){

			// if the dragon is close
			if ( dragonClose){

				state  = State.FLEE;
			}

		}else if (state == State.FLEE){

			if (pos.x < spawn.bounds.min.x){
				if (dragonClose && dragonRight){
					dir.x = 1;
					state = State.AFRAID;
					timer = 0f;
				}else
					dir.x = 1;
			}else if (pos.x > spawn.bounds.max.x){
				if (dragonClose && !dragonRight){
					dir.x = -1;
					state = State.AFRAID;
					timer = 0f;
				}else
					dir.x = -1;
			}else{
				if (dragonRight)
					dir.x = -1;
				else
					dir.x = 1;
			}


			UpdateDir ();

			if (state != State.AFRAID){

				if ( !dragonClose){
					state = State.IDLE;
				}else{
					pos += dir*Time.deltaTime*runSpeed;
				}
			}
		}else if (state == State.BURN){

		
			if (pos.x < spawn.bounds.min.x){
				dir.x = 1;
			}else if (pos.x > spawn.bounds.max.x){
				dir.x = -1;
			}
			
			UpdateDir ();
			pos += dir*Time.deltaTime*runSpeed*1.5f;

			Vector3 fpos = pos;
			fpos.y += 20f;
			fire.transform.position = fpos;
		}else if (state == State.AFRAID){

			timer += Time.deltaTime;
			if (timer > 4f)
				state = State.IDLE;
		}else if (state == State.EXPLODE){
		
			pos += dir*(400f*Time.deltaTime);
			dir += Vector3.down*(0.8f*Time.deltaTime);
		//	transform.Rotate(Vector3.forward*720f*Time.deltaTime);

			if (dir.y < 0f){
				Vector3 testp = pos;
				testp.y+=20f;
				RaycastHit ground;
				if (Physics.Linecast( testp, testp+Vector3.down*40f, out ground, groundMask)){
					pos.y = ground.point.y;
					state = State.DEAD;
				}
			}
		}


		anim.SetBool ("run", state == State.FLEE);
		anim.SetBool ("idle", state == State.IDLE);
		anim.SetBool ("scared", state == State.AFRAID);

		// Apply gravity
		if (state != State.EXPLODE){
			Vector3 testp = pos;
			testp.y+=20f;
			RaycastHit ground;
			if (Physics.Linecast( testp, testp+Vector3.down*40f, out ground, groundMask)){
				pos.y = ground.point.y;
			}else{
				pos.y -= 200f*Time.deltaTime;
				if (state == State.FLEE)
					state = State.IDLE;
			}
		}

		//anim.SetBool ("walk",state == State.WALK);
		transform.position = pos;
	}

	void UpdateDir(){
	
		Vector3 lscale = transform.localScale;
		lscale.x = Mathf.Sign(dir.x)*originScale.x;
		transform.localScale = lscale;
	}

	public void OnBurn(){

		if (state != State.BURN && state != State.DEAD && state != State.EXPLODE){
			state = State.BURN;
			anim.SetBool ("run", true);

			FirePool pool = GameObject.Find ("FirePool").GetComponent<FirePool>();
		    fire = pool.GetParticle();
		
			if (fire){
					
				fire.GetComponent<FireParticle>().Activate ();
			}


		}
	}

	public void OnExplode(Vector3 center){

		state = State.EXPLODE;
		if (center.x > pos.x)
			dir = new Vector3(Random.Range(-1.0f,-0.5f),Random.Range(0.25f,0.75f),0f);
		else
			dir = new Vector3(Random.Range(0.5f,1.0f),Random.Range(0.25f,0.75f),0f);

		dir.Normalize();

	}
	
	public void Spawn(){

		spawn.state = SpawnableBehaviour.State.NONE;

		state = State.IDLE;

		timer = Random.Range(2,8);
		dir.x = -1;
		pos = spawn.bounds.center;
		
		pos.x  += Random.Range (-spawn.bounds.extents.x,spawn.bounds.extents.x);
		pos.y  += Random.Range (-spawn.bounds.extents.y,spawn.bounds.extents.y);
		pos.z  += Random.Range (-30f,5f);

		// Plant them on start
		RaycastHit ground;
		if (Physics.Linecast( pos, pos+Vector3.down*300f, out ground, groundMask))
			pos.y = ground.point.y;

	    transform.position = pos;
		transform.localScale = originScale;

		GetComponent<GameEntity>().RestoreHealth();
		if (Random.Range(0,1000) < 200){
			GetComponent<GameEntity>().isGolden = true;
			Material goldMat = Resources.Load ("Materials/Gold") as Material;
			Material[] materials = transform.FindChild ("view").GetComponentInChildren<SkinnedMeshRenderer>().materials;
			for(int i=0;i<materials.Length;i++){
				materials[i] = goldMat;
			}
			transform.FindChild ("view").GetComponentInChildren<SkinnedMeshRenderer>().materials = materials;
		}else{
			GetComponent<GameEntity>().isGolden = false;
		}

		edible.OnSpawn();
	}

	void OnDisable(){

		if (fire != null){
			fire.SetActive(false);
			fire = null;
		}
	}
}
