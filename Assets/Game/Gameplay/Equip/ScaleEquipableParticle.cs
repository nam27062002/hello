using UnityEngine;

public class ScaleEquipableParticle : MonoBehaviour {

	public bool m_addToBodyParts = false;
	public bool m_stopInsideWater = false;
	public bool m_stopWhenDead = false;

	void Start()
	{
		Transform _tr;
		DragonEquip _dragonEquip = GetComponentInParent<DragonEquip>();
		if ( _dragonEquip )
		{
			_tr = _dragonEquip.transform;
		}
		else
		{
			DragonCorpse _corpse = GetComponentInParent<DragonCorpse>();
			if ( _corpse )
			{
				_tr = _corpse.transform;
			}
			else
			{
				_tr = transform;
			}
		}
		Setup( _tr );


		if ( m_addToBodyParts && _dragonEquip)
		{
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

	public void Setup( Transform _tr )
	{
		ParticleScaler scaler = GetComponentInChildren<ParticleScaler>();
		if ( scaler != null )
		{
			scaler.m_scaleOrigin = ParticleScaler.ScaleOrigin.TRANSFORM_SCALE;
			scaler.m_whenScale = ParticleScaler.WhenScale.ENABLE;
			scaler.m_transform = _tr;
			scaler.DoScale();
		}
	}

}
