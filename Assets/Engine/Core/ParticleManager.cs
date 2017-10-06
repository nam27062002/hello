//#define PRINT_POOLS

using UnityEngine;
using System.Collections.Generic;

#if PRINT_POOLS
	using System;
	using System.IO;
#endif

public class ParticleManager : UbiBCN.SingletonMonoBehaviour<ParticleManager> {
	private class PoolContainer {
		public Pool				pool;
		public ParticleHandler	handler;
		public int				size;
		public string			version;
	}

	public enum PoolLimits {
		Unlimited = 0,
		LoadedArea,
		LevelEditor
	}

	// Pool of pools! :D
	private Dictionary<string, PoolContainer> m_pools = new Dictionary<string, PoolContainer>();
	private List<Pool> m_iterator = new List<Pool>();

	#if PRINT_POOLS	
	private float m_printTimer = 10f;
	#endif

	private PoolLimits m_poolLimits = PoolLimits.Unlimited;
	public PoolLimits poolLimits {
		get { return m_poolLimits; }
		set { m_poolLimits = value; }
	}



	//---------------------------------------------------------------//
	//-- Static Methods ---------------------------------------------//
	//---------------------------------------------------------------//

	public static void PreBuild() { 
		instance.__PreBuild();
	}

	public static void Rebuild(){
		instance.__Rebuild();
	}


	public static ParticleHandler CreatePool(ParticleData _data) {
		return instance.__CreatePool(_data.name, _data.path);
	}

	/// <summary>
	/// Preload a particle effect before it is needed in game.
	/// </summary>
	/// <param name="_prefabName">Identifier. Must match the name of the prefab to be used.</param>
	/// <param name="_path">Optional resources path of the prefab to be considerd if no pool with the given ID is found. Folder name within the Resources/Particles/ folder, excluding prefab name (e.g. "Game/Effects")</param>
	public static ParticleHandler CreatePool(string _prefabName, string _folderPath = "") {
		return instance.__CreatePool(_prefabName, _folderPath);
	}

	public static ParticleHandler GetHandler(ParticleData _data) {
		if (instance.m_pools.ContainsKey(_data.name)) {
			return instance.m_pools[_data.name].handler;
		}
		return null;
	}

	public static ParticleHandler GetHandler(string _prefabName) {
		if (instance.m_pools.ContainsKey(_prefabName)) {
			return instance.m_pools[_prefabName].handler;
		}
		return null;
	}

	/// <summary>
	/// Will destroy all the pools and loose reference to any created instances.
	/// Additionally they can be deleted from the scene.
	/// </summary>
	public static void Clear() { 
		instance.__Clear();
	}



	//---------------------------------------------------------------//
	//-- Instance Methods -------------------------------------------//
	//---------------------------------------------------------------//

	void Update() {
		#if PRINT_POOLS						
		m_printTimer -= Time.deltaTime;
		if (m_printTimer <= 0f) {
			string fileName = "PM_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea + ".xml";
			StreamWriter sw = new StreamWriter(fileName, false);
			sw.WriteLine("<Definitions>");
			foreach (KeyValuePair<string, PoolContainer> pair in m_pools) {
				sw.WriteLine("<Definition sku=\"" + pair.Key + "\" poolSize=\"" + pair.Value.pool.Size() + "\"/>");
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

	private void __PreBuild() {
		if (m_poolLimits != PoolLimits.Unlimited) {
			if (LevelManager.currentLevelData != null) {
				List<DefinitionNode> poolSizes = GetPoolSizesForCurrentArea();
				for (int i = 0; i < poolSizes.Count; i++) {
					DefinitionNode def = poolSizes[i];
					ResetPoolContainerSize(def);
				}
			}
		}
	}

	protected List<DefinitionNode> GetPoolSizesForCurrentArea(){
		string category = "";
		if (m_poolLimits == PoolLimits.LoadedArea) {
			category = "PARTICLE_MANAGER_SETTINGS_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea;
		} else if (m_poolLimits == PoolLimits.LevelEditor) {
			category = LevelEditor.LevelEditor.settings.poolLimit;
		}
		List<DefinitionNode> poolSizes = DefinitionsManager.SharedInstance.GetDefinitionsList(category.ToUpper());
		return poolSizes;
	}

	private PoolContainer ResetPoolContainerSize( DefinitionNode def ){
		PoolContainer pc = null;
		if (m_pools.ContainsKey(def.sku)) {
			pc = m_pools[def.sku];
		} else {
			pc = new PoolContainer();
			pc.handler = new ParticleHandler();

			m_pools.Add(def.sku, pc);
		}

		// default values
		pc.version = "Master/";
		pc.size = def.GetAsInt("count");

		// npew read the current profile
		switch(FeatureSettingsManager.instance.Particles) {
			case FeatureSettings.ELevel5Values.very_low:							
			case FeatureSettings.ELevel5Values.low:
				if (def.GetAsBool("lowVersion")) {
					pc.version = "Low/";
				}
				pc.size = def.GetAsInt("countLow", pc.size);
				break;

			case FeatureSettings.ELevel5Values.high:
				if (def.GetAsBool("highVersion")) {
					pc.version = "High/";
				}
				pc.size = def.GetAsInt("countHigh", pc.size);
				break;
				
			case FeatureSettings.ELevel5Values.very_high:
				if (def.GetAsBool("veryHighVersion")) {
					pc.version = "VeryHigh/";
				}
				pc.size = def.GetAsInt("countVeryHigh", pc.size);
				break;
		}
		return pc;
	}


	/// <summary>
	/// Rebuild. When changin area, this funcion makes the proper changes to adapt to the area
	/// </summary>
	private void __Rebuild() {
		if (m_poolLimits != PoolLimits.Unlimited) {
			if (LevelManager.currentLevelData != null) {
				Dictionary<string, PoolContainer> toDelete = new Dictionary<string, PoolContainer>( m_pools );
				List<DefinitionNode> poolSizes = GetPoolSizesForCurrentArea();
				for (int i = 0; i < poolSizes.Count; i++) {
					DefinitionNode def = poolSizes[i];
					PoolContainer pc = ResetPoolContainerSize(def);
					if ( pc.pool != null ){
						pc.pool.Resize( pc.size );
					}
					// Remove from to delete list!
					toDelete.Remove( def.sku );
				}
				foreach(KeyValuePair<string, PoolContainer> pair in toDelete) {
					PoolContainer pc = m_pools[pair.Key];
					if (pc.pool != null) {
						m_iterator.Remove(pc.pool);
						pc.pool.Clear();
						pc.pool = null;
						pc.handler.Invalidate();
					}
					m_pools.Remove(pair.Key);
				}
				toDelete.Clear();
			}
		}
	}

	private ParticleHandler __CreatePool(string _prefabName, string _folderPath) {
		if (string.IsNullOrEmpty(_prefabName)) {
			return null;
		} else {
			PoolContainer container = null;

			if (m_pools.ContainsKey(_prefabName)) {
				container = m_pools[_prefabName];
				if ( container.size <= 0 )
					container.size = 1;
			} else {
				container = new PoolContainer();
				container.handler = new ParticleHandler();
				container.size = 1;
				container.version = "Master/";

				m_pools.Add(_prefabName, container);
			}

			if (container.pool == null) {
				if (!string.IsNullOrEmpty(_folderPath)) {
					if (!_folderPath.EndsWith("/")) _folderPath = _folderPath + "/";
				}

				GameObject go = Resources.Load<GameObject>("Particles/" + container.version + _folderPath + _prefabName);

				if (go != null) {
					if (m_poolLimits == PoolLimits.Unlimited) {
						container.pool = new Pool(go, instance.transform, 1, true, true, true);
					} else {
						container.pool = new Pool(go, instance.transform, container.size, false, true, true);
					}
					m_iterator.Add(container.pool);
				}
				container.size = 0;
			}

			container.handler.AssignPool(container.pool);

			return container.handler;
		}
	}

	private void __Clear() {
		foreach(KeyValuePair<string, PoolContainer> pair in m_pools) {
			PoolContainer pc = pair.Value;
			if (pc.pool != null) {
				pc.pool.Clear();
				pc.pool = null;
				pc.handler.Invalidate();
			}
		}
		m_iterator.Clear();
	}
}
