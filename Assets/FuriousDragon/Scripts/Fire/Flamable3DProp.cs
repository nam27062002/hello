using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Flamable3DProp : FlamableBehaviour {

	List<Transform> burnPoints = new List<Transform>();
	
	enum State{
		IDLE,
		BURNING,
		BURNT
	};
	
	
	State state = State.IDLE;
	float timer = 0f;
	
	Transform burnt;
	
	// Use this for initialization
	override protected void Initialize () {

		for(int i=0;i< transform.childCount;i++){
			Transform child = transform.GetChild(i);
			if (child.CompareTag("BurnPoint"))
				burnPoints.Add(child);
		}
		
		burnt = transform.FindChild("burnt");
		if (burnt)
			burnt.GetComponent<MeshRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (state == State.BURNING){
			
			timer += Time.deltaTime;
			if (timer >4){
				
				state = State.BURNT;
				if (burnt)
					burnt.GetComponent<MeshRenderer>().enabled = true;
				GetComponent<MeshRenderer>().enabled = false;
			}
		}
		
	}
	
	override protected void BurnImpl(Vector3 pos, float power){
		
		if (state == State.IDLE && hasBurned){
				
				state = State.BURNING;
				timer = 0f;
				
				FirePool pool = GameObject.Find ("FirePool").GetComponent<FirePool>();
				
				foreach(Transform t in burnPoints){
					
					GameObject fire = pool.GetParticle();
					if (fire){
						
						float delay = (t.position - pos).magnitude*0.002f;
						
						fire.GetComponent<FireParticle>().Burn(t.position,delay);
					}
				}
		}
	}
}
