using UnityEngine;
using System.Collections.Generic;

public class ZoneManager : MonoBehaviour {

	public enum ZoneEffect {
		None = 0,
		S, // feedback
		M, // burn / destroy with KB or collide 
		L  // disintegrate
	};

	public enum Zone {
		None = 0,
		Zone1,
		Zone2
	};

	[SerializeField] private float m_zone1Size;
	[SerializeField] private float m_zone2Size;

	[SeparatorAttribute]
	[SerializeField] private Color m_zone1Color = Colors.WithAlpha(Colors.paleYellow, 0.45f);
	[SerializeField] private Color m_zone2Color = Colors.WithAlpha(Colors.maroon, 0.25f);
	public Color zone1Color { get { return m_zone1Color; } }
	public Color zone2Color { get { return m_zone2Color; } }


	//----------------------------------------------------------------------------------------------------------------------------------------------------------//
	void OnEnable() {
		InstanceManager.zoneManager = this;	
	}

	void OnDisable() {
		if (ApplicationManager.IsAlive)
			InstanceManager.zoneManager = null;		
	}

	public ZoneEffect GetFireEffectCode(Decoration _deco, DragonTier _tier) {		
		if (_tier >= _deco.tier) {
			int distance = ((int)_tier) - ((int)_deco.tier);
			if (distance >= 2) {
				return ZoneEffect.L;		
			} else {
				return ZoneEffect.M;
			}
		} else {
			return ZoneEffect.S;
		}
	}

	public ZoneEffect GetSuperFireEffectCode(Decoration _deco, DragonTier _tier) {		
		if (_tier >= _deco.tier) {
			return ZoneEffect.L;
		} else {
			int distance = ((int)_deco.tier) - ((int)_tier);
			if (distance == 1) {
				return ZoneEffect.M;
			} else {
				return ZoneEffect.S;
			}
		}
	}

	public ZoneEffect GetDestructionEffectCode(Decoration _deco, DragonTier _tier) {		
		if (_tier >= _deco.tier) {			
			return ZoneEffect.M;
		} else {
			return ZoneEffect.S;
		}
	}

	public Zone GetZone(float _z) {
		if (_z > -m_zone1Size * 0.5f) {
			if (_z < m_zone1Size * 0.5f) {
				return Zone.Zone1;
			}
			if (_z < (m_zone1Size * 0.5f + m_zone2Size)) {
				return Zone.Zone2;
			}
		}
		return Zone.None;
	}

	//----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnDrawGizmosSelected() {
		Rect mapBounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelData data = LevelManager.currentLevelData;
		if (data != null) {
			mapBounds = data.bounds;
		}

		Vector3 centerZone1 = (Vector3)mapBounds.center;
		Vector3 centerZone2 = centerZone1 + Vector3.forward * (m_zone1Size + m_zone2Size) * 0.5f;

		Vector3 sizeZone1 = (Vector3)mapBounds.size + Vector3.forward * m_zone1Size;
		Vector3 sizeZone2 = (Vector3)mapBounds.size + Vector3.forward * m_zone2Size;

		Gizmos.color = m_zone1Color;
		Gizmos.DrawCube(centerZone1, sizeZone1);
		Gizmos.color = m_zone2Color;
		Gizmos.DrawCube(centerZone2, sizeZone2);
	}
}