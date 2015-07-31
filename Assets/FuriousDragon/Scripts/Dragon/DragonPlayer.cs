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
		EATING,
		BOOST,
		DYING,
		DEAD,
		DEAD_GORUND
	};

	public enum EFireMode{
		BOOST,
		HOMING,
		RAY,
		FRENZY
	};

	public static EFireMode fireMode =  EFireMode.FRENZY;
	#endregion

	#region EXPOSED MEMBERS ----------------------------------------------------
	[Header("Generic Settings")]
	[Range(0, 1)] public int dragonType = 0;  
	public float dragonSpeed = 100f;
	public float speedDirectionMultiplier = 2f;
	public float boostMultiplier = 2.5f;
	public float eatRange = 80f;
	public Range movementLimitX = new Range(-10000, 10000);

	[Header("Life")]
	public float maxLife = 100f;
	public float lifeDrainPerSecond = 10f;
	public float lifeWarningThreshold = 0.2f;	// Percentage of maxLife

	[Header("Energy")]
	public float maxEnergy = 50f;
	public float energyDrainPerSecond = 10f;
	public float energyRefillPerSecond = 25f;
	public float energyMinRequired = 25f;

	[Header("Components")]
	public SkinnedMeshRenderer bodyMesh;
	public SkinnedMeshRenderer wingsMesh;
	#endregion

	#region PUBLIC MEMBERS -----------------------------------------------------
	[HideInInspector] public float energy;
	[HideInInspector] public Rigidbody rbody;

	private float mLife;
	[HideInInspector] public float life {
		get { 
			return mLife; 
		}
		set { 
			// Check warning threshold
			float fThresholdValue = maxLife * lifeWarningThreshold;
			if(mLife > fThresholdValue && value <= fThresholdValue) {
				// [AOC] Start warning threshold
				ToggleStarving(true);
			} else if(mLife <= fThresholdValue && value > fThresholdValue) {
				// [AOC] Stop warning threshold
				ToggleStarving(false);
			}
			mLife = Mathf.Min(value, maxLife);
		}
	}

	[HideInInspector] public bool invulnerable = false;	// Debug purposes
	#endregion

	#region INTERNAL MEMBERS ---------------------------------------------------
	// Game objects
	TouchControlsDPad	touchControls;
	DragonFireInterface fireBreath;
	Animator  			animator;
	DragonOrientation   orientation;
	Transform    		mouth;

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
	bool    allowFire = true;
	bool 	allowBoost = true;
	bool	allowMovement = true;
	float   speedMulti;
	float 	impulseMulti;
	float   glideTimer = 0f;
	float   sqrEatRange;
	GrabableBehaviour grab = null;

	#endregion

	#region GENERIC METHODS ----------------------------------------------------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {

		// Initialize some internal vars
		touchControls = GameObject.Find("PF_GameInput").GetComponent<TouchControlsDPad>();
		fireBreath = GetComponent<DragonFireInterface>();
		rbody = GetComponent<Rigidbody>();
		animator = transform.FindChild("view").GetComponent<Animator>();
		orientation = GetComponent<DragonOrientation>();
		mouth = transform.FindSubObjectTransform("eat");
		sqrEatRange = eatRange*eatRange;
		life = maxLife;
		energy = maxEnergy;
		pos = transform.position;
		impulseMulti = 4f;

		// Load selected skin
		// Load both materials
		Material bodyMat = Resources.Load<Material>("Proto/Materials/Dragon/MT_dragon_" + GameSettings.skinName);
		Material wingsMat = Resources.Load<Material>("Proto/Materials/Dragon/MT_dragon_" + GameSettings.skinName + "_alphaTest");
		if(bodyMat != null && wingsMat != null) {
			// Apply body materials
			bodyMesh.material = bodyMat;
			wingsMesh.material = wingsMat;
		}

		Application.targetFrameRate = 30;

		// Go to IDLE state
		ChangeState(EState.IDLE);

		allowFire = fireMode != EFireMode.FRENZY;

		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);

	}

	void OnDestroy(){

		Messenger.RemoveListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {

		// Aux vars
	    allowMovement = false;
		speedMulti = 1f;

		// Do different stuff depending on dragon's state
		switch(mState) {

			case EState.IDLE: {

				// Movement allowed
				allowMovement = true;
			
				// Should we be firing?
				UpdateFire(Input.GetKey(KeyCode.X) || touchControls.touchAction);
				
				// Update life (unless invulnerable)
				if(life > 0f && !IsInvulnerable()) {
					life -= Time.deltaTime * lifeDrainPerSecond;

					// Have we died?
					if(life <= 0) {
						life = 0;
						ChangeState(EState.DYING);
					}
				}
			} break;
				
			case EState.EATING: {

				allowMovement = true;
				
				if (fireMode != EFireMode.FRENZY)
					allowFire = true;
				else
					UpdateFire(false);

				speedMulti = 0.3f;
				
				mStateTimer -= Time.deltaTime;
				if(mStateTimer < 0f) {
					ChangeState(EState.IDLE);
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

		// Update touch controller
		touchControls.UpdateTouchControls();

		// Update animations
		animator.SetBool("bite", mState == EState.EATING);
		animator.SetBool ("fire", allowFire);

		if (grab != null)
			UpdateGrab ();
	}
	
	void FixedUpdate(){

		// Update dragon's movement
		if(allowMovement) {
			UpdateMovement(speedMulti);
		}

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

			case EState.EATING: {
				// Start state timer
				mStateTimer = 0.35f;
				
				// Stop any active fire breath
				UpdateFire(false);
			} break;

			case EState.DYING: {
				// Stop any active fire breath and starving status
				UpdateFire(false);
				
				// Stop starving feedback as well
				ToggleStarving(false);
				
				// Notify the logic that the game has to end
				// [AOC] TODO!! Probably this shouldn't be controlled from the dragon itself
				//		 Maybe sending a global game event?
				App.Instance.gameLogic.EndGame();

				// Play death animation
				animator.SetTrigger ("dead");
				orientation.OnDeath();

				// Finish after some time
				mStateTimer = 1f;
			} break;

			case EState.DEAD: {
				// Open summary popup
				GameObject.Find("CanvasPopups").GetComponent<UIPrefabLoader>().InstantiatePrefab("Proto/Prefabs/UI/Popups/PF_PopupSummary");
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


		touchControls.CalcSharkDesiredVelocity(dragonSpeed * _fSpeedMultiplier, false);
		//impulse = Vector3.Lerp (new Vector3(touchControls.SharkDesiredVel.x, touchControls.SharkDesiredVel.y, 0f),impulse,0.15f);x
		impulse = new Vector3(touchControls.SharkDesiredVel.x, touchControls.SharkDesiredVel.y, 0f);

		bool plummeting = (dir.y < -0.75f && rbody.velocity.y < -dragonSpeed*0.85f) || (_fSpeedMultiplier == boostMultiplier  && rbody.velocity.magnitude > dragonSpeed*0.85f);
		plummeting = plummeting && grab == null;

		bool flyUp = !plummeting && dir.y > 0.75f &&  _fSpeedMultiplier == 1f;

		if(!impulse.Equals(Vector3.zero)) {

			Vector3 oldDir = dir; 
			dir = impulse;
			dir.Normalize();

			// Check sharp turn
			if (Vector3.Dot(oldDir,dir) < 0f && Mathf.Abs (dir.y) < 0.35f && Mathf.Abs (oldDir.y) < 0.35f){
				if (dir.x < 0f)
					animator.SetTrigger("turn_left");
				else
					animator.SetTrigger("turn_right");
			}

		}else{

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

		// limit movement
		pos = transform.position;
		if (pos.x < movementLimitX.min){
			pos.x = movementLimitX.min;
			transform.position = pos;
		}else if (pos.x > movementLimitX.max){
			pos.x = movementLimitX.max;
			transform.position = pos;
		}else if (pos.y > 5000f){
			pos.y = 5000f;
			transform.position = pos;
		}

		// decide if flying
		bool flying = touchControls.CurrentTouchState != TouchState.none ;

		// use fly animation if flying, but use idle animation if carrying something
		animator.SetBool("fly",flying && grab == null); 

		if(flying) {
			if (grab == null){ // Orientation with something grabbed is limited
				orientation.SetDirection(impulse);
			}else{
				Vector3 grabOrientation = impulse;
				grabOrientation.y = 0;
				orientation.SetDirection(grabOrientation);
			}
			glideTimer += Time.deltaTime;
			if (glideTimer > 6f)
				glideTimer = 0f;
		}

		// Glide timer controls how long we do the glide animation
		// we set it to zero if flying too flat or we need a diferent animation, like idle or plummeting
		if (!flying || plummeting || dir.y > 0.65f){
			glideTimer = 0f;
		}

		animator.SetFloat ("glide_time",glideTimer);
	}

	/// <summary>
	/// Enable/disable firebreath.
	/// </summary>
	/// <param name="_bActivate">Whether to activate or deactivate the dragon's firebreath.</param>
	void UpdateFire(bool _bActivate) {

			if (fireMode == EFireMode.FRENZY){
				
				UpdateBoost (_bActivate);

				
				if (allowFire){

					
					if (impulse.magnitude > 0)
						fireBreath.Fire(impulse);
					else
						fireBreath.Fire(dir);
					
					speedMulti = Mathf.Max ( speedMulti, 1.45f);
					
				}

			}
	}

	
	void UpdateBoost(bool activate){
		
		if (activate && allowBoost){
			
			energy -= Time.deltaTime*energyDrainPerSecond;
			
			speedMulti=boostMultiplier;
			
			if (energy < 0f){
				energy = 0f;
				allowBoost = false;
			}
		}else{
			
			if (energy > 0f && !activate)
				allowBoost = true;

			if (!activate){
				energy += Time.deltaTime*energyRefillPerSecond;
				if (energy > maxEnergy)
					energy = maxEnergy;
			}
		}
	}

	void UpdateGrab(){

		// try to detect fly height
		RaycastHit ground;
		if (Physics.Linecast( transform.position, transform.position + Vector3.down * 10000f, out ground, 1 << LayerMask.NameToLayer("Ground"))){
			float flyHeight =  transform.position.y - ground.point.y;
			if (flyHeight > 1000f || (flyHeight > 400f &&  speedMulti != 1f)){
				grab.Release(impulse);
				grab = null;
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
		life -= _damage;
			
		// Have we died?
		if(life <= 0) {
			life = 0;
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
		return IsAlive() && (mLife > maxLife * lifeWarningThreshold);
	}

	/// <summary>
	/// Whether the dragon is firing or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon is currently throwing fire; otherwise, <c>false</c>.</returns>
	public bool IsFiring() {
		return allowFire;	// [AOC] This may only be ok in Frenzy mode
	}

	/// <summary>
	/// Whether the dragon can take damage or not.
	/// </summary>
	/// <returns><c>true</c> if the dragon currently is invulnerable; otherwise, <c>false</c>.</returns>
	public bool IsInvulnerable() {
		// During fire, we're invulnerable
		if(IsFiring()) return true;

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
	/// <summary>
	/// Collision with another object
	/// </summary>
	/// <param name="other">The object which we've collided with.</param>
	void OnTriggerStay(Collider other) {

		// Only care if we're in idle state
		if(mState == EState.IDLE || mState == EState.EATING) {
			// Can object be eaten?
			EdibleBehaviour edible = other.gameObject.GetComponent<EdibleBehaviour>();
			if(edible != null && edible.edibleFromType <= dragonType) {

				Vector3 p1 = edible.transform.position;
				Vector3 p2 = mouth.position;

				p1.z = 0f;
				p1.y += edible.modelbounds.extents.y;
				p2.z = 0f;

				float distance = (p1-p2).sqrMagnitude;

				// Is  within mouth range?
				if (distance < sqrEatRange){

					// Yes!! Eat it!
					edible.OnEat();

					// Give hp reward
					GameEntity entity = edible.GetComponent<GameEntity>();
					if(entity != null) {
						life += entity.rewardHealth;
						life = Mathf.Min(life, maxLife);
					}

					animator.SetBool ("big_prey", edible.bigPrey);
					if (edible.bigPrey)
						mStateTimer = 0.65f;

					// Change logic state
					ChangeState(EState.EATING);
				}
			}
		}
	}

	public void Grab(GrabableBehaviour other){

		if ((mState == EState.IDLE || mState == EState.EATING) && grab == null){
			grab = other;
			grab.Grab ();
		}
	}

	/// <summary>
	/// Triggered whenever the fury rush is toggled.
	/// </summary>
	/// <param name="_bActivated">Whether the fury rush started or ended.</param>
	public void OnFuryToggled(bool _bActivated) {
		// Allow fire
		allowFire = _bActivated;

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
		Camera.main.GetComponent<CameraController>().Shake();
	}

	#endregion
}
#endregion
