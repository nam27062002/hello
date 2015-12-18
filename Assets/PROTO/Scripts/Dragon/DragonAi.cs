using UnityEngine;
using System.Collections;

public class DragonAi : MonoBehaviour {


	#region CONSTANTS ----------------------------------------------------------
	public enum EState {
		INIT,
		IDLE,
		DYING,
		DEAD,
		DEAD_GORUND
	};
	
	#endregion
	
	#region EXPOSED MEMBERS ----------------------------------------------------
	[Header("Generic Settings")]
	public float dragonSpeed = 100f;
	public float boostMultiplier = 2.5f;
	public Range movementLimitX = new Range(-10000, 10000);
	
	[Header("Components")]
	public SkinnedMeshRenderer bodyMesh;
	public SkinnedMeshRenderer wingsMesh;
	#endregion
	
	#region PUBLIC MEMBERS -----------------------------------------------------
	[HideInInspector] public bool invulnerable = false;	// Debug purposes
	#endregion
	
	#region INTERNAL MEMBERS ---------------------------------------------------
	// Game objects
	DragonControl		controls;
	DragonBreathBehaviour fireBreath;
	Rigidbody       	rbody;
	Animator  			animator;
	DragonOrientation   orientation;
	Transform    		mouth;
	EdibleBehaviour_OLD 	edible;

	// Control vars
	EState mState = EState.INIT;
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
		edible = GetComponent<EdibleBehaviour_OLD>();
		mouth = transform.FindTransformRecursive("eat");
		pos = transform.position;
		impulseMulti = 4f;
		
		// Load selected skin
		// Load both materials
		/*
		Material bodyMat = Resources.Load<Material>("Proto/Materials/Dragon/MT_dragon_" + GameSettings.skinName);
		Material wingsMat = Resources.Load<Material>("Proto/Materials/Dragon/MT_dragon_" + GameSettings.skinName + "_alphaTest");
		if(bodyMat != null && wingsMat != null) {
			// Apply body materials
			bodyMesh.material = bodyMat;
			wingsMesh.material = wingsMat;
		}
		*/

		// Go to IDLE state
		ChangeState(EState.IDLE);
		
		allowFire = false;

	}

	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {

		if (edible.state != EdibleBehaviour_OLD.State.NONE)
			return;

		// Aux vars
		allowMovement = false;
		speedMulti = 1f;
		
		// Do different stuff depending on dragon's state
		switch(mState) {
			
		case EState.IDLE: {
			
			// Movement allowed
			allowMovement = true;
			
			// Should we be firing?
			//UpdateFire(controls.action);

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
		
		// Update animations
		animator.SetBool ("fire", allowFire);

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
	public void ChangeState(EState _eNewState) {
		// Ignore if state hasn't changed
		if(mState == _eNewState) return;
		
		// Do custom actions based on old and new states
		switch(_eNewState) {
			
		case EState.IDLE: {
			
		} break;
			
		case EState.DYING: {
			// Stop any active fire breath and starving status
			//UpdateFire(false);
		
			// Play death animation
			animator.SetTrigger ("dead");
			orientation.OnDeath();
			
			// Finish after some time
			mStateTimer = 1f;
		} break;
			
		case EState.DEAD: {
		} break;
			
		case EState.DEAD_GORUND: {
			animator.SetTrigger("dead_hit");
		}break;
		}
		
		// Store new state
		mState = _eNewState;
	}
	
	/// <summary>
	/// Group all the movement logic in here (to help with code readability).
	/// </summary>
	/// <param name="_fSpeedMultiplier">The multiplier of the dragon's movement speed.</param>
	void UpdateMovement(float _fSpeedMultiplier) {
		
		impulse = controls.GetImpulse(dragonSpeed*_fSpeedMultiplier); 
		
		bool plummeting = (dir.y < -0.75f && rbody.velocity.y < -dragonSpeed*0.85f) || (_fSpeedMultiplier == boostMultiplier  && rbody.velocity.magnitude > dragonSpeed*0.85f);
		//plummeting = plummeting;
		
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

		
		// decide if flying
		bool flying = controls.moving;
		
		// use fly animation if flying, but use idle animation if carrying something
		animator.SetBool("fly",flying); 
		
		if(flying) {
			orientation.SetDirection(impulse);
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
		
	void UpdateBoost(bool activate){
		
		if (activate && allowBoost){
			
			speedMulti=boostMultiplier;

		}else{
			
			if (!activate)
				allowBoost = true;

		}
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

	
	public void OnImpact(Vector3 origin, float damage, float intensity, DamageDealer_OLD _source){

		// Ignore if dragon is not alive
		if(!IsAlive()) return;
		
		// Ignore if dragon is invulnerable as well
		if(IsInvulnerable()) return;
		
		Vector3 expForce = transform.position - origin;
		rbody.AddForce(expForce*300f*intensity);
	}

	public void AddForce(Vector3 force){
	
		rbody.AddForce(force);
	}
	
	#endregion
}
