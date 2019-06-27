using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IFireNode : IQuadTreeItem {
	BoundingSphere boundingSphere { get; }
	CircleAreaBounds area { get; }
	void UpdateLogic();
	void SetEffectVisibility(bool _visible);
	void Burn(Vector2 _direction, bool _dragonBreath, DragonTier _tier, DragonBreathBehaviour.Type _breathType, IEntity.Type _source, FireColorSetupManager.FireColorType _fireColorType);
}

public class FireNode : MonoBehaviour, IFireNode {
	private enum State {
		Idle = 0,
		Spreading,
		Burning,
		GoingToExplode,
		Extinguish,
		Extinguished
	};

	private Decoration m_decoration;
	private InflammableDecoration m_parent;
	private Transform m_transform;

	private ParticleData m_feedbackParticle;
	private ParticleData m_burnParticle;
	private FireProcController m_fireSprite;

	private bool m_feedbackParticleMatchDirection = false;
	private float m_hitRadius = 0f;

	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	private BoundingSphere m_boundingSphere;
	public BoundingSphere boundingSphere { get { return m_boundingSphere; } }

	private CircleAreaBounds m_area;
	public CircleAreaBounds area { get { return m_area; } }

	private DragonTier m_breathTier;
	public DragonTier breathTier { get{ return m_breathTier; } }

	private IEntity.Type m_sourceType = IEntity.Type.OTHER;
	private IEntity.Type sourceType { get { return m_sourceType; }}
	private DragonBreathBehaviour.Type m_breathType;
    private FireColorSetupManager.FireColorType m_colorType;
    public FireColorSetupManager.FireColorType colorType { get { return m_colorType; }}

	private List<FireNode> m_neighbours;
	private List<float> m_neihboursFireResistance;

	private float m_timer;
	private float m_powerTimer;

	private State m_state;
	private State m_nextState;



	// Use this for initialization
	void Awake() {
		m_transform = transform;
		m_boundingSphere = new BoundingSphere(m_transform.position, 8f * m_transform.localScale.x);
	}

	void Start() {
		gameObject.SetActive(false);
	}

	void OnDestroy() {
		if (ApplicationManager.IsAlive) {
			FirePropagationManager.UnregisterBurningNode (this);
		}
	}

	public void Init(InflammableDecoration _parent, Decoration _decoration, ParticleData _burnParticle, ParticleData _feedbackParticle, bool _feedbackParticleMatchDirection, float _hitRadius) {		
		m_decoration = _decoration;
		m_parent = _parent;

		m_burnParticle = _burnParticle;
		m_feedbackParticle = _feedbackParticle;

		m_feedbackParticleMatchDirection = _feedbackParticleMatchDirection;
		m_hitRadius = _hitRadius;

		Reset();

		m_rect = new Rect((Vector2)m_transform.position - Vector2.one * m_hitRadius, Vector2.one * m_hitRadius * 2f);
		m_area = new CircleAreaBounds(m_transform.position, m_hitRadius);

		FirePropagationManager.Insert(this);
	}

	public void Reset() {
		StopFireEffect();

		m_state = State.Idle;
		m_nextState = m_state;

		if (m_neighbours == null) {
			FindNeighbours();
		}
		SetNeighboursDistance();

		m_timer = 0f;
	}

	public void Disable() { 
		StopFireEffect();
		FirePropagationManager.UnregisterBurningNode(this);
		m_state = m_nextState = State.Extinguished; 
	}

	public bool IsSpreadingFire() 	{ return m_state == State.Spreading;  		}
	public bool IsBurning() 		{ return m_state == State.Burning; 	  		}
	public bool IsGoingToExplode()  { return m_state == State.GoingToExplode; 	}
	public bool IsExtinguishing() 	{ return m_state == State.Extinguish; 		}
	public bool IsExtinguished() 	{ return m_state == State.Extinguished;		}


	public void Burn(Vector2 _direction, bool _dragonBreath, DragonTier _tier, DragonBreathBehaviour.Type _breathType, IEntity.Type _source, FireColorSetupManager.FireColorType _fireColorType =  FireColorSetupManager.FireColorType.RED ) {
		if (m_state == State.Idle) {
			ZoneManager.ZoneEffect effect = ZoneManager.ZoneEffect.None; 
			m_breathTier = _tier;
			m_breathType = _breathType;
			m_sourceType = _source;
            m_colorType = _fireColorType;

			if (_breathType == DragonBreathBehaviour.Type.Mega) {
				effect = InstanceManager.zoneManager.GetSuperFireEffectCode(m_decoration, _tier);
			} else {
				effect = InstanceManager.zoneManager.GetFireEffectCode(m_decoration, _tier);
			}

			if (effect >= ZoneManager.ZoneEffect.M) {
				FirePropagationManager.RegisterBurningNode(this);

				if (effect == ZoneManager.ZoneEffect.L) {
					m_nextState = State.GoingToExplode;
					m_parent.LetsBurn(true, m_sourceType, _fireColorType);
				} else {
					m_nextState = State.Spreading;
					m_parent.LetsBurn(false, m_sourceType, _fireColorType);
				}
			} else {
				// Dragon can't burn this thing, so lets put a few feedback particles
				if (_dragonBreath && m_timer <= 0f) {
					GameObject hitParticle = m_feedbackParticle.Spawn(m_transform.position);
					if (hitParticle != null && m_feedbackParticleMatchDirection) {
						Vector3 angle = (_direction.x < 0)? Vector3.down : Vector3.up;

						hitParticle.transform.rotation = Quaternion.Euler(angle * 90f);
					}
					m_timer = 0.5f;
				}
			}
		}
	}

	public void Extinguish() {
		if (m_state > State.Idle) {
			m_nextState = State.Extinguish;
		}
	}

	public void Explode() {
		StopFireEffect();
		FirePropagationManager.UnregisterBurningNode(this);
		m_state = State.Extinguished;
	}

	public void UpdateLogic() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		float dt = Time.deltaTime * (1f + ((int)m_breathTier * 0.4f));

		if (m_powerTimer < 1f) {
			m_powerTimer += dt;
			if (m_powerTimer > 1f) {
				m_powerTimer = 1f;
			}
		}

		switch (m_state) {
			case State.Spreading:						
				if (m_fireSprite != null) {
					m_fireSprite.SetPower(m_powerTimer * 6f);
				}

				bool allBurned = true;
				for (int i = 0; i < m_neihboursFireResistance.Count; i++) {
					if (m_neighbours[i].IsSpreadingFire() || m_neighbours[i].IsBurning()) {
						m_neihboursFireResistance[i] = 0;
					} else {
						allBurned = false;
						if (m_neihboursFireResistance[i] > 0.1f) {
							m_neihboursFireResistance[i] *= 0.5f;						
						} else {
							m_neighbours[i].Burn(Vector2.zero, false, m_breathTier, m_breathType, m_sourceType, m_colorType);
						}
					}
				}

				if (allBurned) {
					m_nextState = State.Burning;
				}
				break;

			case State.Burning:
				if (m_fireSprite != null) {
					m_fireSprite.SetPower(m_powerTimer * 6f);
				}
				break;

			case State.Extinguish:				
				if (m_fireSprite != null) {
					m_fireSprite.SetPower(6f + (m_powerTimer * (-6f)));
				}

				if (m_timer > 0) {
					m_timer -= dt;
					if (m_timer <= 0) {
						StopFireEffect();

						FirePropagationManager.UnregisterBurningNode(this);

						m_state = State.Extinguished;
					}
				}
				break;
		}
	}

	private void ChangeState() {
		switch (m_nextState) {
			case State.Spreading:
				m_powerTimer = 0f;
				StartFireEffect();
				break;

			case State.Burning:
				break;

			case State.Extinguish:
				m_powerTimer = 0f;
				m_timer = 6f;
				break;
		}

		m_state = m_nextState;
	}

	public void SetEffectVisibility(bool _visible) {
		if (m_state >= State.Spreading && m_state <= State.Extinguish) {
			if (_visible) {			
				StartFireEffect();
			} else {
				StopFireEffect();
			}
		}
	}

	private void StartFireEffect() {
		// Not used at the moment
		// FirePropagationManager.PlayBurnAudio();
		if (m_fireSprite == null) {
			GameObject go = m_burnParticle.Spawn(m_transform.position);

			if (go != null) {
				m_fireSprite = go.GetComponent<FireProcController>();
				m_fireSprite.m_colorSelector.m_fireType = m_colorType;
				m_fireSprite.transform.localScale = m_transform.localScale * Random.Range(0.9f, 1.1f);

				if (m_state == State.Spreading) {
					m_fireSprite.SetPower(m_powerTimer * 6f);				
				} else if (m_state == State.Extinguish) {
					m_fireSprite.SetPower(6f + (m_powerTimer * (-6f)));
				}
			}
		}
	}

	private void StopFireEffect() {
		// Not used at the moment
		// FirePropagationManager.StopBurnAudio();
		if (m_fireSprite != null) {
			m_fireSprite.gameObject.SetActive(false);
			m_burnParticle.ReturnInstance(m_fireSprite.gameObject);
		}
		m_fireSprite = null;
	}

	private void FindNeighbours() {
		m_neighbours = new List<FireNode>();
		m_neihboursFireResistance = new List<float>();
		FireNode[] nodes = m_transform.parent.GetComponentsInChildren<FireNode>(true);
		
		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null && nodes[i] != this) {
				m_neighbours.Add(nodes[i]);
				m_neihboursFireResistance.Add(0);
			}
		}
	}

	private void SetNeighboursDistance() {
		for (int i = 0; i < m_neighbours.Count; i++) {
			m_neihboursFireResistance[i] = Vector3.SqrMagnitude(m_transform.position - m_neighbours[i].transform.position);
		}
	}


	//------------------------------------------------------------------------------
	public void OnDrawGizmosSelected() {
		Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.5f);
		Gizmos.DrawSphere(transform.position, 0.5f);

		Gizmos.color = Colors.fuchsia;
		Gizmos.DrawWireSphere(transform.position, m_hitRadius);
	
		if (m_transform == null) {
			m_transform = transform;
		}

		FindNeighbours();
		
		for (int i = 0; i < m_neighbours.Count; i++) {
			Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.15f);
			Gizmos.DrawSphere(m_neighbours[i].transform.position, 0.5f);
		}
	}
}
