using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfilerNPCSceneController : MonoBehaviour {

	public string npcScene;
	public string particleDefinitions;
	public Transform npcRoot;
	public Transform particleSystemRoot;


	private class PS_DATA {
		public string path = "";
		public int quantity = 1;
	}


	private Dictionary<string, GameObject> m_npcs = new Dictionary<string, GameObject>();
	private Dictionary<string, PS_DATA> m_particles = new Dictionary<string, PS_DATA>();
	private string m_category;
	private StreamWriter m_sw;


	public void Build(List<ISpawner> _spawners) {

		string fileName = "dump.txt";
		m_sw = new StreamWriter(fileName, false);
		m_sw.AutoFlush = true;

		if (npcRoot != null) {
			while (npcRoot.childCount > 0) {
				Transform child = npcRoot.GetChild(0);
				child.parent = null;
				GameObject.DestroyImmediate(child.gameObject);
			}
		}
		m_npcs.Clear();

		if (particleSystemRoot != null) {
			while (particleSystemRoot.childCount > 0) {
				Transform child = particleSystemRoot.GetChild(0);
				child.parent = null;
				GameObject.DestroyImmediate(child.gameObject);
			}
		}
		m_particles.Clear();

		m_category = "PARTICLE_MANAGER_SETTINGS_" + particleDefinitions;
		m_category = m_category.ToUpper();

		//lets instantiate one of each NPC			 
		for (int i = 0; i < _spawners.Count; ++i) {
			ISpawner sp = _spawners[i];

			if (sp.GetType() == typeof(Spawner) || sp.GetType() == typeof(SpawnerCage)) {
				Spawner.EntityPrefab[] prefabs = (sp as Spawner).m_entityPrefabList;
				for (int j = 0; j < prefabs.Length; ++j) {
					if (!m_npcs.ContainsKey(prefabs[j].name)) {
						GameObject go = Resources.Load<GameObject>(IEntity.EntityPrefabsPath + prefabs[j].name);
						go = GameObject.Instantiate(go);
						go.transform.SetParent(npcRoot, false);
						m_npcs.Add(prefabs[j].name, go);
					}
				}
			} else if (sp.GetType() == typeof(SpawnerBg)) {
				string name = (sp as SpawnerBg).m_entityPrefabStr;
				if (!m_npcs.ContainsKey(name)) {
					GameObject go = Resources.Load<GameObject>(IEntity.EntityPrefabsPath + name);
					go = GameObject.Instantiate(go);
					go.transform.SetParent(npcRoot, false);
					m_npcs.Add(name, go);
				}
			}
		}

		foreach (GameObject go in m_npcs.Values) {
			go.transform.localPosition = Vector3.zero;
			AnalizeGameObject(go);
		}

		m_sw.Close();

		foreach (string key in m_particles.Keys) {
			PS_DATA data = m_particles[key];
			GameObject go = Resources.Load<GameObject>("Particles/Master/" + data.path + key);
			if (go == null) {
				Debug.Log("null -> Particles/" + data.path + key);
			} else {
				for (int i = 0; i < data.quantity; i++) {
					go = GameObject.Instantiate(go);
					go.name = key;
					go.transform.SetParent(particleSystemRoot, false);
					go.transform.localPosition = Vector3.zero;
				}
			}
		}
	}

	private void AnalizeGameObject(GameObject _go) {
		HashSet<object> exploredObjects = new HashSet<object>();

		// Loops through all components of this game object
		foreach (Component component in _go.GetComponents<Component>()) {
			if (component != null) {
				m_sw.WriteLine("Component: " + component.GetType());

				if (!(component is DG.Tweening.DOTweenAnimation)) {
					FindAllInstancesRecursive(component, exploredObjects);
				}
			}
		}

		Transform t = _go.transform;
		for (int i = 0; i < t.childCount; i++) {
			AnalizeGameObject(t.GetChild(i).gameObject);
		}
	}

	private void FindAllInstancesRecursive(object _value, HashSet<object> _exploredObjects) {
		if (IsNull(_value))
			return;

		if (_exploredObjects.Contains(_value))
			return;

		_exploredObjects.Add(_value);

		IEnumerable enumerable = _value as IEnumerable;

		if (enumerable != null) {
			foreach(object item in enumerable) {
				FindAllInstancesRecursive(item, _exploredObjects);
			}
		} else if (_value is Animator) {
			Animator anim = _value as Animator;
			foreach (SpawnParticleSystem behaviour in anim.GetBehaviours<SpawnParticleSystem>()) {
				FindAllInstancesRecursive(behaviour, _exploredObjects);
			}
		} else {
			if (_value.GetType() == typeof(ParticleData)) {
				AddParticleData(_value as ParticleData);
			} else {
				Type type = _value.GetType();
				FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);

				foreach (FieldInfo fieldInfo in fieldInfos) {
					object propertyValue = fieldInfo.GetValue(_value);

					FindAllInstancesRecursive(propertyValue, _exploredObjects);
				}
			}
		}
	}
		
	private bool IsNull(object _value) {
		if (_value is GameObject) {
			return (_value as GameObject) == null;
		} else if (_value is Component) {
			return (_value as Component) == null;
		}

		return _value == null;
	}

	private void AddParticleData(ParticleData _data) {
		if (_data.IsValid()) {
			if (!m_particles.ContainsKey(_data.name)) {
				PS_DATA psData = new PS_DATA();
				DefinitionNode def =  DefinitionsManager.SharedInstance.GetDefinition(m_category, _data.name);

				if (def != null) {
					psData.quantity = def.GetAsInt("poolSize");
				}

				psData.path = _data.path;
				if (!string.IsNullOrEmpty(psData.path)) {
					if (!psData.path.EndsWith("/")) psData.path = psData.path + "/";
				}

				m_particles.Add(_data.name, psData);
			}
		}
	}
}
