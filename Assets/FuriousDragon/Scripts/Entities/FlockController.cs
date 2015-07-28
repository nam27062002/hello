using UnityEngine;
using System.Collections;

public class FlockController : MonoBehaviour {

	// Normal spawn
	public Object entityPrefab;
	public int numEntities = 4;
	public float range = 800f;
	public float guideSpeed = 2f;

	[HideInInspector]
	public GameObject[] entities;
	GameObject player;
	bool 	playerInRange = false;
	float 	sqrRange;
	Bounds  bounds;

	// Flock control
	[HideInInspector]
	public Vector3  followPos;
	float    timer;

	public enum GuideFunction{
		SMALL_FLOCK,
		FAST_FLOCK
	};

	public GuideFunction guideFunction = GuideFunction.SMALL_FLOCK;
	
	// Use this for initialization
	void Start () {
		
		entities = new GameObject[numEntities];
		for(int i=0;i<numEntities;i++){
			
			GameObject obj = (GameObject)Object.Instantiate(entityPrefab);
			obj.transform.parent = transform;
			obj.GetComponent<FlockBehaviour>().flock = this;
			obj.SetActive(false);
			entities[i] = obj;
		}
		
		player = GameObject.Find ("Player");
		sqrRange = range*range;
		
		BoxCollider2D box = GetComponent<BoxCollider2D>();
		if (box != null){
			bounds = box.bounds;
			box.enabled = false;
		}else{
			bounds = new Bounds(transform.position,Vector3.one*100f);
		}

		followPos = bounds.center;
	}

	void OnDestroy(){
		foreach(GameObject obj in entities){
			
			DestroyObject (obj);
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Control flocking
		// Move target for follow behaviour
		timer += Time.deltaTime*guideSpeed;
		if (guideFunction== GuideFunction.SMALL_FLOCK){
			followPos.x = bounds.center.x + Mathf.Sin (timer*0.5f)*bounds.size.x*0.5f+Mathf.Cos (timer*0.25f)*bounds.size.x*0.5f;
			followPos.y = bounds.center.y + Mathf.Sin (timer*0.35f)*bounds.size.y*0.5f+Mathf.Cos (timer*0.65f)*bounds.size.y*0.5f;
		}else if (guideFunction== GuideFunction.FAST_FLOCK){
			followPos.x = bounds.center.x + Mathf.Sin (timer)*bounds.size.x;
			followPos.y = bounds.center.y + Mathf.Cos (timer)*bounds.size.y;
		}

		// Hide/respawn depending on player distance
		float d = (transform.position - player.transform.position).sqrMagnitude;
		
		if (d < sqrRange && !playerInRange){
			
			playerInRange = true;
			
			foreach(GameObject obj in entities){
				
				obj.GetComponent<SpawnableBehaviour>().Spawn(bounds);
				obj.SetActive (true);
			}
		}else if (d > sqrRange && playerInRange){
			
			playerInRange = false;
			
			foreach(GameObject obj in entities){
				
				obj.SetActive (false);
			}

			Messenger.Broadcast<GameObject>("SpawnOutOfRange",this.gameObject);
		}

	}
}
