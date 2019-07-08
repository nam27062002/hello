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
		public string			variant;
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

	private bool m_useBlood = true;

	//---------------------------------------------------------------//
	//-- Static Methods ---------------------------------------------//
	//---------------------------------------------------------------//

	public static void PreBuild() {
        Clear();
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

	/// <summary>
	/// Will clear all unused particles on all pools.
	/// </summary>
	public static void ClearUnsued(){
		instance.__ClearUnsued();
	}



	//---------------------------------------------------------------//
	//-- Instance Methods -------------------------------------------//
	//---------------------------------------------------------------//

	void Update() {
		#if PRINT_POOLS						
		m_printTimer -= Time.deltaTime;
		if (m_printTimer <= 0f) {
			if (LevelManager.currentLevelData != null) {
				string fileName = "PM_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea + ".xml";
				using (StreamWriter sw = new StreamWriter(fileName, false)) {
					sw.WriteLine("<Definitions>");
					foreach (KeyValuePair<string, PoolContainer> pair in m_pools) {
						if (pair.Value.pool != null) {
							sw.WriteLine("<Definition sku=\"" + pair.Key + "\" poolSize=\"" + pair.Value.pool.Size() + "\"/>");
						}
					}
					sw.WriteLine("</Definitions>");
					sw.Close();
				}
			}
			m_printTimer = 10f;
		}
		#endif

		for (int i = 0; i < m_iterator.Count; i++) {
			m_iterator[i].Update();
		}
	}

	private void __PreBuild() {
        m_useBlood = FeatureSettingsManager.instance.IsBloodEnabled();
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
            pc = new PoolContainer {
                handler = new ParticleHandler()
            };

            m_pools.Add(def.sku, pc);
		}

		// default values
		pc.variant = "Master";
		pc.size = def.GetAsInt("count");

        // npew read the current profile
		switch(FeatureSettingsManager.instance.Particles) {
			case FeatureSettings.ELevel5Values.very_low:							
            case FeatureSettings.ELevel5Values.low:              CheckLow(def, ref pc);	break;
            case FeatureSettings.ELevel5Values.high:            CheckHigh(def, ref pc);	break;				
			case FeatureSettings.ELevel5Values.very_high:   CheckVeryHigh(def, ref pc);	break;
		}

		// size is 0 if we dont want to use blood and the particle is blood
		if (!m_useBlood && def.GetAsBool("isBlood", false)) {
			pc.size = 0;
		}

		return pc;
	}

    private void CheckVeryHigh(DefinitionNode _def, ref PoolContainer _pc) {
        bool checkLowerLevel = true;
        if (_def.GetAsBool("veryHighVersion")) {
            _pc.variant = "VeryHigh";
            checkLowerLevel = false;
        }

        if (_def.Has("countVeryHigh")) {
            _pc.size = _def.GetAsInt("countVeryHigh");
            checkLowerLevel = false;
        }

        if (checkLowerLevel) {
            CheckHigh(_def, ref _pc);
        }
    }

    private void CheckHigh(DefinitionNode _def, ref PoolContainer _pc) {
        if (_def.GetAsBool("highVersion")) {
            _pc.variant = "High";
        }
        _pc.size = _def.GetAsInt("countHigh", _pc.size);
    }

    private void CheckLow(DefinitionNode _def, ref PoolContainer _pc) {
        if (_def.GetAsBool("lowVersion")) {
            _pc.variant = "Low";
        }
        _pc.size = _def.GetAsInt("countLow", _pc.size);
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
				if ( container.size < 0 )
					container.size = 1;
			} else {
				container = new PoolContainer();
				container.handler = new ParticleHandler();
				container.size = 1;
				container.variant = "Master";

				m_pools.Add(_prefabName, container);
			}

			if (container.pool == null) {				
                GameObject go = HDAddressablesManager.Instance.LoadAsset<GameObject>(_prefabName, container.variant);

                if (go != null) {
					if (m_poolLimits == PoolLimits.Unlimited) {
						container.pool = new Pool(go, _prefabName, container.variant, instance.transform, 1, true, true, true);
					} else {
						container.pool = new Pool(go, _prefabName, container.variant, instance.transform, container.size, false, true, true);
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
		m_pools.Clear();
		m_iterator.Clear();
	}

	private void __ClearUnsued() {
		foreach(KeyValuePair<string, PoolContainer> pair in m_pools) {
			PoolContainer pc = pair.Value;
			if (pc.pool != null) {
				pc.pool.ClearFreeInstances();
			}
		}
	}


	#region utils
	/// <summary>
	/// Inits the leveled particle. Search a particle with name "particle" from Particles levels to very low and, if found, instantiated it and sets _anchor as parent
	/// </summary>
	/// <returns>The leveled particle.</returns>
	/// <param name="particle">Particle.</param>
	/// <param name="_anchor">Anchor.</param>
	public static ParticleSystem InitLeveledParticle(string _particle, Transform _anchor)
	{
		ParticleSystem ret = null;
        string variant = GetVariant(_particle);
        if (!string.IsNullOrEmpty(variant)) {
            GameObject go = HDAddressablesManager.Instance.LoadAsset<GameObject>(_particle, variant);
            if (go != null) {
                 ret = InitParticle(go,  _anchor);
            }
        }
        return ret;
	}

    /// <summary>
    /// Returns the variant to use for the particle passed as a parameter
    /// </summary>
    /// <param name="_particle">Particle id which variant is requested. This id is defined in addressables catalog.</param>
    /// <returns>Returns the variant to use for the particle passed as a parameter</returns>
    public static string GetVariant(string _particle)
    {
        string ret = null;
        for (FeatureSettings.ELevel5Values level = FeatureSettingsManager.instance.Particles;
            level >= FeatureSettings.ELevel5Values.very_low && ret == null;
            level = level - 1)
        {
            string variant = "";
            switch (level)
            {
                //	path = "Particles/VeryLow/";
                // break;
                case FeatureSettings.ELevel5Values.very_low:
                case FeatureSettings.ELevel5Values.low:
                    variant = "Low";
                    break;
                case FeatureSettings.ELevel5Values.mid:
                    variant = "Master";
                    break;
                case FeatureSettings.ELevel5Values.high:
                    variant = "High";
                    break;
                case FeatureSettings.ELevel5Values.very_high:
                    variant = "VeryHigh";
                    break;
            }

            if (!string.IsNullOrEmpty(variant) && HDAddressablesManager.Instance.ExistsResource(_particle, variant))
            {
                ret = variant;                    
            }
        }

        return ret;
    }

    public static GameObject InitLeveledParticleObject(string _particle, Transform _anchor) {
        GameObject ret = null;
        for (FeatureSettings.ELevel5Values level = FeatureSettingsManager.instance.Particles;
             level >= FeatureSettings.ELevel5Values.very_low && ret == null;
             level = level - 1)
        {
            string variant = "";
            switch (level) {
                case FeatureSettings.ELevel5Values.very_low:
                case FeatureSettings.ELevel5Values.low:
                variant = "Low";
                break;
                case FeatureSettings.ELevel5Values.mid:
                variant = "Master";
                break;
                case FeatureSettings.ELevel5Values.high:
                variant = "High";
                break;
                case FeatureSettings.ELevel5Values.very_high:
                variant = "VeryHigh";
                break;
            }

            if (!string.IsNullOrEmpty(variant)) {
                GameObject go = HDAddressablesManager.Instance.LoadAsset<GameObject>(_particle, variant);

                if (go != null) {
                    ret = Instantiate(go);
                    ret.transform.SetParentAndReset(_anchor);
                    ret.SetActive(false);
                }
            }
        }
        return ret;
    }
    

	/// <summary>
	/// Inits the particle. Instantiates _prefa, stops particle and sets _anchor as parent
	/// </summary>
	/// <returns>The particle.</returns>
	/// <param name="_prefab">Prefab.</param>
	/// <param name="_anchor">Anchor.</param>
	public static ParticleSystem InitParticle(GameObject _prefab, Transform _anchor) {
		if(_prefab == null) return null;

		GameObject go = Instantiate(_prefab);
		ParticleSystem psInstance = go.GetComponent<ParticleSystem>();
		if(psInstance != null) {
			psInstance.transform.SetParentAndReset(_anchor);
			psInstance.Stop();
			go.SetActive(false);
		}
		return psInstance;
	}
	#endregion

}
