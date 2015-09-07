using UnityEngine;
using System.Collections;

[RequireComponent(typeof(GameEntity))]
public class HeliBehaviour : MonoBehaviour {

	// Heli components
	private GameEntity entity = null;
	private FlamableHeli flamable = null;
	private DragonPlayer player = null;
	private CannonBehaviour cannon = null;
	private GameObject mMainRotor = null;
	private GameObject mTailRotor = null;

	enum State{

		IDLE,
		TRACKING,
		FALLING
	}

	State state = State.IDLE;
	Vector3 impulse = Vector3.zero;
	Vector3 targetPos = Vector3.zero;
	Vector3 targetOffset = Vector3.zero;
	Vector3 oriPos;
	Vector3 pos;
	Vector3 oldPos;
	float timer = 0;
	float decisionTimer = 0;
	float angle = 0;
	CompositeExplosion mExplosion;
	int groundMask;
	float actionRadius;

	// Use this for initialization
	void Start () {

		entity = GetComponent<GameEntity>();
		flamable = GetComponent<FlamableHeli>();
		cannon = GetComponentInChildren<CannonBehaviour>();
		player = GameObject.Find ("Player").GetComponent<DragonPlayer>();
		mExplosion = GetComponent<CompositeExplosion>();
		pos = transform.position;
		oriPos = pos;
		groundMask = 1 << LayerMask.NameToLayer("Ground");
		actionRadius = 600*600;

		// Rotors
		mMainRotor = this.gameObject.FindSubObject ("helicopter-rotor");
		mTailRotor = this.gameObject.FindSubObject ("helicopter-tail-rotor");
	}
	
	void Update () {
	
		oldPos = pos;

		if (state == State.IDLE){

			UpdateIdle();
		}else if (state == State.TRACKING){

			UpdateTracking();
		}

		UpdateAngle();

		// Update rotors animation
		if(state != State.FALLING) {
			TwistRotors();
		}

		// Check damage by dragon
		if (entity.health < 0 && state != State.FALLING){
			if (cannon != null)
				cannon.StopShooting();

			if (Random.Range(0,100) < -25){ // Heli can explode inmediately

				FinalExplosion();
			}else{

				state = State.FALLING;

				// Trigger an explosion a bit smaller than the final one
				if(mExplosion) {
					mExplosion.Explode(mExplosion.explosionsAmount/2, mExplosion.delayRange, mExplosion.spawnOffset, mExplosion.spawnArea/2f, mExplosion.scaleRange, mExplosion.rotationRange);
				}

				// [AOC] If it has the slow motion component, activate it
				SlowMotionController sloMo = GetComponent<SlowMotionController>();
				if(sloMo != null) {
					sloMo.StartSlowMotion();
				}
			}
		}else if (state == State.FALLING){ // Or fall to the ground
			impulse.y -= 1000f*Time.deltaTime;
			pos += impulse*Time.deltaTime;
			transform.position = pos;

			// Plant them on start
			RaycastHit ground;
			if (Physics.Linecast( pos, pos + Vector3.down * 75f, out ground, groundMask)){

				FinalExplosion();
			}
		}
	}

	void UpdateIdle(){

		Vector3 p = Vector3.Lerp (pos,oriPos,0.1f) - pos;
		if (p.magnitude > 12f){
			p = p.normalized*12f;
		}
		pos += p;
		transform.position = pos;

		decisionTimer-=Time.deltaTime;
		if ((player.transform.position - transform.position).sqrMagnitude < actionRadius && decisionTimer < 0f){
			state = State.TRACKING;
			if (cannon != null)
				cannon.Shoot();
			decisionTimer = Random.Range (2f,4f);
			targetOffset.x = Random.Range (-100f,100f);
			targetOffset.y = Random.Range (-100f,100f);
		}
	}

	void UpdateTracking(){

		Vector3 dir = pos+targetOffset - player.transform.position;
		dir.Normalize();
		targetPos = player.transform.position+dir*300f;
		dir *= -1f;
	
		// Move limiting speed
		Vector3 p = Vector3.Lerp (pos,targetPos,0.1f) - pos;
		if (p.magnitude > 12f){
			p = p.normalized*12f;
		}
		pos += p;

		// Don't go into the ground
		RaycastHit ground;
		if (Physics.Linecast( pos, pos - dir * 150f, out ground, groundMask)){
			pos = new Vector3(ground.point.x,ground.point.y,0f)+dir*150f;
		}

		transform.position = pos;

		decisionTimer-=Time.deltaTime;

		float distanceFromOrigin = (oriPos - pos).magnitude;

		if (decisionTimer <= 0f){

			if (distanceFromOrigin > 2000f){
				state = State.IDLE;
				impulse = Vector3.zero;
				decisionTimer = Random.Range (2f,4f);
				if (cannon != null)
					cannon.StopShooting();
			}else{
				decisionTimer = Random.Range (1f,4f);
				targetOffset.x = Random.Range (-100f,100f);
				targetOffset.y = Random.Range (-100f,100f);
			}
		}
	}

	void UpdateAngle(){
		// Aux vars
		float fRotationSpeed = 5f;	// deg/seconds?
		Vector3 dir = player.transform.position - transform.position;

		// [AOC] Quaternion.LookRotation() does the hard job! ^_^
		Quaternion q = Quaternion.LookRotation(dir);
		q *= Quaternion.Euler(0, -90, 0);	// LookRotation gives the rotation so the Z axis looks towards the target dir. Since our heli is aligned with the X axis, do some extra rotation

		// Clamp the rotation in Z to limit heli's inclination
		// [AOC] The euler angles are always positive (0-360), being values [0..180] the top semicircle and [180..0] the bottom one
		float fAngleZ = q.eulerAngles.z;
		if(fAngleZ < 180f) {
			fAngleZ = Mathf.Clamp(fAngleZ, 0f, 30f);	// [AOC] MAGIC NUMBERS!! Upper limit
		} else {
			fAngleZ = Mathf.Clamp(fAngleZ, 360f - 30f, 360f);	// [AOC] MAGIC NUMBERS!! Lower limit
		}

		// Create thefinal quaternion, interpolate for a smoother animation, and apply rotation
		q = Quaternion.Euler(q.eulerAngles.x, q.eulerAngles.y, fAngleZ);
		q = Quaternion.Slerp(transform.localRotation, q, Time.deltaTime * fRotationSpeed);
		transform.localRotation = q;
	}

	private void TwistRotors() {
		// From HSE
		float fAngle = Mathf.Repeat(Time.deltaTime * 1440.0f, 360.0f);	// [AOC] MAGIC NUMBERS!! Rotation speed
		mMainRotor.transform.Rotate(fAngle * Vector3.forward); 
		mTailRotor.transform.Rotate(fAngle * Vector3.right); 
	}

	void OnCollisionEnter(Collision collision) {
		
		if (collision != null && collision.collider.GetComponent<DragonPlayer>() != null){
			if (collision.collider.transform.position.y > transform.position.y + 50){ // Collision with top of the heli
				collision.collider.GetComponent<DragonPlayer>().OnImpact(transform.position, 10f, 100f, GetComponent<DamageDealer>());
			}
		}
		
	}

	void FinalExplosion(){
		// Launch explosion
		CompositeExplosion explosion = GetComponent<CompositeExplosion>();
		if(explosion != null) {
			explosion.Explode();
			
			// Hide mesh and destroy object after all explosions have been triggered
			GameObject heliObj = this.gameObject.FindSubObject("helicopter");
			heliObj.SetActive(false);
			StartCoroutine(DelayedDestruction(explosion.delayRange.max));
		} else {
			// No explosion was found, destroy immediately
			DestroyObject(this.gameObject);
		}

		ExplosionExpansion exp = ((GameObject)Object.Instantiate(Resources.Load ("PROTO/Effects/ExplosionExpansion"))).GetComponent<ExplosionExpansion>();
		exp.finalRadius = 400f;
		Vector3 p = transform.position;
		p.z = 0f;
		exp.center = p;


		Camera.main.GetComponent<CameraController_OLD>().Shake ();
	}

	IEnumerator DelayedDestruction(float _fDelay) {
		// Delay destruction
		yield return new WaitForSeconds(_fDelay);
		DestroyObject(this.gameObject);
	}
}
