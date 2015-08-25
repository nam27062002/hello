using UnityEngine;
using System.Collections;

public class DragonGrabBehaviour : MonoBehaviour {

	public float m_grabTime = 5f;
	
	private GrabableBehaviour m_entity;
	
	private Transform m_mouth;
	private Animator m_animator;
	private DragonStats m_dragon;
	
	private float m_grabReleaseTimer = 0f;
	private float m_grabTimer = 0f; //wait a few seconds before trying to grab again
	
	
	// Use this for initialization
	void Start () {
		
		m_entity = null;
		
		m_mouth = transform.FindSubObjectTransform("eat");
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_dragon = GetComponent<DragonStats>();
	}
	
	
	public bool HasGrabbedEntity() {
		
		return m_entity != null;
	}

	public float WeightCarried() {

		if (m_entity == null)
			return 1;

		return m_entity.weight;
	}
	
	public void Grab(GrabableBehaviour other){
		
		if (enabled && m_entity == null) {
			if (m_grabTimer <= 0){
				m_entity = other;
				m_entity.Grab ();
				m_grabReleaseTimer = 0f;

				m_animator.SetBool("carrying_entity", true);
			}
		}
	}
	
	// Update is called once per frame
	void Update() {
		
		if (m_grabTimer > 0) {
			m_grabTimer -= Time.deltaTime;
			if (m_grabTimer < 0) {
				m_grabTimer = 0;
			}
		}
		
		if (m_entity != null) {
			// if not try to detect fly height
			float flyHeight = 250f;
			float minHeight = 400f;
			float maxHeight = 1500f / (m_entity.weight * 0.125f);
			float customGrabTime = Mathf.Min (1, m_grabTime / (m_entity.weight * 0.5f));
			
			RaycastHit ground;
			if (Physics.Linecast(transform.position, transform.position + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))) {

				flyHeight =  transform.position.y - ground.point.y;
			}
			
			//try and release by time
			m_grabReleaseTimer += Time.deltaTime;

			if (flyHeight > maxHeight || m_grabReleaseTimer > customGrabTime) {
				m_entity.Release(Vector3.up);//impulse);
				m_entity = null;
				m_grabTimer = 1f;
				
				m_animator.SetBool("carrying_entity", false);
			}
			
			if (flyHeight < 200f){

				Vector3 pos = transform.position;
				pos.y = ground.point.y + 200f;
				transform.position = pos;
			}
		}
	}
}
