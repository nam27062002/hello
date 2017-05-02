using UnityEngine;
using System.Collections;
using AI;

public class DevicePassengersSpawner : AbstractSpawner {	

	//-------------------------------------------------------------------
	[System.Serializable]
	public class EntityPrefab {
		[EntityPrefabListAttribute]
		public string name = "";
		public float chance = 100;

		public EntityPrefab() {
			name = "";
			chance = 100;
		}
	}
	[SerializeField] private EntityPrefab[] m_entityPrefabList = new EntityPrefab[1];
	[SerializeField] private RangeInt m_quantity = new RangeInt(1, 1);
	[SerializeField] private Transform[] m_spawnAtTransform;
	[SerializeField] private bool m_mustBeChild = true;
	//-------------------------------------------------------------------	


	private Transform[] m_parents;
	private IMachine[] m_machines;


    void Awake() {
		m_parents = new Transform[m_quantity.max];
		m_machines = new IMachine[m_quantity.max];
    }

	void OnDestroy() {
		if (ApplicationManager.IsAlive) {
			ForceRemoveEntities();
		}
	}
    //-------------------------------------------------------------------

    //-------------------------------------------------------------------
    // AbstractSpawner implementation
    //-------------------------------------------------------------------	

    private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
    public override AreaBounds area { get { return m_areaBounds; } set { m_areaBounds = value; } }

	protected override void OnInitialize() {
        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;        
        UseSpawnManagerTree = false; 

		if (m_entityPrefabList != null && m_entityPrefabList.Length > 0) {
			if (m_quantity.max < m_quantity.min) {
				m_quantity.min = m_quantity.max;
			}                			

			// adjust probabilities
			float probFactor = 0;
			for (int i = 0; i < m_entityPrefabList.Length; i++) {
				probFactor += m_entityPrefabList[i].chance;
			}

			if (probFactor > 0f) {
				probFactor = 100f / probFactor;
				for (int i = 0; i < m_entityPrefabList.Length; i++) {
					m_entityPrefabList[i].chance *= probFactor;
				}

				//sort probs
				for (int i = 0; i < m_entityPrefabList.Length; i++) {
					for (int j = 0; j < m_entityPrefabList.Length - i - 1; j++) {
						if (m_entityPrefabList[j].chance > m_entityPrefabList[j + 1].chance) {
							EntityPrefab temp = m_entityPrefabList[j];
							m_entityPrefabList[j] = m_entityPrefabList[j + 1];
							m_entityPrefabList[j + 1] = temp;
						}
					}
				}

				for (int i = 0; i < m_entityPrefabList.Length; i++) {
					PoolManager.RequestPool(m_entityPrefabList[i].name, IEntity.EntityPrefabsPath, m_entities.Length);
				}

				gameObject.SetActive(false);

				return;
			}
		}        
    }

    protected override uint GetMaxEntities() {
		return (uint)m_quantity.max;
    }

    protected override bool CanRespawnExtended() {        
        return true;
    }

    protected override uint GetEntitiesAmountToRespawn() {        
		return (uint)m_quantity.GetRandom();
    }        

	protected override string GetPrefabNameToSpawn(uint index) {		
		return m_entityPrefabList[GetPrefabIndex()].name;
	}

	private int GetPrefabIndex() {
		int i = 0;
		float rand = Random.Range(0f, 100f);
		float prob = 0;

		for (i = 0; i < m_entityPrefabList.Length - 1; i++) {
			prob += m_entityPrefabList[i].chance;

			if (rand <= prob) {
				break;
			} 

			rand -= prob;
		}

		return i;
	}    

    protected override void OnEntitySpawned(GameObject spawning, uint index, Vector3 originPos) {
        Transform groundSensor = spawning.transform.FindChild("groundSensor");
        Transform t = spawning.transform;
        
		m_parents[index] = t.parent;
		m_machines[index] = spawning.GetComponent<IMachine>();

		Transform parent = m_spawnAtTransform[index % m_spawnAtTransform.Length];

		if (m_mustBeChild) {
			t.parent = parent;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
		} else {
			t.position = parent.position;
			t.rotation = parent.rotation;
		}

		if (groundSensor != null) {
			t.position -= groundSensor.localPosition;
		}
		t.localScale = Vector3.one;
    }

    protected override void OnRemoveEntity(GameObject _entity, int index) {
		for (int i = 0; i < m_machines.Length; i++) {
			if (m_machines[i] != null && _entity == m_machines[i].gameObject) {
				m_machines[i] = null;
				return;
	        }
		}
    }    
    //-------------------------------------------------------------------


    //-------------------------------------------------------------------
    // Queries
    //-------------------------------------------------------------------
    public void PassengersEnterDevice() {
		for (int i = 0; i < m_machines.Length; i++) {
			if (m_machines[i] != null) {
				m_machines[i].EnterDevice(false);
			}
		}
	}

	public void PassengersLeaveDevice() {
		for (int i = 0; i < m_machines.Length; i++) {
			if (m_machines[i] != null) {
				if (m_mustBeChild) {
					m_machines[i].transform.parent = m_parents[i];
				}
				m_machines[i].LeaveDevice(false);
			}
		}
	}

	public void PassengersBurn() {
		for (int i = 0; i < m_machines.Length; i++) {
			if (m_machines[i] != null) {
				m_machines[i].Burn(transform);
			}
		}
	}
	//-------------------------------------------------------------------


	//-------------------------------------------------------------------
	// Debug
	//-------------------------------------------------------------------
	void OnDrawGizmosSelected() {
		if (m_spawnAtTransform != null) {
			Gizmos.color = Colors.coral;
			for (int i = 0; i < m_spawnAtTransform.Length; i++) {
				Gizmos.DrawSphere(m_spawnAtTransform[i].position, 0.5f);
			}
		}
	}	
	//-------------------------------------------------------------------
}
