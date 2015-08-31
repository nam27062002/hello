// DragonPlayer.cs
// Hungry Dragon
// 
// Created by Pere Alsina on 20/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR ----------------------------------------------
using UnityEngine;
using System.Collections;
#endregion

#region CLASSES ----------------------------------------------------------------
/// <summary>
/// Main control of a dragon
/// </summary>
public class DragonPlayer : MonoBehaviour {
	#region CONSTANTS ----------------------------------------------------------
	public enum EState {
		INIT,
		IDLE,
		BOOST,
		DYING,
		DEAD,
		DEAD_GORUND
	};

	#endregion

	#region EXPOSED MEMBERS ----------------------------------------------------
	[Header("Generic Settings")]
	public float speedDirectionMultiplier = 2f;
	public Range movementLimitX = new Range(-10000, 50000);
	public float grabTime = 5f;
	public float chargeDamage = 50f;
	
	[Header("Components")]
	public SkinnedMeshRenderer bodyMesh;
	public SkinnedMeshRenderer wingsMesh;
	#endregion

	#region PUBLIC MEMBERS -----------------------------------------------------
	[HideInInspector] public Rigidbody rbody;
	[HideInInspector] public int dragonType { get { return m_stats.type; } }
	[HideInInspector] public bool invulnerable = false;	// Debug purposes
	#endregion

	#region INTERNAL MEMBERS ---------------------------------------------------
	// Game objects
	DragonControl			controls;
	DragonBreathBehaviour 	fireBreath;
	Animator  				animator;
	DragonOrientation   	orientation;
	DragonEatBehaviour		eatBehaviour;
	DragonBreathBehaviour 	m_breathBehaviour;
	DragonGrabBehaviour		m_grabBehaviour;

	// Since stats are very frequently used and accessed from outside, do a shortcut getter for them
	private DragonStats		m_stats;
	public DragonStats		stats { get { return m_stats; }}

	// Control vars
	EState mState = EState.INIT;
	public EState state {
		get { return mState; }
	}
	float mStateTimer = 0f;

	// Movement
	Vector3 impulse;
	Vector3 dir = Vector3.right;
	Vector3 pos;

	bool 	allowBoost = true;
	bool	allowMovement = true;
	bool 	inWater = false;
	float   waterOriginY = 0f;
	float   speedMulti;
	float 	impulseMulti;
	float   glideTimer = 0f;
	float   sqrEatRange;


	bool isStarving = false;

	#endregion

	#region GENERIC METHODS ----------------------------------------------------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {

		// Initialize some internal vars
		controls = GetComponent<DragonControl>();
		fireBreath = GetComponent<DragonBreathBehaviour>();
		rbody = GetComponent<Rigidbody>();
		animator = transform.FindChild("view").GetComponent<Animator>();
		orientation = GetComponent<DragonOrientation>();
		pos = transform.position;
		impulseMulti = 4f;


		eatBehaviour = GetComponent<DragonEatBehaviour>();
		m_breathBehaviour = GetComponent<DragonBreathBehaviour>();
		m_grabBehaviour = GetComponent<DragonGrabBehaviour>();
		m_stats = GetComponent<DragonStats>();


		// Load selected skin
		// Load both materials
		/*
		Material bodyMat = Resources.Load<Material>("Materials/Dragon/MT_dragon_" + GameSettings.skinName);
		Material wingsMat = Resources.Load<Material>("Materials/Dragon/MT_dragon_" + GameSettings.skinName + "_alphaTest");
		if(bodyMat != null && wingsMat != null) {
			// Apply body materials
			bodyMesh.material = bodyMat;
			wingsMesh.material = wingsMat;
		}
		*/

		Application.targetFrameRate = 30;

		// Go to IDLE state
		ChangeState(EState.IDLE);

	}

	void OnDestroy(){

	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {

		// Check warning threshold
		float fThresholdValue = m_stats.maxLife * m_stats.lifeWarningThreshold;
		if(m_stats.life <= fThresholdValue && !isStarving) {
			// [AOC] Start warning threshold
			ToggleStarving(true);
		} else if(m_stats.life > fThresholdValue && isStarving) {
			// [AOC] Stop warning threshold
			ToggleStarving(false);
		}

		// Aux vars
	    allowMovement = false;
		speedMulti = 1f;

		// Do different stuff depending on dragon's state
		switch(mState) {

			case EState.IDLE: {

				// Movement allowed
				allowMovement = true;
			
				// Should we be firing?
				UpdateBoost(Input.GetKey(KeyCode.X) || controls.action);
				
				// Update life (unless invulnerable)
				if(m_stats.life > 0f && !IsInvulnerable()) {
					m_stats.AddLife(-Time.deltaTime * m_stats.lifeDrainPerSecond);

					// Have we died?
					if(m_stats.life <= 0) {
						ChangeState(EState.DYING);
					}
				}

				if (eatBehaviour.IsEating()) {
					allowMovement = true;					
					speedMulti = 0.3f;
				} else if (m_breathBehaviour.IsFuryOn()) {
					speedMulti = Mathf.Max ( speedMulti, 1.45f);
				}

			} break;
				
			case EState.DYING: {
				
				// Simulate gravity with values we like
				rbody.AddForce(Vector3.down*2000f);

				mStateTimer -= Time.deltaTime;
				if(mStateTimer < 0f) {
					ChangeState(EState.DEAD);
				}
			} break;
			case EState.DEAD:{
				// Simulate gravity with values we like
				rbody.AddForce(Vector3.down*4000f);

				// Detect ground hit
				RaycastHit ground;
				if (Physics.Linecast( transform.position, transform.position + Vector3.down * 150f, out ground, 1 << LayerMask.NameToLayer("Ground"))){
					ChangeState(EState.DEAD_GORUND);
				}

			} break;
			case EState.DEAD_GORUND:{
				
				rbody.velocity = Vector3.zero;
			} break;

			default: {
				
			} break;
		}



	}
	
	void FixedUpdate(){

		// Update dragon's movement
		if (inWater) {			

			float depth = Mathf.Min(1500f, (waterOriginY - transform.position.y));

			if (depth < 100) {
				animator.SetBool("swim",false);
				rbody.drag = 1.5f;	
				if(allowMovement) {
					UpdateMovement(speedMulti);
				}
			} else {
				Vector3 repulsion = Vector3.up * depth; 

				//if (speedMulti != 1f) {
					if (depth < 200) {
						rbody.drag = 1.5f;
						repulsion *= 4;
					} else {
						//rbody.drag = Mathf.Clamp(1.5f - ((depth - 200f) / 200f), 0, 1.5f);
						rbody.drag = 0;
					}
			//	}

				if (rbody.velocity.y > 0) {
				}

				//lets check if player wants to modify a bit the movement
				Vector3 impulse = controls.GetImpulse(m_stats.speed*speedMulti); 
				impulse.y = 0;
				
				impulse *= 0.5f;
				repulsion += impulse;

				rbody.AddForce(repulsion);

				animator.SetBool("fly",true);
				animator.SetBool("swim",true);				
			}
				orientation.SetDirection(rbody.velocity);
		} else if(allowMovement) {
			UpdateMovement(speedMulti);
		}
				
		// limit movement
		pos = transform.position;
		pos.z = 0;
		if (pos.x < movementLimitX.min){
			pos.x = movementLimitX.min;
		}else if (pos.x > movementLimitX.max){
			pos.x = movementLimitX.max;
		}else if (pos.y > 15000f){
			pos.y = 15000f;
		}
		transform.position = pos;
	}
	#endregion

	#region INTERNAL UTILS -----------------------------------------------------
	/// <summary>
	/// Change the current state of the dragon, perform any required actions based on
	/// current and new states.
	/// <para>Nothing will be done if state is the same as current one.</para>
	/// </summary>
	/// <param name="_eNewState">The state to change to.</param>
	void ChangeState(EState _eNewState) {
		// Ignore if state hasn't changed
		if(mState == _eNewState) return;

		// Do custom actions based on old and new states
		switch(_eNewState) {

			case EState.IDLE: {

			} break;

			case EState.DYING: {
				eatBehaviour.enabled = false;
				m_breathBehaviour.enabled = false;
				m_grabBehaviour.enabled = false;
				
				// Stop starving feedback as well
				ToggleStarving(false);
				
				// Notify the logic that the game has to end
				App.Instance.gameLogic.EndGame();	// [AOC] DEPRECATED
				Messenger.Broadcast(GameEvents.PLAYER_DIED);

				// Play death animation
				animator.SetTrigger ("dead");
				orientation.OnDeath();

				// Finish after some time
				mStateTimer = 1f;
			} break;

			case EState.DEAD: {
				// Open summary popup
				// [AOC] DEPRECATED!!
				GameObject popupsCanvas = GameObject.Find("CanvasPopups");
				if(popupsCanvas != null && popupsCanvas.GetComponent<UIPrefabLoader>() != null) {
					popupsCanvas.GetComponent<UIPrefabLoader>().InstantiatePrefab("Proto/Prefabs/UI/Popups/PF_PopupSummary");
				}
			} break;

			case EState.DEAD_GORUND: {
				animator.SetTrigger("dead_hit");
			}break;
		}

		// Store new state and notify game
		EState oldState = mState;	// Just for the event broadcast
		mState = _eNewState;
		Messenger.Broadcast<EState, EState>(GameEvents.PLAYER_STATE_CHANGED, oldState, _eNewState);
	}

	/// <summary>
	/// Group all the movement logic in here (to help with code readability).
	/// </summary>
	/// <param name="_fSpeedMultiplier">The multiplier of the dragon's movement speed.</param>
	void UpdateMovement(float _fSpeedMultiplier) {

		bool grab = m_grabBehaviour.HasGrabbedEntity();

		if (grab)
			_fSpeedMultiplier *= 1f / m_grabBehaviour.WeightCarried();

		impulse = controls.GetImpulse(m_stats.speed*_fSpeedMultiplier); 

		bool plummeting = (dir.y < -0.75f && rbody.velocity.y < -m_stats.speed*0.85f) || (_fSpeedMultiplier == m_stats.boostMultiplier  && rbody.velocity.magnitude > m_stats.speed*0.85f);
		plummeting = plummeting && !grab;

		bool flyUp = !plummeting && dir.y > 0.75f &&  _fSpeedMultiplier == 1f && !grab;

		if (!impulse.Equals(Vector3.zero)) {

			rbody.drag = 1.5f;

			Vector3 oldDir = dir; 
			dir = impulse;
			dir.Normalize();

			// Check sharp turn
			if (Vector3.Dot(oldDir, dir) < 0.5f && Mathf.Abs (dir.y) < 0.75f && Mathf.Abs (oldDir.y) < 0.75f){
				if (dir.x < 0f)
					animator.SetTrigger("turn_left");
				else
					animator.SetTrigger("turn_right");

				rbody.velocity = Vector3.zero;
				rbody.drag = 10f;
			}

		} else {
			rbody.drag = Mathf.Min(10f, rbody.drag + 1.5f);
			plummeting = false;
		}

		animator.SetBool("plummet",plummeting);
		animator.SetBool("flight_up",flyUp);

		// This is manual physics, we don't use it because we can't do collisions well with this
		//transform.position += impulse * Time.fixedDeltaTime;

		if (_fSpeedMultiplier != 1.0f){
			impulseMulti  = Mathf.Lerp(impulseMulti,4f,0.05f);
			rbody.AddForce(impulse*impulseMulti);
		}else{
			float multi = 1f;
			if (dir.y > 0f)
				multi = 0.6f+0.4f*(1.0f-dir.y);
			else
				multi = 1f+1f*(-dir.y);

			impulseMulti  = Mathf.Lerp(impulseMulti,4f*multi,0.05f);
			rbody.AddForce(impulse*impulseMulti);
		}
		//rbody.velocity = impulse;
		rbody.angularVelocity = Vector3.zero;

		// decide if flying
		bool flying = controls.moving;

		// use fly animation if flying, but use idle animation if carrying something
		animator.SetBool("fly", flying); 

		if(flying) {
			if (!grab){ // Orientation with something grabbed is limited
				orientation.SetDirection(impulse);
			}else{
				Vector3 grabOrientation = impulse;
				grabOrientation.y = 0;
				orientation.SetDirection(grabOrientation);
			}
			glideTimer += Time.deltaTime;
			if (glideTimer > 6f)
				glideTimer = 0f;
		} else {
			if (dir.x < 0)
				dir = Vector3.left;
			else 
				dir = Vector3.right;
			orientation.SetDirection(dir);
		}

		// Glide timer controls how long we do the glide animation
		// we set it to zero if flying too flat or we need a diferent animation, like idle or plummeting
		if (!flying || plummeting || dir.y > 0.65f){
			glideTimer = 0f;
		}

		animator.SetFloat ("glide_time",glideTimer);
	}

	void UpdateBoost(bool activate){
		
		if (activate && allowBoost){

			m_stats.AddEnergy(-Time.deltaTime*m_stats.energyDrainPerSecond);
			
			speedMulti=m_stats.boostMultiplier;
			
			if (m_stats.energy <= 0f) {
				allowBoost = false;
			}
		}else{
			
			if (m_stats.energy > 0f && !activate)
				allowBoost = true;

			if (!activate){
				m_stats.AddEnergy(Time.deltaTime*m_stats.energyRefillPerSecond);
			}
		}
	}

	/// <summary>
	/// Enable/Disable the "starving" status.
	/// </summary>
	/// <param name="_bIsStarving">Whether the dragon is starving or not.</param>
	void ToggleStarving(bool _bIsStarving) {
		// Play/Stop animation
		animator.SetBool("is_starving", _bIsStarving);

		// Send game event
		Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, _bIsStarving);

		//
		isStarving = _bIsStarving;
	}
	#endregion

	#region PUBLIC METHODS -----------------------------------------------------
	/// <summary>
	/// Apply damage to the dragon.
	/// </summary>
	/// <param name="_damage">Amount of damage to be applied.</param>
	/// <param name="_source">The source of the damage.</param>
	public void ApplyDamage(float _damage, DamageDealer _source) {
		// Ignore if dragon is not alive
		if(!IsAlive()) return;

		// Ignore if dragon is invulnerable
		if(IsInvulnerable()) return;
		
		// Apply damage
		m_stats.AddLife(-_damage);
			
		// Have we died?
		if(m_stats.life <= 0) {
			ChangeState(EState.DYING);
		}

		// Notify the game
		Messenger.Broadcast<float, DamageDealer>(GameEvents.PLAYER_DAMAGE_RECEIVED, _damage, _source);
	}

	/// <summary>
	/// Pretty straightforward.
	/// </summary>
	/// <param name="_force">The force vector to be applied.</param>
	public void ApplyForce(Vector3 _force) {
		// Just do it
		rbody.AddForce(_force);
	}

	/// <summary>
	/// Stop dragon's movement
	/// </summary>
	public void Stop() {
		rbody.velocity = Vector3.zero;
	}
	#endregion

	#region GETTERS ------------------------------------------------------------
	/// <summary>
	/// Is the dragon alive?
	/// </summary>
	/// <returns><c>true</c> if the dragon is not dead or dying; otherwise, <c>false</c>.</returns>
	public bool IsAlive() {
		return mState < EState.DYING;
	}

	/// <summary>
	/// Is the dragon starving?
	/// </summary>
	/// <returns><c>true</c> if the dragon is alive and its current life under the specified warning threshold; otherwise, <c>false</c>.</returns>
	public bool IsStarving() {
		return IsAlive() && (m_stats.life > m_stats.maxLife * m_stats.lifeWarningThreshold);
	}

	/*
	/// <summary>
	/// Whether the dragon is firing or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon is currently throwing fire; otherwise, <c>false</c>.</returns>
	public bool IsFiring() {
		return allowFire;	// [AOC] This may only be ok in Frenzy mode
	}
	*/

	/// <summary>
	/// Whether the dragon can take damage or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon currently is invulnerable; otherwise, <c>false</c>.</returns>
	public bool IsInvulnerable() {
		// During fire, we're invulnerable
		if(m_breathBehaviour.IsFuryOn()) return true;

		// If cheat is enable
		if(invulnerable) return true;

		// All checks passed, we're not invulnerable
		return false;
	}

	/// <summary>
	/// Gets the direction.
	/// </summary>
	/// <returns>The direction.</returns>
	public Vector3 GetDirection(){

		return dir;
	}
	#endregion
	
	#region CALLBACKS ----------------------------------------------------------

	void OnTriggerEnter(Collider other) {

		if (other.tag == "Water" && !inWater) {
			inWater = true;
			waterOriginY = other.transform.position.y;

			//Add particles
		}
	}

	void OnTriggerExit(Collider other) {
		
		if (other.tag == "Water") {
			inWater = false;
			rbody.drag = 1.5f;
			animator.SetBool("swim",false);
		}
	}



	/// <summary>
	/// Collision with another object
	/// </summary>
	/// <param name="other">The object which we've collided with.</param>
	void OnTriggerStay(Collider other) {

		if (other.tag == "Water") {
			inWater = true;
			return;
		}
	}

	void OnCollisionEnter(Collision collision) {

		float hitAngle = Vector3.Angle(rbody.velocity, Vector3.right);

		if (hitAngle >= 45f) {
			HittableBehaviour hit = collision.gameObject.GetComponent<HittableBehaviour>();
			if (hit != null){
				float finalDamage = (chargeDamage*rbody.velocity.magnitude)/(m_stats.speed*m_stats.boostMultiplier);
				hit.OnHit(finalDamage);

				//ContactPoint contact =  collision.contacts[0];
				//Vector3 reflection = Vector3.Reflect(dir,contact.normal).normalized;
				//rbody.AddForce(reflection*12500f);
				//orientation.SetDirection(rbody.velocity);

				Stop();
			}
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="_origin">Origin.</param>
	/// <param name="_damage">Damage.</param>
	/// <param name="_intensity">Intensity.</param>
	/// <param name="_source">_source.</param>
	public void OnImpact(Vector3 _origin, float _damage, float _intensity, DamageDealer _source){
		// Ignore if dragon is not alive
		if(!IsAlive()) return;

		// Apply force
		if(_intensity != 0f) {
			Vector3 impactDir = (transform.position - _origin).normalized;
			ApplyForce(impactDir * _intensity);
		}

		// Apply damage
		ApplyDamage(_damage, _source);

		// Shake camera
		Camera.main.GetComponent<CameraController_OLD>().Shake();
	}

	#endregion
}
#endregion
