using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThunderStorm : MonoBehaviour {

	//-----------------------------------------------
	struct Segment {
		public Vector3 pointA;
		public Vector3 pointB;
		
		public Segment(Vector3 _pointA, Vector3 _pointB) {
			pointA = _pointA;
			pointB = _pointB;
		}
	};

	struct ThunderBranch {
		public List<Segment> segments;
		public int generationAt;
		public GameObject gameObject;

		public ThunderBranch(int _generation) {
			segments = new List<Segment>();
			generationAt = _generation;
			gameObject = null;
		}
	};

	/**************************************************/	

	class Thunder {

		//---------------------
		// Attributes			 
		//---------------------

		private List<ThunderBranch> m_branches;

		private int m_segmentDivisions = 5;
		private float m_pathOffsetFactor = 0.3f;
		private float m_width = 20f;

		private float m_branchingProbability = 0.6f;
		private float m_branchOffsetFactor = 0.5f;
		private float m_branchLengthFactor = 0.7f;

		private float m_alpha;


		//---------------------
		// Methods			 
		//---------------------
		public Thunder () {
			m_branches = new List<ThunderBranch>();			
			
			//add a thunder branch into Pool Controller
			PoolManager.CreatePool((GameObject)Resources.Load("PROTO/WeatherEffects/ThunderStorm/ThunderBranch"), (m_segmentDivisions - 1) * (m_segmentDivisions - 1), false);
		}
		
		public void Destroy () {
			for (int i = 0; i < m_branches.Count; i++) {
				m_branches[i].segments.Clear();
				GameObject.Destroy(m_branches[i].gameObject);
			}
			m_branches.Clear();
		}

		public void Clear () {
			for (int i = 0; i < m_branches.Count; i++) {				
				m_branches[i].gameObject.SetActive(false);
			}
			m_branches.Clear();
		}

		public void Generate (float _width, Vector3 _source, Vector3 _target) {
			
			m_alpha = 1;
			m_width = _width;
			m_branches.Clear();


			float maxOffsetValue = m_pathOffsetFactor * (_target - _source).magnitude;
			float maxBranches = Random.Range(5, (m_segmentDivisions - 1) * (m_segmentDivisions - 1));

			ThunderBranch main = new ThunderBranch(0);
			main.gameObject = PoolManager.GetInstance("ThunderBranch");
			main.segments.Add(new Segment(_source, _target));
			m_branches.Add(main);

			for (int d = 0; d < m_segmentDivisions; d++) { // number of divisions

				int branchesToProcess = m_branches.Count;
				for (int b = 0; b < branchesToProcess; b++) {

					ThunderBranch currentBranch = m_branches[b];
					int segmentCount = currentBranch.segments.Count;
					for (int s = 0; s < segmentCount; s++) {

						Segment currentSegment = currentBranch.segments[0];
						Vector3 pointC = ((currentSegment.pointA + currentSegment.pointB) * 0.5f) + new Vector3(Random.Range(-maxOffsetValue, maxOffsetValue), Random.Range(-maxOffsetValue, 0), 0);

						currentBranch.segments.RemoveAt(0);
						currentBranch.segments.Add(new Segment(currentSegment.pointA, pointC));
						currentBranch.segments.Add(new Segment(pointC, currentSegment.pointB));

						// lets branch!
						if (d < m_segmentDivisions - 1 && m_branches.Count < maxBranches) {

							if (Random.Range(0f, 1f) < m_branchingProbability) {
								GameObject branchGO = PoolManager.GetInstance("ThunderBranch");; // get from pool
								if (branchGO != null) {
									Vector3 dir = pointC - currentSegment.pointA;
									float maxBranchDirOffset = dir.magnitude * m_branchOffsetFactor;

									dir += new Vector3(Random.Range(-maxBranchDirOffset, maxBranchDirOffset), Random.Range(-maxBranchDirOffset, 0), 0);
									Vector3 pointD = pointC + dir * m_branchLengthFactor;

									ThunderBranch branch = new ThunderBranch(d);
									branch.gameObject = branchGO;
									branch.segments.Add(new Segment(pointC, pointD));
									m_branches.Add(branch);	
								}
							}
						}
					}
				}
				
				// Reduce the max offset for the next generation
				maxOffsetValue *= 0.5f;
			}
		}

		public void BuildLineRenderer (GameObject _parent, Color _color, Material _material) {

			for (int i = 0; i < m_branches.Count; i++) {

				if (m_branches[i].generationAt > 0) {
					m_width /= m_branches[i].generationAt + 1;
				}
				
				LineRenderer lineRenderer = m_branches[i].gameObject.GetComponent<LineRenderer>();
				lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				lineRenderer.receiveShadows = false;
				lineRenderer.enabled = true; 
				
				lineRenderer.SetColors(_color, _color);
				lineRenderer.material = _material;
				lineRenderer.SetWidth( m_width, 2);
				
				// populate line renderer with vertex
				int s = 0;
				lineRenderer.SetVertexCount(m_branches[i].segments.Count + 1);
				for (s = 0; s < m_branches[i].segments.Count; s++) {
					lineRenderer.SetPosition(s, m_branches[i].segments[s].pointA);
				}
				lineRenderer.SetPosition(s, m_branches[i].segments[s - 1].pointB);
			}

			SetAlpha(1);
		}

		public void SetAlpha (float _alpha) {

			m_alpha = _alpha;
		
			Color color = Color.white;
			color.a = m_alpha;
			for (int i = 0; i < m_branches.Count; i++) {
				LineRenderer lineRenderer = m_branches[i].gameObject.GetComponent<LineRenderer>();
				lineRenderer.SetColors(color, color);
			}
		}

		public float GetAlpha () {

			return m_alpha;
		}
	}


	
	/**************************************************/
	
	
	public Material rayMaterial;
	public List<Transform> m_source;
	public List<Transform> m_target;
	public float m_width = 20;

	public float damage = 5;	
	public float thunderSpawnTime = 0.75f;
	public float thunderChangeTime = 0.08f;


	private float spawnTimer;
	private float changeTimer;
	
	private int changeCount;
	
	private Thunder m_thunder;

	private int m_sourceIndex;
	private int m_targetIndex;

	private bool m_doDamage;
	private bool m_onScreen;


	// Use this for initialization
	void Start () {
		m_thunder = new Thunder();

		spawnTimer = thunderSpawnTime;
		changeTimer = thunderChangeTime;
		changeCount = 0;
		m_doDamage = false;
		m_onScreen = false;
	}

	void OnDestroy() {
		m_thunder.Destroy();
	}
		
	void SpawnThunder () {
		m_thunder.Clear();	
		m_thunder.Generate(m_width, m_source[m_sourceIndex].position, m_target[m_targetIndex].position);
		m_thunder.BuildLineRenderer(gameObject, Color.white, rayMaterial);		
	}

	void FadeOutThunder (float _t) {
		float alpha = m_thunder.GetAlpha();
		alpha = Mathf.Lerp(alpha, 0, _t);
		m_thunder.SetAlpha(alpha);
	}

	void RandomizeSourceTarget () {		
		m_sourceIndex = Random.Range(0, m_source.Count);
		m_targetIndex = Random.Range(0, m_target.Count);
	}

	void Update () {

		Vector3 screenPoint = Camera.main.WorldToViewportPoint (transform.position);
		if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1) {
			m_onScreen = true;

			if (spawnTimer > 0) {
				
				spawnTimer -= Time.deltaTime;

				FadeOutThunder(1 - Mathf.Max(0, spawnTimer / thunderSpawnTime));

				if (spawnTimer <= 0) {

					RandomizeSourceTarget();
					SpawnThunder();

					spawnTimer = 0;
					changeCount = 0;
					changeTimer = thunderChangeTime;
					m_doDamage = true;
				}

			} else {
				changeTimer -= Time.deltaTime;

				if (changeTimer <= 0) {
					if (changeCount < 3) {
						SpawnThunder();
						changeTimer = thunderChangeTime;
						changeCount++;
					} else {
						spawnTimer = thunderSpawnTime;
					}
				}
			}
		} else {
			if (m_onScreen) {
				m_thunder.Clear();
				m_onScreen = false;
			}
		}
	}

	void OnTriggerStay (Collider collider) {
		
		if (collider != null){
			DragonMotion player = collider.GetComponent<DragonMotion>();
			if (player != null){
				if (m_doDamage) {
					player.OnImpact(transform.position, damage, 0, GetComponent<DamageDealer>());
					m_doDamage = false;
				}
			}
		}
	}

}
