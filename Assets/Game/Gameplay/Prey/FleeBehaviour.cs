using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SensePlayer))]
public class FleeBehaviour : Initializable {

	private enum State {
		None = 0,
		Evade,
		RunAway, // flee from the dragon
		Afraid,  // the prey is scared and can't move anymore
		Panic    // the prey reached the end of the area or map and runs to the other side in panic
	};

	public bool m_fleeAround = false;
	[CommentAttribute("This prey stops and plays a special animation. Can be easily eaten.")]
	[SerializeField] private bool m_canBeAfraid = false;
	[CommentAttribute("This prey can't move away from the dragon in this direction so it tries to escape going back to the center of their movement area.")]
	[SerializeField] private bool m_canPanic = false;

	[CommentAttribute("This prey will flee away from the dragon running only for a limited amount of time. Then it'll keep fleeing away but at normal speed.")]
	[SerializeField] private bool m_limitRunTime = false;
	[SerializeField] private Range m_runningTimeRange = new Range(0f, 0f);
	[SerializeField] private Range m_waitRunningRange = new Range(0f, 0f);

	private Transform m_dragonMouth;
			
	private PreyMotion m_motion;
	private Animator m_animator;
	private SensePlayer m_sensor;
	private FlockBehaviour m_flock;

	private Vector2 m_panicTarget;
	
	private State m_state;
	private State m_nextState;

	private float m_runningTime = 0;
	private float m_nextRunTime = 0;

	public List<string> m_afraidSounds = new List<string>();

	// dirty and fast implementation of sound for running away groups
	private Entity m_entity;
	static Dictionary<string, int> m_runningAway = new Dictionary<string,int>();

	// Use this for initialization
	void Awake () {
		m_motion = GetComponent<PreyMotion>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_sensor = GetComponent<SensePlayer>();
		m_flock = GetComponent<FlockBehaviour>();
		m_entity = GetComponent<Entity>();
	}

	void Start() {
		m_dragonMouth = InstanceManager.player.GetComponent<DragonMotion>().tongue;
	}
	
	public override void Initialize() {
		m_state = State.None;
		m_nextState = State.Evade;
		m_runningTime = 0;
		m_nextRunTime = 0;
	}

	void OnEnable() {
		m_state = State.None;
		m_nextState = State.Evade;
	}

	void OnDisable() {
		m_nextState = State.None;
		ChangeState();
	}

	void Update() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		if (m_state == State.Evade) {
			if (m_sensor.alert) {
				m_nextState = State.RunAway;
			}
		} else if (m_state == State.RunAway) {
			if (m_canBeAfraid || m_canPanic) {
				if (m_sensor.alert && !m_area.Contains(m_motion.position)) {
					if (m_canBeAfraid) {
						m_nextState = State.Afraid;
					} else {
						m_nextState = State.Panic;
					}
				}
			}
		} else if (m_state == State.Panic) {			
			if (!m_sensor.alert || m_motion.lastSeekDistanceSqr < m_motion.slowingRadius * m_motion.slowingRadius) {
				m_nextState = State.RunAway;
			}
		}

		if (!m_sensor.isInsideMaxArea) {
			m_nextState = State.Evade;
		}
	}

	// Update is called once per frame
	void FixedUpdate() {
		switch (m_state) {
			case State.Evade:
				m_motion.Flee(m_dragonMouth.position, false);
				break;
				
			case State.RunAway: {					
				bool run = true;

				if (m_limitRunTime) {
					if (m_runningTime > 0) {
						m_runningTime -= Time.deltaTime;
					} else {
						m_nextRunTime -= Time.deltaTime;
						if (m_nextRunTime <= 0) {
							m_runningTime = m_runningTimeRange.GetRandom();
							m_nextRunTime = m_waitRunningRange.GetRandom();
						}
						run = false;
					}
				}

				m_motion.Flee(m_dragonMouth.position, run);
			}	break;

			case State.Afraid:
				Vector3 player = m_dragonMouth.position;
				if (player.x < m_motion.position.x) {
					m_motion.direction = Vector2.left;
				} else {
					m_motion.direction = Vector2.right;
				}
				m_motion.Stop();
				break;

			case State.Panic:
				m_motion.RunTo(m_panicTarget);
				break;
		}
	}

	private void ChangeState() {
		// exit State
		switch (m_state) {
			case State.RunAway:
				m_animator.SetBool("move", false);
				m_runningAway[ m_entity.sku ]--;
				break;
				
			case State.Afraid:
				m_animator.SetBool("scared", false);
				break;

			case State.Panic:
				m_animator.SetBool("move", false);
				break;
		}
		
		// enter State
		switch (m_nextState) {
			case State.RunAway:
			{
				m_animator.SetBool("move", true);
				if (m_runningAway.ContainsKey(m_entity.sku))
				{
					m_runningAway[ m_entity.sku ]++;
				}
				else
				{
					m_runningAway.Add(m_entity.sku, 1);	
				}

				CheckRunAwaySound();

			}break;
				
			case State.Afraid:
			{
				m_animator.SetBool("scared", true);
				if (m_afraidSounds.Count > 0)
				{
					// Play sound!
					string soundName = m_afraidSounds[ Random.Range( 0, m_afraidSounds.Count ) ];
					if (!string.IsNullOrEmpty( soundName ))
					{
						AudioManager.instance.PlayClip( soundName );
					}
				}
				m_motion.Stop();
			}break;

			case State.Panic:
				m_animator.SetBool("move", true);
				m_motion.Stop();

				m_panicTarget = m_motion.ProjectToGround(m_area.center);
				break;
		}
		
		m_state = m_nextState;
	}


	/// <summary>
	/// Checks the run away sound. Checks if enough entities are running away to play a sound. 
	/// This should be done somwhere else. Maybe inside Ambient Sound Manager registering the states of each entity and having information from content
	/// </summary>
	private void CheckRunAwaySound()
	{
		// We should add this to content or do it somwhere else
		switch( m_entity.sku )
		{
			case "canary":
			{
				if ( m_runningAway[ m_entity.sku ] == 4 )	// play sound
				{
					switch( Random.Range(0,2) )
					{
						case 0:AudioManager.instance.PlayClip("audio/sfx/Animals/Air/PigeonsFly");break;
						case 1:AudioManager.instance.PlayClip("audio/sfx/Animals/Air/Amb_Rnd_PerchedBirdsFlee_02");break;
					}
				}
			}break;
			case "starling":
			{
				if ( m_runningAway[ m_entity.sku ] == 4 )	// play sound
				{
					switch( Random.Range(0,2) )
					{
						case 0:AudioManager.instance.PlayClip("audio/sfx/Animals/Air/PigeonsFly");break;
						case 1:AudioManager.instance.PlayClip("audio/sfx/Animals/Air/Amb_Rnd_PerchedBirdsFlee_02");break;
					}
				}
			}break;
		}
	}
}
