using UnityEngine;
using System.Collections.Generic;

public class ParticleManager : SingletonMonoBehaviour<ParticleManager> {
	
	private Dictionary<string, Pool> m_particles = new Dictionary<string, Pool>();

	public static GameObject Spawn(string _id, Vector3 _at) {

		GameObject system = null;

		if(!instance.m_particles.ContainsKey(_id)) {
			CreatePool(_id);
		}

		system = instance.m_particles[_id].Get();
	
		if (system != null) {
			system.transform.localPosition = Vector3.zero;
			system.transform.position = _at;
			system.GetComponent<ParticleSystem>().Clear();
			system.GetComponent<ParticleSystem>().Play();
		}
		
		return system;
	}


	private static void CreatePool(string _id) {

		GameObject prefab = (GameObject)Resources.Load("Particles/" + _id);
		Pool pool = new Pool(prefab, instance.transform, 5, false, true);
		instance.m_particles.Add(_id, pool);
	}

	public static void Clear() {
		instance.m_particles.Clear();
	}

	private void OnLevelWasLoaded() {
		// Clear the manager with every new scene
		// Avoid creating new pools in the Awake calls, do it on the Start at least
		Clear();
	}
}
