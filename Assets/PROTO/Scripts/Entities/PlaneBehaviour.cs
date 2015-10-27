// PlaneBehaviour.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Main controller for the plane.
/// The plane will fly horizontally at the center of the fly area and slowly move vertically towards the dragon if it enters 
/// the area in front of the plane.
/// </summary>
/// 
[RequireComponent(typeof(GameEntity))]
public class PlaneBehaviour : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	enum EState {
		FLYING,
		FALLING
	}
	#endregion

	#region EXPOSED PROPERTIES -----------------------------------------------------------------------------------------
	public Vector2 flyArea = new Vector2(5000, 1000);	// Will be ignored if set from outside the inspector
	public float speed = 600;	// Units per second
	public float detectionDistance = 700;	// If the player is within this distance in front of the plane, go towards it
	public float maxVerticalVelocity = 200;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	// Control vars
	private Rect mFlyRect;	// Will be initialized during the Awake call based on plane's initial position
	private EState mState = EState.FLYING;
	private Vector3 mVelocity;	// Direction and magnitude of plane's flight
	private Vector3 mTargetVelocity;

	// Some important game objects
	private DragonMotion mPlayer;
	private FlamableHeli mFlamable;
	private GameEntity mEntity;
	private CompositeExplosion mExplosion;

	// Internal aux vars
	private int mGroundMask;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Initialize fly area 
		mFlyRect = new Rect(transform.position.x, transform.position.y - flyArea.y/2, flyArea.x, flyArea.y);

		// Get references to important objects
		mFlamable = GetComponent<FlamableHeli>();
		mEntity = GetComponent<GameEntity>();
		mExplosion = GetComponent<CompositeExplosion>();
	}

	/// <summary>
	/// More initialization.
	/// </summary>
	public void Start() {
		// Make sure we start in the right place and state
		mPlayer = GameObject.Find("Player").GetComponent<DragonMotion>();
		transform.SetPosX(mFlyRect.x);
		transform.SetPosY(mFlyRect.center.y);
		mState = EState.FLYING;
		mVelocity = new Vector3(speed, 0, 0);
		mTargetVelocity = mVelocity;
		mGroundMask = 1 << LayerMask.NameToLayer("Ground");	// From HeliBehaviour
	}

	/// <summary>
	/// Logic update, called every frame.
	/// </summary>
	public void Update() {
		// Depends on current state
		if(mState == EState.FLYING) {
			UpdatePosition();
			UpdateVelocity();
			UpdateRotation();
			UpdateTracking();

			// If we've died, go to falling state
			if(mEntity.health <= 0) {
				mState = EState.FALLING;

				// Show some fireworks
				if(mExplosion != null) {
					// Trigger explosion a bit smaller than the defined one
					mExplosion.Explode(mExplosion.explosionsAmount/2, mExplosion.delayRange, mExplosion.spawnOffset, mExplosion.spawnArea/2f, mExplosion.scaleRange, mExplosion.rotationRange);
				}

				// [AOC] If it has the slow motion component, activate it
				SlowMotionController sloMo = GetComponent<SlowMotionController>();
				if(sloMo != null) {
					sloMo.StartSlowMotion();
				}
			}
		} else if(mState == EState.FALLING) {
			// Increase vertical speed dramatically while we decrease horizontal one
			mVelocity.y -= 1000f * Time.deltaTime;
			mVelocity.x -= mVelocity.x/2;
			UpdatePosition();
			
			// Detect ground collision
			RaycastHit ground;
			if(Physics.Linecast(transform.position, transform.position + Vector3.down * 100f, out ground, mGroundMask)) {	
				FinalExplosion();
			}
		}
	}
	#endregion

	#region INTERNAL METHODS -------------------------------------------------------------------------------------------
	/// <summary>
	/// Update plane's position based on its current velocity.
	/// </summary>
	private void UpdatePosition() {
		// Easy for now
		transform.position += mVelocity * Time.deltaTime;
	}

	/// <summary>
	/// Update plane's velocity based on its current position.
	/// </summary>
	private void UpdateVelocity() {
		// Interpolate towards target velocity
		mVelocity = Vector3.Lerp(mVelocity, mTargetVelocity, Time.deltaTime * 3f);	// [AOC] Arbitrary factor to speed up interpolation

		// Check whether we're out of bounds and adjust target velocity based on that
		// [AOC] Simple version, will be slightly out of the flying rect after reaching the limit
		// Horizontal
		if(transform.position.x > mFlyRect.xMax) {
			mTargetVelocity.x = -speed;
		} else if(transform.position.x < mFlyRect.xMin) {
			mTargetVelocity.x = speed;
		}
		
		// Vertical
		if(transform.position.y > mFlyRect.yMax || transform.position.y < mFlyRect.yMin) {
			// Immediately reverse and point towards 0 vertical velocity
			mVelocity.y *= -1;
			mTargetVelocity.y = 0;
		}
	}

	/// <summary>
	/// Update plane's rotation based on its current velocity.
	/// </summary>
	private void UpdateRotation() {
		// Update plane's rotation around Y axis (yaw)
		// [AOC] Simple interpolation between min and max X speed
		float fDelta = Mathf.InverseLerp(-speed, speed, mVelocity.x);
		float fAngleY = Mathf.LerpAngle(180f, 0f, fDelta);
		Quaternion qY = Quaternion.AngleAxis(fAngleY, Vector3.up);
		
		// Update plane's rotation around Z axis (pitch)
		// [AOC] Let's just use the angle between current velocity and horizontal in the same X direction
		Vector3 velocityXY = new Vector3(mVelocity.x, mVelocity.y, 0);	// Ignore Z
		Vector3 horizontalVelocity = new Vector3(mVelocity.x, 0, 0);
		float fAngleZ = Vector3.Angle(horizontalVelocity, velocityXY);
		fAngleZ *= Mathf.Sign(mVelocity.y);	// Change sign if moving downwards
		Quaternion qZ = Quaternion.AngleAxis(fAngleZ, Vector3.forward);

		// When turning (rotation around Y), add also a small rotation around X (roll)
		// Just for visuals :P
		/*fDelta = Mathf.InverseLerp(0, speed, Mathf.Abs(mVelocity.x));
		float fAngleX = Mathf.LerpAngle(0f, 90f, 1 - fDelta);
		fAngleX *= Mathf.Sign(mVelocity.x);
		Quaternion qX = Quaternion.AngleAxis(fAngleX, Vector3.right);
		*/

		// Apply rotation
		// Don't interpolate, that way we keep up with the translation animation speed
		transform.localRotation = qY * qZ /** qX*/;
	}

	/// <summary>
	/// Adjust target velocity based on player's position.
	/// </summary>
	private void UpdateTracking() {
		// Simple condition for now: if the player is in front of the plane and within the fly area, go towards it
		// [AOC] To check this, get the vector from the plane to the dragon and check its magnitude as well as the angle 
		//		 from the horizontal in plane's direction. This angle should be < 90.
		bool bTargetFound = false;

		// 1st check: is player within the fly area?
		Vector3 playerPos = mPlayer.transform.position;
		if(mFlyRect.Contains(playerPos)) {
			// 2nd check: is player within detection distance?
			Vector3 dir = playerPos - transform.position;
			dir.z = 0;	// Ignore Z
			float fDist = dir.magnitude;
			if(fDist <= detectionDistance) {
				// 3rd check: is player in front of the plane?
				Vector3 horizontalVelocity = new Vector3(mVelocity.x, 0, 0);
				float fAngle = Vector3.Angle(horizontalVelocity, dir);
				if(fAngle < 90f) {
					// All checks passed! Move towards the player
					// [AOC] Add a vertical velocity proportional to the distance to the player
					float fDelta = Mathf.InverseLerp(0, detectionDistance, fDist);
					mTargetVelocity.y = Mathf.Sign(dir.y) * Mathf.Lerp(0, maxVerticalVelocity, fDelta);
					bTargetFound = true;
				}
			}
		}

		// If target wasn't found, move towards default fly position
		if(!bTargetFound) {
			// Move slower as we approach target position
			float fDistY = mFlyRect.center.y - transform.position.y;
			mTargetVelocity.y = Mathf.Clamp(fDistY, -maxVerticalVelocity, maxVerticalVelocity);
		}
	}
	#endregion

	#region IMPORTED FROM HELI BEHAVIOUR -------------------------------------------------------------------------------
	/// <summary>
	/// Something has collided with the plane's propeller.
	/// </summary>
	/// <param name="_collision">The collision data.</param>
	private void OnCollisionEnter(Collision _collision) {
		// Is it the dragon
		DragonMotion p = _collision.collider.GetComponent<DragonMotion>();
		if(p != null) {
			// Yes! Deal some damage.
			p.OnImpact(transform.position, 10f, 100f, GetComponent<DamageDealer_OLD>());
		}
	}

	/// <summary>
	/// Trigger the final explosion.
	/// </summary>
	private void FinalExplosion() {
		// Launch some explosion over the plane
		if(mExplosion != null) {
			mExplosion.Explode();
		}
		
		// Look for any other burnable object around us
		ExplosionExpansion exp = ((GameObject)Object.Instantiate(Resources.Load ("PROTO/Effects/ExplosionExpansion"))).GetComponent<ExplosionExpansion>();
		exp.finalRadius = 400f;
		Vector3 p = transform.position;
		p.z = 0f;
		exp.center = p;


		// Shake camera
		Camera.main.GetComponent<CameraController_OLD>().Shake();
		
		// Destroy ourselves
		DestroyObject(this.gameObject);
	}
	#endregion
}
#endregion