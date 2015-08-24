using UnityEngine;
using System.Collections;

public class MetroController : MonoBehaviour {



	float timer = 5f;
	public float start = 7619f;
	public float end = -10469f;

	Vector3 pos;

	public float speed = 1000f;

	enum State{
		RUNNING,
		WAITING
	};

	State state = State.WAITING;

	// Use this for initialization
	void Start () {
	
		pos = transform.position;
		timer = Random.Range (0f,5f);
	}
	
	// Update is called once per frame
	void Update () {

		if (state == State.WAITING){
		
			timer -= Time.deltaTime;
			if (timer <= 0f){
				state = State.RUNNING;
				pos.x = start;
				transform.position = pos;
			}
		}else{
			if (start > end){
				pos.x -= speed*Time.deltaTime;
				transform.position = pos;
				if (pos.x < end){
					timer = Random.Range (3f,6f);
					state = State.WAITING;
				}
			}else{
				pos.x += speed*Time.deltaTime;
				transform.position = pos;
				if (pos.x > end){
					timer = Random.Range (3f,6f);
					state = State.WAITING;
				}
			}
		}
	}
}
