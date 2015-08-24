using UnityEngine;
using System.Collections;

public class AnimalBehaviour : MonoBehaviour {

	public enum State{
		
		IDLE,
		WALK,
		PANIC,
		BURN,
		EXPLODE,
		DEAD
	}

	public float walkSpeed = 100f;
	
	[HideInInspector]
	public State state = State.IDLE;
	
	float timer = 0f;
	float bounceTimer = 0f;
	Animator anim;
	GameEntity entity;
	EdibleBehaviour edible;
	SpawnableBehaviour spawn;
	Vector3 originScale;
	Transform player;
	
	Vector3 dir = Vector3.right;
	Vector3 pos = Vector3.zero;
	Vector3 fpos;
	int groundMask;
	bool initialized = false;
	float panicDistanceSqr = 600f;

	Material[] originalMaterials;
	
	void Initialize() {
		
		initialized = true;
		entity = GetComponent<GameEntity>();
		edible = GetComponent<EdibleBehaviour>();
		originScale = transform.localScale;
		anim = GetComponent<Animator>();
		anim.SetTrigger("idle");
		spawn = GetComponent<SpawnableBehaviour>();
		player = GameObject.Find ("Player").transform;

		groundMask = 1 << LayerMask.NameToLayer("Ground");

		bounceTimer = Random.Range (0f,1f);

		panicDistanceSqr *= panicDistanceSqr;
	}
	
	void Update () {
		
		if (!initialized)
			Initialize();
		
		if (edible.state != EdibleBehaviour.State.NONE)
			return;
		
		if (spawn.state == SpawnableBehaviour.State.SPAWN)
			Spawn ();
	
		bool dragonClose = (pos - player.position).sqrMagnitude < panicDistanceSqr;
		bool dragonRight = pos.x < player.position.x;

		if (state == State.IDLE){
			
			timer -= Time.deltaTime;
			if (timer < 0f){
				state = State.WALK;
				anim.SetTrigger("walk");
				timer = Random.Range(4,8);
			}

			if (dragonClose){
				anim.SetTrigger("run");
				state = State.PANIC;
				timer = 0f;
				dir.x = dragonRight? -1:1;
			}
		}else if (state == State.WALK){


			if (dir.x == -1 && pos.x < spawn.bounds.min.x){
				dir.x = 1;
				UpdateDir();
			}else if (dir.x == 1 && pos.x > spawn.bounds.max.x){
				dir.x = -1;
				UpdateDir();
			}

			timer -= Time.deltaTime;
			if (timer < 0f){
				if (Random.Range(0,100) < 50){
					state = State.IDLE;
					anim.SetTrigger("idle");
					timer = Random.Range(4,8);
				}else{
					timer = Random.Range(4,8);
				}
				dir.x = -dir.x;
				
				UpdateDir ();
			}else{
				
				pos += dir*Time.deltaTime*walkSpeed;
			}

			if (dragonClose){
				anim.SetTrigger("run");
				state = State.PANIC;
				timer = 0f;
				dir.x = dragonRight? -1:1;
			}

		}else if(state == State.PANIC){

			if (pos.x < spawn.bounds.min.x){
				dir.x = 1;
			}else if (pos.x > spawn.bounds.max.x){
				dir.x = -1;
			}
			
			UpdateDir ();
			pos += dir*Time.deltaTime*walkSpeed*4f;

			timer += Time.deltaTime;
			if (!dragonClose && timer > 4f){
				state = State.WALK;
				anim.SetTrigger("walk");
				timer = Random.Range(4,8);
			}

		}else if (state == State.BURN){
			
			timer -= Time.deltaTime;
			if (timer < 0f){
				state = State.DEAD;
			}
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


		// Apply gravity
		if (state != State.EXPLODE){

			Vector3 testp = pos;
			testp.y+=50f;
			RaycastHit ground;
			if (Physics.Linecast( testp, testp + Vector3.down*70f, out ground, groundMask)){
				pos.y = ground.point.y-5f;
			}else{
				pos.y -= 200f*Time.deltaTime;
				if (state == State.WALK)
					state = State.IDLE;
			}
		}
	

		transform.position = pos;

		//transform.Rotate(Vector3.up*360f*Time.deltaTime);
	}

	void UpdateDir(){

		if (dir.x < 0f)
			transform.rotation = Quaternion.AngleAxis(-90f,Vector3.up);
		else if (dir.x > 0f)
			transform.rotation = Quaternion.AngleAxis(90f,Vector3.up);
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
		
		if (Random.Range(0,100) < 50){
			state = State.WALK;
			anim.SetTrigger("walk");
		}else
			state = State.IDLE;


		
		timer = Random.Range(2,8);
		dir.x = -1;
		pos = spawn.bounds.center;
		
		pos.x  += Random.Range (-spawn.bounds.extents.x,spawn.bounds.extents.x);
		pos.y  += Random.Range (-spawn.bounds.extents.y,spawn.bounds.extents.y);
		pos.z  += Random.Range (-30f,5f);


		groundMask = 1 << LayerMask.NameToLayer("Ground");
		
		// Plant them on start
		RaycastHit ground;
		if (Physics.Linecast( pos, pos + Vector3.down*1000f, out ground, groundMask)){
			pos.y = ground.point.y-7f;
			transform.position = pos;
		}

		transform.localPosition = Vector3.zero;
		transform.position = pos;
		transform.localScale = originScale;

		GetComponent<GameEntity>().RestoreHealth();

		if (Random.Range(0,1000) < 200){
			entity.isGolden = true;
			Material goldMat = Resources.Load ("Materials/Gold") as Material;
			if (originalMaterials == null)
				 originalMaterials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
			Material[] materials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
			for(int i=0;i<materials.Length;i++){
				materials[i] = goldMat;
			}
			GetComponentInChildren<SkinnedMeshRenderer>().materials = materials;
		}else{
			entity.isGolden = false;
			if (originalMaterials != null)
				GetComponentInChildren<SkinnedMeshRenderer>().materials = originalMaterials;
		}

		edible.OnSpawn();

		UpdateDir();
	}
}
