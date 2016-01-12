using UnityEngine;
using System.Collections;

public class OLD_FlyingMineBehaviour : MonoBehaviour {

	public float damage = 25f;
	 
	GameEntity mEntity;
	EdibleBehaviour_OLD edible;

	Vector3 origin;
	Vector3 pos;
	float timer = 0f;

	void Awake() {
		mEntity = GetComponent<GameEntity>();
	}

	void Start () {

		edible = GetComponent<EdibleBehaviour_OLD>();
		origin = transform.position;
	}
	
	void Update () {

		if (edible.state != EdibleBehaviour_OLD.State.NONE)
			return;

		timer += Time.deltaTime;
		pos = origin;
		pos.y += Mathf.Sin(timer)*50f;
		pos.x += Mathf.Cos(timer*0.3f)*25f;
		transform.position = pos;

		float angle =  Mathf.Sin(timer*2f)*7f;
		transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward)*Quaternion.AngleAxis(180f, Vector3.up);
	}


	void OnCollisionEnter(Collision collision) {

		if (edible.state == EdibleBehaviour_OLD.State.NONE){

			if (collision != null){
				DragonMotion player = collision.collider.GetComponent<DragonMotion>();
				if (player != null && edible.edibleFromTier >= InstanceManager.player.data.def.tier){
					player.OnImpact(transform.position, damage, 100f, GetComponent<DamageDealer_OLD>());
					FinalExplosion();
				}
			}
		}
		
	}

	void FinalExplosion(){
		// Shake camera
		Camera.main.GetComponent<CameraController_OLD>().Shake ();


		ExplosionExpansion exp = ((GameObject)Object.Instantiate(Resources.Load ("PROTO/Effects/ExplosionExpansion"))).GetComponent<ExplosionExpansion>();
		exp.finalRadius = 500f;
		Vector3 p = transform.position;
		p.z = 0f;
		exp.center = p;


		// Launch explosion
		CompositeExplosion explosion = GetComponent<CompositeExplosion>();
		if(explosion != null) {
			explosion.Explode();

			// Hide mesh and destroy object after all explosions have been triggered
			MeshRenderer renderer = GetComponent<MeshRenderer>();
			renderer.enabled = false;
			StartCoroutine(DelayedDestruction(explosion.delayRange.max));
		} else {
			// No explosion was found, destroy immediately
			DestroyObject(this.gameObject);
		}
	}

	public void OnBurn(){
		FinalExplosion();
	}

	IEnumerator DelayedDestruction(float _fDelay) {
		// Delay destruction
		yield return new WaitForSeconds(_fDelay);
		DestroyObject(this.gameObject);
	}
}
