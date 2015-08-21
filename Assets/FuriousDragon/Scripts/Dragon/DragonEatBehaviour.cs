using UnityEngine;
using System.Collections;

public class DragonEatBehaviour : MonoBehaviour {

	[SerializeField] private float m_eatRange;

	private float m_eatingTimer;
	private float m_eatRangeSqr;

	private Transform m_mouth;
	private Animator m_animator;
	private DragonStats m_dragon;

	// Use this for initialization
	void Start () {
	
		m_eatingTimer = 0;
		m_eatRangeSqr = m_eatRange * m_eatRange;

		m_mouth = transform.FindSubObjectTransform("eat");
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_dragon = GetComponent<DragonStats>();
	}

	public bool IsEating() {

		return m_eatingTimer > 0;
	}

	// Update is called once per frame
	void Update() {
	
		if (m_eatingTimer > 0) {
			m_eatingTimer -= Time.deltaTime;
			if (m_eatingTimer < 0) {
				m_eatingTimer = 0;
				m_animator.SetBool("big_prey", false);
				m_animator.SetBool("bite", false);
			}
		}
	}

	void OnTriggerStay(Collider _other) {
			
		if (m_eatingTimer <= 0) {
			// Can object be eaten?
			EdibleBehaviour edible = _other.gameObject.GetComponent<EdibleBehaviour>();

			if(edible != null && edible.edibleFromType <= m_dragon.type) {
				
				Vector3 p1 = edible.transform.position;
				Vector3 p2 = m_mouth.position;
				
				p1.z = 0f;
				p1.y += edible.modelbounds.extents.y;
				p2.z = 0f;
				
				float distance = (p1 - p2).sqrMagnitude;
				
				// Is  within mouth range?
				if (distance < m_eatRangeSqr) {
					
					// Yes!! Eat it!
					edible.OnEat();
					
					// Give hp reward
					GameEntity entity = edible.GetComponent<GameEntity>();
					if(entity != null) {
						m_dragon.AddLife(entity.rewardHealth);
					}
					
					m_animator.SetBool("big_prey", edible.bigPrey);
					m_animator.SetBool("bite", true);

					m_eatingTimer = 0.5f;
				}
			}
		}
	}
}
