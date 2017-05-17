using System.Collections.Generic;
using System;

[Serializable]
public class Hit {
	public int count;
	public bool needBoost;
}

[Serializable]
public class HitsPerDragonTier : SerializableDictionary<DragonTier, Hit> { }
