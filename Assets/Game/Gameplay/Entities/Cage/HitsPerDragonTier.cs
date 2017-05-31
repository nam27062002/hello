using System.Collections.Generic;
using System;

[Serializable]
public class Hit {
	public int count;
	public bool needBoost;
}

[Serializable]
public class HitsPerDragonTier : SerializableDictionary<DragonTier, Hit> { 
	
	public DragonTier GetMinTier() {
		DragonTier first = DragonTier.COUNT;

		for (int i = 0; i < keyList.Count; ++i) {
			DragonTier key = keyList[i];
			if (m_dict[key].count > 0) {
				if (key < first) {
					first = key;
				}
			}
		}

		return first;
	}
}
