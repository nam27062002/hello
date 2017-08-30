using UnityEngine;

public class ScaleEquipableParticle : MonoBehaviour {

	public bool m_addToBodyParts = false;
	public bool m_stopInsideWater = false;
	public bool m_stopWhenDead = false;

	public void Setup()
	{
		ParticleScaler scaler = GetComponentInChildren<ParticleScaler>();
		scaler.m_scaleOrigin = ParticleScaler.ScaleOrigin.TRANSFORM_SCALE;
		scaler.m_whenScale = ParticleScaler.WhenScale.ENABLE;
		scaler.m_transform = GetComponentInParent<DragonEquip>().transform;
		scaler.DoScale();
	}

}
