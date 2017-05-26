using UnityEngine;
using System.Collections;

public class FlameParticle : MonoBehaviour {

	private float m_lifeTime = 5f;
	public float lifeTime { set { m_lifeTime = value; } }

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

	private PoolHandler m_poolHandler;

	void Start() {
		m_poolHandler = PoolManager.GetHandler(gameObject.name);
	}


	// Update is called once per frame
	void LateUpdate () {
		
		if (state == State.ACTIVE){			
			timer -= Time.deltaTime * speed;
			if (timer > 0) {
				float t = (1 - (timer / m_lifeTime));
				transform.position = mouthPosition.position +  dir * (distance * t);
				transform.localScale = Vector3.one * m_scaleCurve.Evaluate(t) * tscale;				
			} else {
				state = State.INACTIVE;
				gameObject.SetActive(false);
				m_poolHandler.ReturnInstance( gameObject );
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
