using UnityEngine;
using System.Collections.Generic;

public class ParticleController : MonoBehaviour {
	
	Dictionary<string, Pool> m_particles;

	void Awake () {

		m_particles = new Dictionary<string, Pool>();				
		InstanceManager.particles = this;
	}

	public GameObject Spaw(string _id, Vector3 _at) {

		GameObject system = null;

		if (!m_particles.ContainsKey(_id)) {
			CreatePool(_id);
		}

		system = m_particles[_id].Get();
	
		if (system != null) {
			system.transform.localPosition = Vector3.zero;
			system.transform.position = _at;
			system.GetComponent<ParticleSystem>().Clear();
			system.GetComponent<ParticleSystem>().Play();
		}
		
		return system;
	}


	private void CreatePool(string _id) {
		
		GameObject prefab = (GameObject)Resources.Load("Particles/" + _id);
		Pool pool = new Pool(prefab, transform, 5, false);
		m_particles.Add(_id, pool);
	}
}
