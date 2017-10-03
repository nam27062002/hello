using UnityEngine;

public class ScaleEquipableParticle : MonoBehaviour {

	public bool m_addToBodyParts = false;
	public bool m_stopInsideWater = false;
	public bool m_stopWhenDead = false;

	void Start()
	{
		Setup();
		if ( m_addToBodyParts )
		{
			DragonEquip _dragonEquip = GetComponentInParent<DragonEquip>();
			DragonParticleController particleController = _dragonEquip.GetComponentInChildren<DragonParticleController>();
			if ( particleController )
			{
				DragonParticleController.BodyParticle bParticle = new DragonParticleController.BodyParticle();
				bParticle.m_stopInsideWater = m_stopInsideWater;
				bParticle.m_stopWhenDead = m_stopWhenDead;
				bParticle.m_particleReference = GetComponentInChildren<ParticleSystem>();
				particleController.m_bodyParticles.Add( bParticle );
			}
		}
	}

	public void Setup()
	{
		ParticleScaler scaler = GetComponentInChildren<ParticleScaler>();
		scaler.m_scaleOrigin = ParticleScaler.ScaleOrigin.TRANSFORM_SCALE;
		scaler.m_whenScale = ParticleScaler.WhenScale.ENABLE;
		scaler.m_transform = GetComponentInParent<DragonEquip>().transform;
		scaler.DoScale();
	}

}
