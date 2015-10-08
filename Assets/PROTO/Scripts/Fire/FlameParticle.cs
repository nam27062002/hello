using UnityEngine;
using System.Collections;

public class FlameParticle : MonoBehaviour {

	public float life = 5f;
	public float initialSpeed = 1f;
	public float drag = 0.25f;

	float distance;
	float timer;
	float tscale;
	float speed;
	Vector3 dir;

	AnimationCurve m_scaleCurve;

	Transform mouthPosition;


	enum State{
		INACTIVE,
		ACTIVE,
		DIYING
	}
	
	
	State state = State.INACTIVE;
	
	// Update is called once per frame
	void Update () {
		
		if (state == State.ACTIVE){
			
			timer -= Time.deltaTime * speed;
			if (timer > 0){
				float t = (1 - (timer / (life * 3f)));
				transform.position = mouthPosition.position +  dir * (distance * t);
				transform.localScale = Vector3.one * m_scaleCurve.Evaluate(t) * tscale;
				
			} else {
				state = State.DIYING;
				timer =1f;
			}
		} else if (state == State.DIYING){
			
			timer -= Time.deltaTime * 3f;
			if (timer > 0){
				transform.localScale = Vector3.one * tscale * timer;
			} else {
				state = State.INACTIVE;
				gameObject.SetActive(false);
			}
		}
	}
	
	public void Activate(Transform _mouth, Vector3 direction, float _speed, AnimationCurve _scaleCurve){

		mouthPosition = _mouth;
		dir = direction.normalized;
		distance = direction.magnitude;
		timer = life * 3f;
		tscale = Random.Range (0.75f, 1.25f);
		speed = _speed;
		transform.Rotate(Vector3.forward * Random.Range(0f,360f));

		m_scaleCurve = _scaleCurve;
		
		gameObject.SetActive(true);
		state = State.ACTIVE;
	}

}
