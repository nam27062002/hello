using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSpawnPoint : MonoBehaviour {
	[SerializeField] private bool m_enableCandleEffect;
	[SerializeField] private HUDDarkZoneEffect.CandleData m_candleData;


	public void Spawn() {
		if (m_enableCandleEffect) {
    		HUDDarkZoneEffect.hotInitialize(m_candleData);
		}
	}
}
