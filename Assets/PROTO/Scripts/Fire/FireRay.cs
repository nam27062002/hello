using UnityEngine;
using System.Collections;

public class FireRay : DragonBreathBehaviour {

	public float firePower = 1f; // Damage per impact
	float timer;

	public float maxRayLength = 1000f;

	public Material rayMaterial;
	public Object particleStartPrefab;
	public Object particleEndPrefab;

	
	Transform mouthPosition;
	Transform headPosition;
	Vector3 dir;

	LineRenderer rayRender;
	int fireMask;
	int groundMask;
	bool firing = false;
	int fireFrame = 0;

	GameObject particleStart;
	GameObject particleEnd;


	override protected void ExtendedStart () {

		timer = 0f;
		
		mouthPosition = transform.FindTransformRecursive("eat");
		headPosition = transform.FindTransformRecursive("head");
	
		rayRender = gameObject.AddComponent<LineRenderer>();
		rayRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		rayRender.receiveShadows = false;
		rayRender.SetColors(Color.white,Color.white);
		rayRender.SetWidth(20f,20f);
		rayRender.material = rayMaterial;
		rayRender.enabled = false;

		particleStart = (GameObject)Object.Instantiate(particleStartPrefab);
		particleStart.transform.localPosition = Vector3.zero;
		particleStart.gameObject.SetActive(false);

		particleEnd = (GameObject)Object.Instantiate(particleStartPrefab);
		particleEnd.transform.localPosition = Vector3.zero;
		particleEnd.gameObject.SetActive(false);


		fireMask = 1 << LayerMask.NameToLayer("Edible") | 1 << LayerMask.NameToLayer("Burnable");
		groundMask = 1 << LayerMask.NameToLayer("Ground");
	}
	
	override protected void ExtendedUpdate () {

		if (firing){
			timer += Time.deltaTime;

			rayMaterial.mainTextureOffset = Vector2.right*-timer;

			dir = mouthPosition.position - headPosition.position;
			dir.z = 0f;
			dir.Normalize();

			float rayStep = 2f;
			Vector3 p;
			RaycastHit hit;
			for(float i=0;i<maxRayLength/rayStep;i+=rayStep){

				p = mouthPosition.position+dir*i;
				if (Physics.Linecast(p+Vector3.forward*-400f,p+Vector3.forward*800f,out hit,fireMask)){
				
					if (hit.collider != null){
						
						// Burn whatever burnable
						FlamableBehaviour flamable =  hit.collider.GetComponent<FlamableBehaviour>();
						if (flamable != null){
							flamable.Burn (hit.point, firePower);
						}
					}
				}
			}

			rayRender.SetPosition(0,mouthPosition.position);
			particleStart.transform.position = mouthPosition.position;

			RaycastHit ground;
			if (Physics.Linecast( mouthPosition.position, mouthPosition.position+dir*maxRayLength, out ground, groundMask)){
				rayRender.SetPosition(1,ground.point);
				particleEnd.transform.position = ground.point;
			}else{
				rayRender.SetPosition(1,mouthPosition.position+dir*maxRayLength);
				particleEnd.transform.position = mouthPosition.position+dir*maxRayLength;
			}


			// DEtect we are not receiving Fire orders anymore
			fireFrame++;
			if (fireFrame >= 2){
				firing = false;
				rayRender.enabled = false;
				particleStart.gameObject.SetActive(false);
				particleEnd.gameObject.SetActive(false);
			}
		}

	}
	
	override protected void Breath() {

		if (!firing){
				
			particleStart.transform.position = mouthPosition.position;
			dir = mouthPosition.position - headPosition.position;
			dir.z = 0f;
			dir.Normalize();
			particleEnd.transform.position = mouthPosition.position+dir*maxRayLength;

			particleStart.gameObject.SetActive(true);
			particleEnd.gameObject.SetActive(true);

			firing = true;
			rayRender.enabled = true;
		}

		fireFrame = 0;
	}
}
