using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(GameEntity))]
public class FlamableBehaviour : MonoBehaviour {
	[Range(0, 1)] public float feedbackProbability = 0.5f;
	public List<UIFeedbackMessage> burnFeedbacks = new List<UIFeedbackMessage>();
	public bool giveRewardsOnBurn = true;	// Set to false for entities that can be eaten after burned

	[HideInInspector] public bool hasBurned = false;
	protected GameEntity entity = null;

	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		entity = GetComponent<GameEntity>();
		hasBurned = false;
		Messenger.AddListener<Vector3,float>("OnExplosion",OnExplosion);
		Initialize();
	}

	void OnDestroy(){
		Messenger.RemoveListener<Vector3,float>("OnExplosion",OnExplosion);
	}

	/// <summary>
	/// Start burning
	/// </summary>
	/// <param name="_pos">The position where the burn is taking place?</param>
	public void Burn(Vector3 pos, float power = 10f) {

		if (entity.health > 0f){
			entity.health -= power;

			// Have we died?
			if(entity.health <= 0) {
				// Update flag
				hasBurned = true;
				
				// Send game event
				Messenger.Broadcast<GameEntity>(GameEvents_OLD.ENTITY_BURNED, entity);
			}

			// Let heirs do their custom stuff
			BurnImpl(pos, power);
		}
	}


	public void OnExplosion(Vector3 position, float distance){

		Vector3 pos = transform.position;
		pos.z = 0f;
		if ((position-pos).sqrMagnitude < distance){
			ExplodeImpl (position, 10f);
		}
	}

	virtual protected void Initialize() {
	}

	/// <summary>
	/// Custom actions to be done on the Burn() call.
	/// </summary>
	/// <param name="_pos">The position where the burn is taking place?</param>
	/// <param name="_power">The damage the particle deals</param>
	virtual protected void BurnImpl(Vector3 _pos, float _power) {
		// To be implemented by heirs
	}

	virtual protected void ExplodeImpl(Vector3 _pos, float _power) {
		// To be implemented by heirs
		Burn (_pos, _power);
	}
}
