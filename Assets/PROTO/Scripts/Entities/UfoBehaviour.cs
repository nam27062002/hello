using UnityEngine;
using System.Collections;

public class UfoBehaviour : MonoBehaviour {


	DragonMotion player;
	CannonBehaviour cannon;
	Object shootPrefab;
	DamageDealer_OLD damageDealer;
		
	enum State {
			
		IDLE,
		TRACKING,
		FALLING
	}
		
	State state = State.IDLE;
	Vector3 impulse = Vector3.zero;
	Vector3 targetPos = Vector3.zero;
	Vector3 targetOffset = Vector3.zero;
	Vector3 oriPos;
	Vector3 pos;
	Vector3 oldPos;
	float timer = 0;
	float decisionTimer = 0;
	float shootTimer = 2f;
	float angle = 0;
	Object expPrefab;
	int groundMask;
		
	// Use this for initialization
	void Start() {
		cannon = GetComponentInChildren<CannonBehaviour>();
		player = GameObject.Find("Player").GetComponent<DragonMotion>();
		shootPrefab = Resources.Load("PROTO/UfoLaser");
		damageDealer = GetComponent<DamageDealer_OLD>();
		pos = transform.position;
		oriPos = pos;
		groundMask = 1 << LayerMask.NameToLayer("Ground");
			
	}
		
	void Update() {
			
		oldPos = pos;
			
		if(state == State.IDLE) {
				
			UpdateIdle();

		}else if(state == State.TRACKING) {
				
			UpdateTracking();

			shootTimer -= Time.deltaTime;
			if(shootTimer <= 0f) {
						
				Vector3 d = player.transform.position - transform.position;
				d.Normalize();
				if(Mathf.Abs(d.x) > Mathf.Abs(d.y)) {  // This avoids shooting too vertical
						
					// Instantiate a bullet and give it properties
					GameObject sht = (GameObject)Object.Instantiate(shootPrefab);
					sht.transform.position = transform.position;
						
					BulletBehaviour bullet = sht.GetComponent<BulletBehaviour>();
					bullet.dir = d;
					bullet.source = damageDealer;
				}
					
				shootTimer = 2f;
			}
		}
			
		UpdateAngle();
	}
		
	void UpdateIdle() {
			
		if(player.transform.position.y > 3000) {

			pos = player.transform.position;
			pos.y += 1000f;
			pos.x += 500f;
			transform.position = pos;

			state = State.TRACKING;
			if(cannon != null)
				cannon.Shoot();
			decisionTimer = Random.Range(2f, 4f);
			targetOffset.x = Random.Range(-100f, 100f);
			targetOffset.y = Random.Range(-100f, 100f);
		}
	}
		
	void UpdateTracking() {
			
		Vector3 dir = pos + targetOffset - player.transform.position;
		dir.Normalize();
		targetPos = player.transform.position + dir * 300f;
		dir *= -1f;
			
		// Move limiting speed
		Vector3 p = Vector3.Lerp(pos, targetPos, 0.1f) - pos;
		pos += p;
			
		// Don't go into the ground
		RaycastHit2D ground = Physics2D.Raycast(pos, Vector3.down, 150f, groundMask);
		if(ground.collider != null) {
			pos.y = ground.point.y + 150f;
		}
			
		transform.position = pos;
			
		decisionTimer -= Time.deltaTime;
		if(decisionTimer <= 0f) {

			decisionTimer = Random.Range(1f, 4f);
			targetOffset.x = Random.Range(-100f, 100f);
			targetOffset.y = Random.Range(-100f, 100f);
		}
			
	}
		
	void UpdateAngle() {
			
		if(transform.localScale.x > 0) {
				
			if(pos.x < oldPos.x) {
				if(angle < 4f)
					angle += Time.deltaTime * 10f;
			}else if(pos.x > oldPos.x) {
				if(angle > -10f)
					angle -= Time.deltaTime * 20f;
			}else {
				angle *= 0.8f;
			}
		}else {
				
			if(pos.x > oldPos.x) {
				if(angle < 4f)
					angle += Time.deltaTime * 10f;
			}else if(pos.x < oldPos.x) {
				if(angle > -10f)
					angle -= Time.deltaTime * 20f;
			}else {
				angle *= 0.8f;
			}
		}
			
		transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
	}

	/*
		void OnCollisionEnter2D(Collision2D collision) {
			
			if (collision != null && collision.collider.GetComponent<DragonPlayer>() != null){
				collision.collider.GetComponent<DragonPlayer>().OnImpact(transform.position, 10f);
			}
			
		}
		*/
}
