using UnityEngine;
using System.Collections;

public class FlameParticle : MonoBehaviour {

	private float m_lifeTime = 5f;
	public float lifeTime { set { m_lifeTime = value; } }

	private float m_dyingTime = 0.25f;
	public float dyingTime { set { m_dyingTime = value; } }

	private float m_dyingSpeed = 3f;
	public float dyingSpeed { set { m_dyingSpeed = value; } }

	private Range m_finalScale = new Range(0.75f, 1.25f);
	public Range finalScale { set { m_finalScale = value; } }

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
		DYING
	}
	
	
	State state = State.INACTIVE;
	
	// Update is called once per frame
	void Update () {
		
		if (state == State.ACTIVE){			
			timer -= Time.deltaTime * speed;
			if (timer > 0) {
				float t = (1 - (timer / m_lifeTime));
				transform.position = mouthPosition.position +  dir * (distance * t);
				transform.localScale = Vector3.one * m_scaleCurve.Evaluate(t) * tscale;				
			} else {
				state = State.DYING;
				timer = m_dyingTime;
			}
		} else if (state == State.DYING){			
			timer -= Time.deltaTime * m_dyingSpeed;
			if (timer > 0) {
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
		timer = m_lifeTime;
		tscale = m_finalScale.GetRandom();
		speed = _speed;
		transform.Rotate(Vector3.forward * Random.Range(0f, 360f));

		m_scaleCurve = _scaleCurve;
		
		gameObject.SetActive(true);
		state = State.ACTIVE;
	}

}
