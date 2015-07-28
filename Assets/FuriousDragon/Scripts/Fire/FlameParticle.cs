using UnityEngine;
using System.Collections;

public class FlameParticle : MonoBehaviour {

	public float life = 5f;
	public float initialSpeed = 200f;
	public float drag = 50f;

	float speed;
	float timer;
	float tscale;
	float cscale;
	Vector3 dir;
	Vector3 pos;
	float collisionDepth;
	int  frame = 0;
	Vector3 rot = new Vector3(0f,0f,1f);
	int fireMask;
	float flamePower = 1f;


	enum State{
		INACTIVE,
		ACTIVE,
		DIYING
	}
	
	
	State state = State.INACTIVE;
	
	// Update is called once per frame
	void Update () {
		
		if (state == State.ACTIVE){
			
			timer -= Time.deltaTime;
			if (timer > 0){
				
				speed -= drag*Time.deltaTime;
				if (speed < 0) speed = 0;
				
				pos += dir*(speed*Time.deltaTime);
				
				transform.position = pos;

				transform.Rotate(rot*200*Time.deltaTime);
				
				if (cscale < tscale){
					cscale += Time.deltaTime*15f;
					transform.localScale = Vector3.one*cscale;
				}

				frame++;
				if (frame >2){
					frame = 0;
					
					// Create a sphere at pos - (0, 0, collisionDepth/2) and move it to pos + (0, 0, collisionDepth/2), detecting collisions in the path
					RaycastHit[] hits = Physics.SphereCastAll(new Vector3(pos.x, pos.y, pos.z - collisionDepth/2f), 10, new Vector3(0, 0, 1), collisionDepth, fireMask);
					//Debug.DrawLine(new Vector3(pos.x, pos.y, pos.z - collisionDepth/2f), new Vector3(pos.x, pos.y, pos.z + collisionDepth/2f));
					foreach(RaycastHit hit in hits){
						if (hit.collider != null){
							
							// Burn whatever burnable
							FlamableBehaviour flamable =  hit.collider.GetComponent<FlamableBehaviour>();
							if (flamable != null){
								flamable.Burn (hit.point, flamePower);
							}
						}
					}
				}
				
			}else{
				state = State.DIYING;
				timer =1f;
			}
		}else if (state == State.DIYING){
			
			timer -= Time.deltaTime*3f;
			if (timer > 0){
				transform.localScale = Vector3.one*tscale*timer;
			}else{
				state = State.INACTIVE;
				gameObject.SetActive(false);
			}
		}
	}
	
	public void Activate(Vector3 position, Vector3 direction, float _fCollisionDepth, float firePower){
		
		transform.position = position;
		pos = position;
		dir = direction.normalized;
		collisionDepth = _fCollisionDepth;
		speed = initialSpeed+direction.magnitude;
		timer = life;
		tscale = Random.Range (1.75f,3f);
		cscale = 0.25f;
		transform.localScale = Vector3.one*cscale;
		transform.Rotate(rot*Random.Range(0f,360f));
		fireMask = 1 << LayerMask.NameToLayer("Edible") | 1 << LayerMask.NameToLayer("Burnable");
		flamePower = firePower;
		
		gameObject.SetActive(true);
		state = State.ACTIVE;
	}

}
