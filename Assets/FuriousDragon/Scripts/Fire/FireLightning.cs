using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireLightning : DragonFireInterface {

	public float firePower = 1f;

	public float segmentLength = 25f; 
	public float segmentWidth = 5f; 
	public float maxAmplitude = 50f; 

	public Material rayMaterial;

	// Test
	public float maxRayLength = 500f;

	float timer = 0f;
	float moveTimer = 0f;

	public Object particleStartPrefab;
	public Object particleEndPrefab;
	
	GameObject particleStart;
	GameObject particleEnd;

	Transform mouthPosition;
	Transform headPosition;
	Vector3 dir;

	int fireMask;
	int groundMask;
	bool firing = false;
	int fireFrame = 0;

	Lightning[] rays = new Lightning[3];


	class Lightning{

		List<LineRenderer> lines = new List<LineRenderer>();

		public float amplitude;
		public float segmentLength;
	
		public Lightning(float rayWidth, Color color,float numSegments, Material rayMaterial){
			
			for(int i=0;i<numSegments;i++){
				
				GameObject obj = new GameObject();
				obj.name = "RaySegment";
				obj.transform.parent = GameObject.Find ("Instances").transform;
				LineRenderer rayRender = obj.AddComponent<LineRenderer>();
				rayRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				rayRender.receiveShadows = false;
				rayRender.SetWidth( rayWidth, rayWidth);
				rayRender.SetColors(color,color);
				rayRender.material = rayMaterial;
				rayRender.enabled = false; 
				lines.Add(rayRender);
				
			}
		}

		public void Draw(Vector3 start, Vector3 end){

			Vector3 previous = start;
			Vector3 dir = (end-start).normalized;
			Vector3 normal = Vector3.Cross(dir,Vector3.forward);
			float dist = (end-start).magnitude;
			float numSegments = dist/segmentLength;
			for(int i=0;i<(int)numSegments;i++){
				lines[i].enabled = true;
				lines[i].SetPosition(0,previous);
				dir  = (end-previous).normalized;
				previous = previous+dir*segmentLength+normal*Random.Range (-amplitude, amplitude);
				if (i < numSegments-1)
					lines[i].SetPosition(1,previous);
				else
					lines[i].SetPosition(1,start);
			}
			
			
			for(int i=(int)numSegments;i<lines.Count;i++){
				lines[i].enabled = false;
			}
		}

		public void Hide(){
			for(int i=0;i<lines.Count;i++){
				lines[i].enabled = false;
			}
		}
	}



	// Use this for initialization
	void Start () {
	


		particleStart = (GameObject)Object.Instantiate(particleStartPrefab);
		particleStart.transform.localPosition = Vector3.zero;
		particleStart.gameObject.SetActive(true);
		
		particleEnd = (GameObject)Object.Instantiate(particleStartPrefab);
		particleEnd.transform.localPosition = Vector3.zero;
		particleEnd.gameObject.SetActive(true);

		mouthPosition = transform.FindSubObjectTransform("eat");
		headPosition = transform.FindSubObjectTransform("head");

		fireMask = 1 << LayerMask.NameToLayer("Edible") | 1 << LayerMask.NameToLayer("Burnable");
		groundMask = 1 << LayerMask.NameToLayer("Ground");


		rays[0] = new Lightning(segmentWidth, Color.white, maxRayLength/segmentLength,rayMaterial);
		rays[0].amplitude = maxAmplitude*0.25f;
		rays[0].segmentLength = segmentLength;

		rays[1] = new Lightning(segmentWidth*0.5f, Color.grey, maxRayLength/segmentLength,rayMaterial);
		rays[1].amplitude = maxAmplitude*0.5f;
		rays[1].segmentLength = segmentLength;

		rays[2] = new Lightning(segmentWidth*0.25f, new Color(0.25f,0.25f,0.25f,1f), maxRayLength/segmentLength,rayMaterial);
		rays[2].amplitude = maxAmplitude*0.5f;
		rays[2].segmentLength = segmentLength;

	}
	
	// Update is called once per frame
	void Update () {

		
		if (firing){
			timer += Time.deltaTime;
		
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

			Vector3 p1 = mouthPosition.position;
			particleStart.transform.position = mouthPosition.position;

			Vector3 p2;

			RaycastHit ground;
			if (Physics.Linecast( mouthPosition.position, mouthPosition.position+dir*maxRayLength, out ground, groundMask)){
				p2 = ground.point;
			}else{
				p2 =  mouthPosition.position+dir*maxRayLength;
			}

			particleEnd.transform.position = p2;

			for(int i=0;i<rays.Length;i++)
				rays[i].Draw(p1,p2);


			// DEtect we are not receiving Fire orders anymore
			fireFrame++;
			if (fireFrame >= 2){
				firing = false;
				particleStart.gameObject.SetActive(false);
				particleEnd.gameObject.SetActive(false);
				for(int i=0;i<rays.Length;i++)
					rays[i].Hide ();

			}
		}

	}

	override public void Fire(Vector3 direction){
		
		if (!firing){
			
			particleStart.transform.position = mouthPosition.position;
			dir = mouthPosition.position - headPosition.position;
			dir.z = 0f;
			dir.Normalize();
			particleEnd.transform.position = mouthPosition.position+dir*maxRayLength;
			
			particleStart.gameObject.SetActive(true);
			particleEnd.gameObject.SetActive(true);

			for(int i=0;i<rays.Length;i++)
				rays[i].Hide ();

			firing = true;
		}
		
		fireFrame = 0;
	}
}
