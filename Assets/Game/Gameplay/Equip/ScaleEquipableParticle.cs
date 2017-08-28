using UnityEngine;
using System.Collections;

public class ScaleEquipableParticle : MonoBehaviour {
	
	public void Setup()
	{
		ParticleScaler scaler = GetComponentInChildren<ParticleScaler>();
		scaler.m_scaleOrigin = ParticleScaler.ScaleOrigin.TRANSFORM_SCALE;
		scaler.m_whenScale = ParticleScaler.WhenScale.ENABLE;
		scaler.m_transform = GetComponentInParent<DragonEquip>().transform;
		scaler.DoScale();
	}

}
