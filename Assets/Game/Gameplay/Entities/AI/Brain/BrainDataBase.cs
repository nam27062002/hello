using UnityEngine;
using System.Collections.Generic;

namespace AI {
	public class BrainDataBase : Singleton<BrainDataBase> {
		
		private Dictionary<string,  Dictionary<string, AIPilot.StateComponentDataKVP>> m_dataBase;


		//------------------------------------------------------------------------//
		// GENERIC METHODS														  //
		//------------------------------------------------------------------------//
		public BrainDataBase() {
			m_dataBase = new Dictionary<string, Dictionary<string, AIPilot.StateComponentDataKVP>>();
		}

		protected override void OnApplicationQuit() {
			
		}

		public bool HasDataFor(string _key) {
			return m_dataBase.ContainsKey(_key) && m_dataBase[_key].Count > 0;
		}

		public bool HasDataFor(string _key, string _typeName) {
			return m_dataBase.ContainsKey(_key) && m_dataBase[_key].ContainsKey(_typeName);
		}

		public void ClearDataFor(string _key) {
		//	if (m_dataBase.ContainsKey(_key)) {
		//		m_dataBase.Remove(_key);
		//	}
		}

		public void ClearDataFor(string _key, string _typeName) {
		//	if (m_dataBase.ContainsKey(_key)) {
		//		if (m_dataBase[_key].ContainsKey(_typeName)) {
		//			m_dataBase[_key].Remove(_typeName);
		//		}
		//	}
		}

		public void AddDataFor(string _key, AIPilot.StateComponentDataKVP _data) {
			if (!m_dataBase.ContainsKey(_key)) {
				m_dataBase[_key] = new Dictionary<string, AIPilot.StateComponentDataKVP>();
			}
			m_dataBase[_key][_data.typeName] = _data;
		}

		public Dictionary<string, AIPilot.StateComponentDataKVP> GetDataFor(string _key) {
			if (m_dataBase.ContainsKey(_key)) {
				return m_dataBase[_key];
			}
			return null;
		}

		public AIPilot.StateComponentDataKVP GetDataFor(string _key, string _typeName) {
			if (m_dataBase.ContainsKey(_key)) {
				Dictionary<string, AIPilot.StateComponentDataKVP> data = m_dataBase[_key];

				if (data.ContainsKey(_typeName)) {
					return data[_typeName];
				}
			}
			return null;
		}

		public void ValidateTypes(string _key, HashSet<string> _typeNames) {
			if (m_dataBase.ContainsKey(_key)) {
				Dictionary<string, AIPilot.StateComponentDataKVP> data = m_dataBase[_key];

				List<string> keysToDelete = new List<string>();
				foreach(string key in data.Keys) {
					if (!_typeNames.Contains(key)) {
						keysToDelete.Add(key);
					}
				}

				for (int i = 0; i < keysToDelete.Count; i++) {
					data.Remove(keysToDelete[i]);
				}
			}
		}
	}
}