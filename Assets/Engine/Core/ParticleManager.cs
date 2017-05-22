//#define PRINT_POOLS

using UnityEngine;
using System.Collections.Generic;

#if PRINT_POOLS
	using System;
	using System.IO;
#endif

public class ParticleManager : UbiBCN.SingletonMonoBehaviour<ParticleManager> {
	// Pool of pools! :D
	private Dictionary<string, Pool> m_particlePools = new Dictionary<string, Pool>();
	private Dictionary<string, int> m_poolSize = new Dictionary<string, int>();
	private List<Pool> m_iterator = new List<Pool>();

	private float m_printTimer = 10f;

	private bool m_useAreaLimits = true;

	void Awake(){
		// if we are in game we use the limits, otherwise ( Level Editor ), we let pools grow
		m_useAreaLimits = !(InstanceManager.sceneController is LevelEditor.LevelEditorSceneController);	
		#if PRINT_POOLS	
			m_useAreaLimits = false;
		#endif
	}

	void Update() {
		#if PRINT_POOLS						
		m_printTimer -= Time.deltaTime;
		if (m_printTimer <= 0f) {
			string fileName = "PM_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea + ".xml";
			StreamWriter sw = new StreamWriter(fileName, false);
			sw.WriteLine("<Definitions>");
			foreach (KeyValuePair<string, Pool> pair in m_particlePools) {
				sw.WriteLine("<Definition sku=\"" + pair.Key + "\" poolSize=\"" + pair.Value.Size() + "\"/>");
			}
			sw.WriteLine("</Definitions>");
			sw.Close();
			m_printTimer = 10f;
		}
		#endif

		for (int i = 0; i < m_iterator.Count; i++) {
			m_iterator[i].Update();
		}
	}

	public static void CreatePool(ParticleData particle) {
		CreatePool( particle.name, particle.path);
	}

	/// <summary>
	/// Preload a particle effect before it is needed in game.
	/// </summary>
	/// <param name="_prefabName">Identifier. Must match the name of the prefab to be used.</param>
	/// <param name="_path">Optional resources path of the prefab to be considerd if no pool with the given ID is found. Folder name within the Resources/Particles/ folder, excluding prefab name (e.g. "Game/Effects")</param>
	/// <param name="_size">Size of the pool.</param>
	public static void CreatePool(string _prefabName, string _folderPath = "") {
		if (!instance.m_particlePools.ContainsKey(_prefabName)) {
			// [AOC] Small hack for retrocompatibility
			if (!string.IsNullOrEmpty(_folderPath)) {
				if (!_folderPath.EndsWith("/")) _folderPath = _folderPath + "/";
			}

			GameObject prefab = (GameObject)Resources.Load("Particles/" + _folderPath + _prefabName);
			CreatePool(prefab);
		}
	}

	public static GameObject Spawn(ParticleData particle, Vector3 _at = default(Vector3)){
		// If we don't have a pool with the given ID, create it
		CreatePool(particle.name, particle.path);

		// Get a new system from the pool, spawn it and return it
		if (instance.m_particlePools.ContainsKey(particle.name)) {
			GameObject system = instance.m_particlePools[particle.name].Get(true);
			SpawnSystem(system, particle, _at);
			return system;
		}

		return null;
	}

	/// <summary>
	/// Spawn a particle effect with the given ID at a world position.
	/// If no pool was found with the given ID, load a prefab from a target path
	/// and create a pool from it.
	/// </summary>
	/// <returns>The spawned game object.</returns>
	/// <param name="_prefabName">Identifier. Must match the name of the prefab to be used.</param>
	/// <param name="_at">World position where to spawn the particle system.</param>
	/// <param name="_path">Optional resources path of the prefab to be considerd if no pool with the given ID is found. Folder name within the Resources/Particles/ folder, excluding prefab name (e.g. "Game/Effects")</param>
	public static GameObject Spawn(string _prefabName, Vector3 _at = default(Vector3), string _folderPath = "") {
		// If we don't have a pool with the given ID, create it
		CreatePool(_prefabName, _folderPath);

		// Get a new system from the pool, spawn it and return it
		if (instance.m_particlePools.ContainsKey(_prefabName)) {
			GameObject system = instance.m_particlePools[_prefabName].Get(true);
			SpawnSystem(system, _at);
			return system;
		}

		return null;
	}

	/// <summary>
	/// Spawn a particle effect with the given prefab at a world position.
	/// If no pool was found with the prefab's name, use the given it to create a new pool.
	/// </summary>
	/// <returns>The spawned game object.</returns>
	/// <param name="_prefab">Prefab to be used. If no pool with the prefab's name is found, a new one will be created..</param>
	/// <param name="_at">World position where to spawn the particle system.</param>
	public static GameObject Spawn(GameObject _prefab, Vector3 _at = default(Vector3)) {
		// Ignore if given prefab is not valid
		if (_prefab == null) return null;

		// If we don't have a pool with the given prefab, create it
		if (!instance.m_particlePools.ContainsKey(_prefab.name)) {
			CreatePool(_prefab);
		}

		// Get a new system from the pool, spawn it and return it
		GameObject system = instance.m_particlePools[_prefab.name].Get(true);
		SpawnSystem(system, _at);
		return system;
	}

	/// <summary>
	/// Spawn the given system at a target location.
	/// Internal use only.
	/// </summary>
	/// <param name="_system">The particle system to be spawned.</param>
	/// <param name="_at">The world position where to spawn the particle system.</param>
	private static void SpawnSystem(GameObject _system, Vector3 _at) {
		// Skip if system is not valid
		if (_system == null) return;

		// Reset system's position
		_system.transform.localPosition = Vector3.zero;
		_system.transform.position = _at;

		// Restart all particle systems within the instance
		List<ParticleSystem> subsystems = _system.transform.FindComponentsRecursive<ParticleSystem>();
		for (int i = 0; i < subsystems.Count; i++) {
			subsystems[i].Clear();
			ParticleSystem.EmissionModule em = subsystems[i].emission;
			em.enabled = true;
			subsystems[i].Play();
		}
	}

	private static void SpawnSystem(GameObject _system, ParticleData particle, Vector3 _at) {
		// Skip if system is not valid
		if (_system == null) return;

		// Reset system's position
		_system.transform.localPosition = Vector3.zero;
		_system.transform.position = _at;

		// Restart all particle systems within the instance
		List<ParticleSystem> subsystems = _system.transform.FindComponentsRecursive<ParticleSystem>();
		for (int i = 0; i < subsystems.Count; i++) {
			subsystems[i].Clear();

			if (particle.changeStartColor) {				
				ParticleSystem.MainModule main = subsystems[i].main;
				ParticleSystem.MinMaxGradient gradient = main.startColor;
				gradient.color = particle.startColor;
				main.startColor = gradient;
			}

			if (particle.changeColorOvertime) {
				ParticleSystem.ColorOverLifetimeModule colorOverLifetime = subsystems[i].colorOverLifetime;
				ParticleSystem.MinMaxGradient gradient = colorOverLifetime.color;
				gradient.gradient = particle.colorOvertime;
				colorOverLifetime.color = gradient;
			}

			ParticleSystem.EmissionModule em = subsystems[i].emission;
			em.enabled = true;
			subsystems[i].Play();
		}

		ParticleScaler scaler = _system.GetComponent<ParticleScaler>();
		if (scaler != null) {
			if (scaler.m_scale != particle.scale) {
				scaler.m_scale = particle.scale;
				scaler.DoScale();
			}
		}
	}

	/// <summary>
	/// Return the instance to the pool
	/// </summary>
	public static void ReturnInstance(GameObject _system) {
		// Make sure we actually have a pool for this system!
		if (instance.m_particlePools.ContainsKey(_system.name)) {
			instance.m_particlePools[_system.name].Return(_system);
		}
	}

	/// <summary>
	/// Create a pool using the given prefab.
	/// The id of the new pool will be the prefab's name.
	/// </summary>
	/// <param name="_prefab">The prefab to be used to create the pool.</param>
	/// <param name="_size">Size of the pool.</param>
	private static void CreatePool(GameObject _prefab) {
		// Ignore if given prefab is not valid
		if (_prefab == null) return;

		// If a pool with the given name already exists, ignore
		if (instance.m_particlePools.ContainsKey(_prefab.name)) return;

		Pool pool = null;
		if (instance.m_useAreaLimits)
		{	
			if (instance.m_poolSize.Count == 0f) {
				if (LevelManager.currentLevelData != null) {
					string category = "PARTICLE_MANAGER_SETTINGS_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea;
					List<DefinitionNode> poolSize = DefinitionsManager.SharedInstance.GetDefinitionsList(category.ToUpper());
					for (int i = 0; i < poolSize.Count; i++) {
						instance.m_poolSize.Add(poolSize[i].sku, poolSize[i].GetAsInt("poolSize"));
					}
				}
			}

			int size = 1;
			if (instance.m_poolSize.ContainsKey(_prefab.name)) {
				size = instance.m_poolSize[_prefab.name];
			} else {
				Debug.LogError("[ParticleManager] system " + _prefab.name + " not found in definitions. Cretaing only 1 instance.");
			}
			pool = new Pool(_prefab, instance.transform, size, false, true, true);
		}
		else
		{
			pool = new Pool(_prefab, instance.transform, 1, true, true, true);	
		}

		instance.m_particlePools.Add(_prefab.name, pool);
		instance.m_iterator.Add(pool);
	}

	/// <summary>
	/// Change the size of the pool with the given ID.
	/// </summary>
	/// <param name="_id">ID of the pool to be resized.</param>
	/// <param name="_newSize">New size for the target pool.</param>
	public static void ResizePool(string _id, int _newSize) {
		// Find pool
		if (!instance.m_particlePools.ContainsKey(_id)) return;

		// Change pool's size
		instance.m_particlePools[_id].Resize(_newSize);
	}

	/// <summary>
	/// Clear all particle pools.
	/// </summary>
	public static void Clear() {
        if (instance.m_particlePools != null) {
            foreach (KeyValuePair<string, Pool> pair in instance.m_particlePools) {
                pair.Value.Clear();
            }

            instance.m_particlePools.Clear();
			instance.m_iterator.Clear();
        }
		instance.m_poolSize.Clear();
	}

	/// <summary>
	/// A new scene was loaded
	/// </summary>
	private void OnLevelWasLoaded() {
		// Clear the manager with every new scene
		// Avoid creating new pools in the Awake calls, do it on the Start at least
		Clear();
	}
}
