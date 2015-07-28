using UnityEngine;
using System.Collections;

public class FireParticle : MonoBehaviour {

	public float life = 5f;
	public float initialSpeed = 200f;
	public float drag = 50f;

	float speed;
	float timer;
	float tscale;
	float cscale;
	Vector3 dir;
	Vector3 pos;
	int  frame = 0;

	RaycastHit2D ground;
	Vector3 groundPos = Vector3.zero;

	enum State{
		INACTIVE,
		ACTIVE,
		DIYING,
		GROUND,
		BURN_DELAY,
		BURN
	}


	State state = State.INACTIVE;

	// Update is called once per frame
	void Update () {
	
		if (state == State.ACTIVE){

			if (cscale < tscale){
				cscale += Time.deltaTime*5f;
				transform.localScale = Vector3.one*cscale;
			}
		}else if (state == State.DIYING){

			timer -= Time.deltaTime*2f;
			if (timer > 0){
				transform.localScale = Vector3.one*tscale*timer;
			}else{
				state = State.INACTIVE;
				gameObject.SetActive(false);
			}
		}else if (state == State.GROUND){

			timer -= Time.deltaTime;
			if (cscale < tscale){
				cscale += Time.deltaTime*10f;
				transform.localScale = Vector3.one*cscale;
			}
			if (timer < 0){
				state = State.DIYING;
				timer = 1f;
			}
		}
		else if (state == State.BURN_DELAY){
			
			timer -= Time.deltaTime;
			if (timer < 0){
				state = State.BURN;
				timer = 3f;
			}
		}
		else if (state == State.BURN){
			
			timer -= Time.deltaTime;
			if (cscale < tscale){
				cscale += Time.deltaTime*5f;
				transform.localScale = Vector3.one*cscale;
			}
			if (timer < 0){
				state = State.DIYING;
				timer = 1f;
			}
		}
	}

	public void Activate(){
	
		state = State.ACTIVE;

		tscale = Random.Range (0.5f,1f);
		cscale = 0f;
		transform.localScale = Vector3.zero;
		
		gameObject.SetActive(true);
		
		float animspeed = Random.Range(0.8f,1.2f);
		GetComponent<Animator>().speed = animspeed;
		transform.FindChild ("AdditiveParticle").GetComponent<Animator>().speed = animspeed;

	}
	

	public void Burn(Vector3 position, float delay = 0){
		
		state = State.BURN_DELAY;
		timer = delay;

		if (timer > 0.75f) 
			timer = 0.75f;

		position.y -= 5;
		tscale = Random.Range (1f,2f);
		cscale = 0f;
		transform.position = position;
		transform.localScale = Vector3.zero;
		
		gameObject.SetActive(true);

		float animspeed = Random.Range(0.8f,1.2f);
		GetComponent<Animator>().speed = animspeed;
		transform.FindChild ("AdditiveParticle").GetComponent<Animator>().speed = animspeed;

	}
	
}
