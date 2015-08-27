using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameEntity))]
public class EdibleBehaviour : MonoBehaviour {

	GameEntity entity = null;

	[Range(0, 1)] public int edibleFromType = 0;  // From which dragon type is this edible

	public string bloodEmitter;     // bood emitter on death

	public Object[]  meatChunks; 

	public bool destroyOnEat = false;

	public enum State{
		NONE,
		GO_TO_MOUTH,
		EATEN,
		DEAD
	};


	public State state = State.NONE;

	[Space(10)]
	[Range(0, 1)] public float feedbackProbability = 0.5f;
	public List<UIFeedbackMessage> eatFeedbacks = new List<UIFeedbackMessage>();

	[HideInInspector]
	public bool bigPrey = false;   // big edible things are eaten with diferent animations
	[HideInInspector] 
	public Bounds modelbounds; // bounds for the 3d model of the entity

	float timer = 0f;
	DragonPlayer 	player;
	Transform	 	playerMouth;
	Transform	 	playerHead;
	Animator 		animator;
	ParticleSystem 	emitter;
	ParticleSystem 	chunkEmitter;
	Vector3 		originScale;
	Quaternion		originRotation;

	// Use this for initialization
	void Start () {

		entity = gameObject.GetComponent<GameEntity>();
		player = GameObject.Find ("Player").GetComponent<DragonPlayer>();
		playerMouth = player.FindSubObjectTransform("eat");
		playerHead = player.FindSubObjectTransform("head");
	
		animator = GetComponent<Animator>();

		originScale = transform.localScale;
		originRotation = transform.rotation;

		// find bounds
		MeshRenderer mesh = GetComponentInChildren<MeshRenderer>();
		if (mesh != null)
			modelbounds = mesh.bounds;
		else{
			SkinnedMeshRenderer smesh = GetComponentInChildren<SkinnedMeshRenderer>();
			if (smesh != null)
				modelbounds = smesh.bounds;
		}

		bigPrey = meatChunks != null && meatChunks.Length > 0;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (state == State.GO_TO_MOUTH){

			// Position the entity in the mouth of the dragon at the correct angle
			Vector3 playerMouthDir = (playerMouth.position-playerHead.position).normalized;
			Vector3 targetPosition = playerMouth.position+playerMouthDir*110f;
			targetPosition.z = 100f;
			targetPosition.y += 40f;
			transform.position = Vector3.Lerp(transform.position,targetPosition,0.25f);
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis(-90f,playerMouthDir),0.25f);
		
			if (emitter != null){
				Vector3 p = playerMouth.position;
				p.z -= 100f;
				emitter.transform.position = p;
			}

			timer -= Time.deltaTime;
			if (timer <= 0f){

				if (bigPrey)
					timer = 0.5f;
				else
					timer = 0.25f;
				state = State.EATEN;
			}
		}else if (state == State.EATEN){

			Vector3 playerMouthDir = (playerMouth.position-playerHead.position).normalized;
			Vector3 targetPosition = playerMouth.position+playerMouthDir*20f;
			targetPosition.z = 100f;
			transform.position = Vector3.Lerp(transform.position,targetPosition,0.25f);
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.AngleAxis(-90f,playerMouthDir),0.25f);
			transform.localScale = Vector3.Lerp (transform.localScale, originScale*0.7f,0.25f);

			if (emitter != null){
				Vector3 p = playerMouth.position;
				p.z -= 100f;
				emitter.transform.position = p;
			}

			timer -= Time.deltaTime;
			if (timer <= 0f){

				state = State.DEAD;
				timer = 2.0f;

				// Support for both 2D and 3D objects
				Renderer renderer = GetComponentInChildren<Renderer>();
				if(renderer != null) renderer.enabled = false;


				if (meatChunks != null){
					
					foreach(Object chunk in meatChunks){
						
						GameObject chunkObj = (GameObject)Object.Instantiate(chunk);
						chunkObj.transform.position = transform.position;
					}
				}

			}
		}else if (state == State.DEAD){
		
			timer -= Time.deltaTime;
			if (timer <= 0){
			
				if (emitter != null){
					DestroyObject(emitter.gameObject);
					emitter = null;
				}

				if (destroyOnEat)
					DestroyObject(this.gameObject);
				else{
					gameObject.SetActive(false);
					state = State.NONE;
				}
			}
		}
	}


	// This is called when the dragon eats this entity
	public void OnEat(){

		if (state == State.NONE){

			if (animator != null)
				animator.SetTrigger("dead");

			if (GetComponent<SphereCollider>() != null)
				GetComponent<SphereCollider>().enabled = false;
			if (GetComponent<BoxCollider>() != null)
				GetComponent<BoxCollider>().enabled = false;
			if (GetComponent<Rigidbody>() != null) 
				GetComponent<Rigidbody>().isKinematic = true;

			state = State.GO_TO_MOUTH;
			timer = 0.4f;

			if (bloodEmitter.Length > 0){

				GameObject  effect = (GameObject)Object.Instantiate (Resources.Load (bloodEmitter));
				effect.transform.localPosition = Vector3.zero;
				effect.transform.position = playerMouth.position;
				emitter = effect.GetComponent<ParticleSystem> ();
				emitter.GetComponent<Renderer>().sortingLayerName = "enemies";
				emitter.Stop();
				emitter.Play();
			}


			// Broadcast message
			Messenger.Broadcast<GameEntity>(GameEvents_OLD.ENTITY_EATEN, entity);
		}
	}

	public void OnSpawn(){

		if (chunkEmitter != null){
			DestroyObject(chunkEmitter.gameObject);
			chunkEmitter = null;
		}

		GetComponentInChildren<Renderer>().enabled =true;

		if (GetComponent<SphereCollider>() != null)
			GetComponent<SphereCollider>().enabled = true;
		if (GetComponent<BoxCollider>() != null)
			GetComponent<BoxCollider>().enabled = true;
		if (GetComponent<Rigidbody>() != null) 
			GetComponent<Rigidbody>().isKinematic = false;

		transform.rotation = originRotation;
		
		state = State.NONE;
	}
}
