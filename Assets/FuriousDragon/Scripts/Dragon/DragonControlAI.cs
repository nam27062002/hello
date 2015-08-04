using UnityEngine;
using System.Collections;

public class DragonControlAI : DragonControl {


	public float reactionDistance = 800f;
	public float homeDistance = 5000f;
	public float attackDistance = 600f;
	public float attackTime = 3f;
	public float damage = 20f;
	public int fearTreshold = 80;


	DragonPlayer 	player;
	DragonAi 		dragon;
	GameEntity 	 	entity;

	
	Vector3 origin;
	Vector3 originDir;
	Vector3 impulse;
	Vector3 playerDir;
	Vector3 targetOffset = Vector3.zero;
	Vector3 fleePosition = Vector3.zero;

	public enum State{
	
		IDLE,
		FOLLOW,
		CHARGE,
		TAKE_DAMAGE,
		MOVE_AWAY,
		FLEE,
		GO_BACK,
		DEAD
	};

	public State state = State.IDLE;

	float timer = 0f;
	float attackTimer = 0f;
	int   fear = 0;
	bool  orienting = false;


	// Use this for initialization
	void Start () {

		dragon = GetComponent<DragonAi>();
		player = GameObject.Find ("Player").GetComponent<DragonPlayer>();
		entity = GetComponent<GameEntity>();
		origin = transform.position;
		moving = true;
	}
	
	// Update is called once per frame
	void Update () {

		playerDir = (player.transform.position+targetOffset) - transform.position;
		float playerDistance = playerDir.magnitude;
		originDir = origin - transform.position;
		float originDistance = originDir.magnitude;
		orienting = false;

		if (state == State.IDLE){

			impulse = Vector3.zero;
			moving = false;
			timer += Time.deltaTime;

			if( playerDistance < reactionDistance ){
				state = State.FOLLOW;
				targetOffset = (transform.position - player.transform.position).normalized;
				targetOffset.y += Random.Range(-0.25f,0.25f);
				targetOffset = targetOffset.normalized*700f;
			}

		}else if (state == State.FOLLOW){

			impulse = playerDir.normalized;
			moving = true;

			attackTimer += Time.deltaTime;

			if (originDistance > homeDistance)
				state = State.GO_BACK;

			if (playerDistance <attackDistance && attackTimer > attackTime){
				state = State.CHARGE;
				attackTimer = 1f;
				action = true;
			}
			else if (playerDistance < 400f){
				orienting = true;
				moving = false;
			}
		}else if (state == State.MOVE_AWAY){

			impulse = -playerDir.normalized;
			timer += Time.deltaTime;
			moving = false;
			if (timer > 2f || playerDistance > attackDistance){
				state = State.FOLLOW;
				moving = true;
			}
			
		}else if (state == State.CHARGE){

			impulse = (player.transform.position - transform.position).normalized;
			moving = true;
			action = true;

			attackTimer -= Time.deltaTime;
			if (attackTimer < 0){
				state = State.FOLLOW;
				action = false;
			}

		}else if (state == State.GO_BACK){

			impulse = originDir.normalized;
			moving = true;
			if (originDistance< 200f){
				state = State.IDLE;
				timer = 0f;
			}else if( playerDistance < reactionDistance)
				state = State.FOLLOW;
		}else if (state == State.FLEE){

			impulse = (fleePosition-transform.position).normalized;
			moving = true;
			if (originDistance > homeDistance || playerDistance > 2000f)
				state = State.GO_BACK;

		}

		if (state != State.DEAD && entity.health <= 0f){
			state = State.DEAD;
			action = false;
			dragon.ChangeState(DragonAi.EState.DYING);
		}
	}
	
	override public Vector3 GetImpulse(float desiredVelocity){

		if (!orienting)
			return impulse*desiredVelocity;
		else 
			return impulse;
	}

	public void OnBurn(Vector3 position){
	
		if (state != State.DEAD){
			Vector3 force = (transform.position - position).normalized * 4000f;
			dragon.AddForce(force);

			fear ++;
			if (fear > fearTreshold){
				fear = 0;
				action = false;

				fleePosition = transform.position;
				fleePosition.x += Mathf.Abs(transform.position.x - player.transform.position.x)*3000f;

				state = State.FLEE;
			}
		}
	}

	void OnTriggerStay(Collider other) {
	
		if (state == State.CHARGE && attackTimer < 0.75f){

			DragonPlayer player = other.gameObject.GetComponent<DragonPlayer>();
			if (player != null && (player.transform.position-transform.position).magnitude < 300f){
			
				player.OnImpact(transform.position,damage,0.75f,null);
				state = State.FOLLOW;
				action = false;
			}
		}
	}

}
