using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent( typeof(ParticleSystem) )]
public class ParticleScaler : MonoBehaviour 
{

	public enum ScaleOrigin
	{
		DRAGON_SIZE,
		TRANSFORM_SCALE,
		ATTRIBUTE_SCALE,
	};

	public ScaleOrigin m_scaleOrigin = ScaleOrigin.DRAGON_SIZE;

	public float m_scale = 1;
	public Transform m_transform;

	public bool m_scaleLifetime = false;
	public bool m_scaleAllChildren = true;

	public enum WhenScale
	{
		START,
		ENABLE,
		AFTER_ENABLE,
		MANUALLY
	}
	public WhenScale m_whenScale;

	public bool m_resetFirst = false;

	protected struct PSDataRegistry
	{
		// Main module
		public float m_startSizeXMultiplier;
		public float m_startSizeYMultiplier;
		public float m_startSizeZMultiplier;
		public float m_gravityModifierMultiplier;
		public float m_startSpeedMultiplier;
		public float m_startLifetimeMultiplier;

		// Shave
		public float m_shapeSize;
		public float m_shapeLengthSize;
		public Vector3 m_boxShapeSize;

		// Velocity Over Lifetime
		public float m_velocityOverLifetimeX;
		public float m_velocityOverLifetimeY;
		public float m_velocityOverLifetimeZ;

		// Limit Velocity Over Lifetime
		public float m_limitVelocityOverLifetimeX;
		public float m_limitVelocityOverLifetimeY;
		public float m_limitVelocityOverLifetimeZ;

		// Force Over Lifetime
		public float m_forceOverLifetimeX;
		public float m_forceOverLifetimeY;
		public float m_forceOverLifetimeZ;
	}
	protected Dictionary<ParticleSystem, PSDataRegistry> m_orignialData = new Dictionary<ParticleSystem, PSDataRegistry>();

	void Awake()
	{
		if ( m_resetFirst )
		{
			// Save original data
			SaveOriginalData();
		}
	}


	// Use this for initialization
	void Start () 
	{
		if ( m_whenScale == WhenScale.START )
			DoScale();
	}

	void SaveOriginalData()
	{
		if ( m_scaleAllChildren )
		{
			ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
			foreach( ParticleSystem p in childs )
				SaveParticleData( p);
		}
		else
		{
			ParticleSystem particle =  GetComponent<ParticleSystem>();
			if (particle)
			{
				SaveParticleData( particle );
			}
		}
	}

	void SaveParticleData( ParticleSystem ps )
	{
		PSDataRegistry data = new PSDataRegistry();
		ParticleSystem.MainModule mainModule = ps.main;
		if ( mainModule.startSize3D )
		{
			data.m_startSizeXMultiplier = mainModule.startSizeXMultiplier;
			data.m_startSizeYMultiplier = mainModule.startSizeYMultiplier;
			data.m_startSizeZMultiplier = mainModule.startSizeZMultiplier;
		}
		else
		{
			data.m_startSizeXMultiplier = mainModule.startSizeMultiplier;	
		}


		data.m_gravityModifierMultiplier = mainModule.gravityModifierMultiplier;
		data.m_startSpeedMultiplier = mainModule.startSpeedMultiplier;
		data.m_startLifetimeMultiplier = mainModule.startLifetimeMultiplier;

		ParticleSystem.ShapeModule shape = ps.shape;
		switch( shape.shapeType )
		{
			case ParticleSystemShapeType.Sphere:
			case ParticleSystemShapeType.SphereShell:
			{
				data.m_shapeSize = shape.radius;
			}break;
			case ParticleSystemShapeType.Hemisphere:
			case ParticleSystemShapeType.HemisphereShell:
			{
				data.m_shapeSize = shape.radius;
			}break;
			case ParticleSystemShapeType.Cone:
			case ParticleSystemShapeType.ConeShell:
			case ParticleSystemShapeType.ConeVolume:
			case ParticleSystemShapeType.ConeVolumeShell:
			{	
				data.m_shapeSize = shape.radius;
				data.m_shapeLengthSize = shape.length;
			}break;
			case ParticleSystemShapeType.Box:
			case ParticleSystemShapeType.BoxShell:
			case ParticleSystemShapeType.BoxEdge:
			{
				data.m_boxShapeSize = shape.box;
			}break;
			case ParticleSystemShapeType.Mesh:
			{
				data.m_shapeSize = shape.meshScale;
			}break;
			case ParticleSystemShapeType.MeshRenderer:
			{
				data.m_shapeSize = shape.meshScale;
			}break;
			case ParticleSystemShapeType.SkinnedMeshRenderer:
			{
				data.m_shapeSize = shape.meshScale;
			}break;
			case ParticleSystemShapeType.CircleEdge:
			case ParticleSystemShapeType.Circle:
			{
				data.m_shapeSize = shape.radius;
			}break;
			case ParticleSystemShapeType.SingleSidedEdge:
			{
				data.m_shapeSize = shape.radius;
			}break;
		}

		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
		data.m_velocityOverLifetimeX = velocityOverLifetime.xMultiplier;
		data.m_velocityOverLifetimeY = velocityOverLifetime.yMultiplier;
		data.m_velocityOverLifetimeZ = velocityOverLifetime.zMultiplier;

		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
		if ( limitVelocityOverLifetime.separateAxes ) 
		{
			data.m_limitVelocityOverLifetimeX = limitVelocityOverLifetime.limitXMultiplier;
			data.m_limitVelocityOverLifetimeY = limitVelocityOverLifetime.limitYMultiplier;
			data.m_limitVelocityOverLifetimeZ = limitVelocityOverLifetime.limitZMultiplier;
		}
		else
		{
			data.m_limitVelocityOverLifetimeX = limitVelocityOverLifetime.limitMultiplier;
		}

		ParticleSystem.ForceOverLifetimeModule forceOverLifetime = ps.forceOverLifetime;
		data.m_forceOverLifetimeX = forceOverLifetime.xMultiplier;
		data.m_forceOverLifetimeY = forceOverLifetime.yMultiplier;
		data.m_forceOverLifetimeZ = forceOverLifetime.zMultiplier;

		if ( m_orignialData.ContainsKey( ps ) )
			m_orignialData[ ps ] = data;
		else
			m_orignialData.Add( ps, data);
	}

	void ResetOriginalData()
	{
		if ( m_scaleAllChildren )
		{
			ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
			foreach( ParticleSystem p in childs )
				ResetParticleData( p );
		}
		else
		{
			ParticleSystem particle =  GetComponent<ParticleSystem>();
			if (particle)
			{
				ResetParticleData( particle );
			}
		}
	}


	void ResetParticleData(ParticleSystem ps)
	{
		if ( m_orignialData.ContainsKey(ps) )
		{
			PSDataRegistry data = m_orignialData[ps];

			ParticleSystem.MainModule mainModule = ps.main;
			if ( mainModule.startSize3D )
			{
				mainModule.startSizeXMultiplier = data.m_startSizeXMultiplier;
				mainModule.startSizeYMultiplier = data.m_startSizeYMultiplier;
				mainModule.startSizeZMultiplier = data.m_startSizeZMultiplier;
			}
			else
			{
				mainModule.startSizeMultiplier = data.m_startSizeXMultiplier;
			}
			mainModule.gravityModifierMultiplier = data.m_gravityModifierMultiplier;
			mainModule.startSpeedMultiplier = data.m_startSpeedMultiplier;
			mainModule.startLifetimeMultiplier = data.m_startLifetimeMultiplier;

			ParticleSystem.ShapeModule shape = ps.shape;
			switch( shape.shapeType )
			{
				case ParticleSystemShapeType.Sphere:
				case ParticleSystemShapeType.SphereShell:
				{
					shape.radius = data.m_shapeSize;
				}break;
				case ParticleSystemShapeType.Hemisphere:
				case ParticleSystemShapeType.HemisphereShell:
				{
					shape.radius = data.m_shapeSize ;
				}break;
				case ParticleSystemShapeType.Cone:
				case ParticleSystemShapeType.ConeShell:
				case ParticleSystemShapeType.ConeVolume:
				case ParticleSystemShapeType.ConeVolumeShell:
				{	
					shape.radius = data.m_shapeSize;
					shape.length = data.m_shapeLengthSize;
				}break;
				case ParticleSystemShapeType.Box:
				case ParticleSystemShapeType.BoxShell:
				case ParticleSystemShapeType.BoxEdge:
				{
					shape.box = data.m_boxShapeSize;
				}break;
				case ParticleSystemShapeType.Mesh:
				{
					shape.meshScale = data.m_shapeSize;
				}break;
				case ParticleSystemShapeType.MeshRenderer:
				{
					shape.meshScale = data.m_shapeSize;
				}break;
				case ParticleSystemShapeType.SkinnedMeshRenderer:
				{
					shape.meshScale = data.m_shapeSize;
				}break;
				case ParticleSystemShapeType.CircleEdge:
				case ParticleSystemShapeType.Circle:
				{
					shape.radius = data.m_shapeSize;
				}break;
				case ParticleSystemShapeType.SingleSidedEdge:
				{
					shape.radius = data.m_shapeSize;
				}break;
			}

			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
			velocityOverLifetime.xMultiplier = data.m_velocityOverLifetimeX;
			velocityOverLifetime.yMultiplier = data.m_velocityOverLifetimeY;
			velocityOverLifetime.zMultiplier = data.m_velocityOverLifetimeZ;

			ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
			if ( limitVelocityOverLifetime.separateAxes )
			{
				limitVelocityOverLifetime.limitXMultiplier = data.m_limitVelocityOverLifetimeX;
				limitVelocityOverLifetime.limitYMultiplier = data.m_limitVelocityOverLifetimeY;
				limitVelocityOverLifetime.limitZMultiplier = data.m_limitVelocityOverLifetimeZ;
			}
			else
			{
				limitVelocityOverLifetime.limitMultiplier = data.m_limitVelocityOverLifetimeX;
			}

			ParticleSystem.ForceOverLifetimeModule forceOverLifetime = ps.forceOverLifetime;
			forceOverLifetime.xMultiplier = data.m_forceOverLifetimeX;
			forceOverLifetime.yMultiplier = data.m_forceOverLifetimeY;
			forceOverLifetime.zMultiplier = data.m_forceOverLifetimeZ;


		}
	}

	void OnEnable()
	{
		if ( m_whenScale == WhenScale.ENABLE )
			DoScale();
		else if (m_whenScale == WhenScale.AFTER_ENABLE) 
		{
			StartCoroutine( AfterEnable() );
		}
	}

	IEnumerator AfterEnable()
	{
		yield return null;
		DoScale();
	}

	public void DoScale()
	{
		if ( m_resetFirst )
		{
			ResetOriginalData();
		}

		switch( m_scaleOrigin )
		{
			case ScaleOrigin.DRAGON_SIZE:
			{
				Scale( InstanceManager.player.data.scale );	
			}break;
			case ScaleOrigin.TRANSFORM_SCALE:
			{
				Scale( m_transform.localScale.x );
			}break;
			case ScaleOrigin.ATTRIBUTE_SCALE:
			{
				Scale( m_scale );
			}break;
		}
	}



	void Scale( float scale )
	{	
		m_scale = scale;
		// transform.localScale *= scale;
		if ( m_scaleAllChildren )
		{
			ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
			foreach( ParticleSystem p in childs )
				ScaleParticle( p, scale );
		}
		else
		{
			ParticleSystem particle =  GetComponent<ParticleSystem>();
			if (particle)
			{
				ScaleParticle( particle, scale );
			}
		}

	}
	
	void ScaleParticle( ParticleSystem ps, float scale)
	{
		ParticleSystem.MainModule mainModule = ps.main;
		if ( mainModule.startSize3D )
		{
			mainModule.startSizeXMultiplier *= scale;
			mainModule.startSizeYMultiplier *= scale;
			mainModule.startSizeZMultiplier *= scale;
		}
		else
		{
			mainModule.startSizeMultiplier *= scale;
		}

		mainModule.gravityModifierMultiplier *= scale;
		mainModule.startSpeedMultiplier *= scale;
		if (m_scaleLifetime)
			mainModule.startLifetimeMultiplier *= scale;

		ParticleSystem.ShapeModule shape = ps.shape;
		switch( shape.shapeType )
		{
			case ParticleSystemShapeType.Sphere:
			case ParticleSystemShapeType.SphereShell:
			{
				shape.radius *= scale;
			}break;
			case ParticleSystemShapeType.Hemisphere:
			case ParticleSystemShapeType.HemisphereShell:
			{
				shape.radius *= scale;
			}break;
			case ParticleSystemShapeType.Cone:
			case ParticleSystemShapeType.ConeShell:
			case ParticleSystemShapeType.ConeVolume:
			case ParticleSystemShapeType.ConeVolumeShell:
			{	
				shape.radius *= scale;
				shape.length *= scale;
			}break;
			case ParticleSystemShapeType.Box:
			case ParticleSystemShapeType.BoxShell:
			case ParticleSystemShapeType.BoxEdge:
			{
				shape.box *= scale;
			}break;
			case ParticleSystemShapeType.Mesh:
			{
				shape.meshScale *= scale;
			}break;
			case ParticleSystemShapeType.MeshRenderer:
			{
				shape.meshScale *= scale;
			}break;
			case ParticleSystemShapeType.SkinnedMeshRenderer:
			{
				shape.meshScale *= scale;
			}break;
			case ParticleSystemShapeType.CircleEdge:
			case ParticleSystemShapeType.Circle:
			{
				shape.radius *= scale;
			}break;
			case ParticleSystemShapeType.SingleSidedEdge:
			{
				shape.radius *= scale;
			}break;
		}

		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
		velocityOverLifetime.xMultiplier *= scale;
		velocityOverLifetime.yMultiplier *= scale;
		velocityOverLifetime.zMultiplier *= scale;

		ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
		if (limitVelocityOverLifetime.separateAxes)
		{
			limitVelocityOverLifetime.limitXMultiplier *= scale;
			limitVelocityOverLifetime.limitYMultiplier *= scale;
			limitVelocityOverLifetime.limitZMultiplier *= scale;	
		}
		else
		{
			limitVelocityOverLifetime.limitMultiplier *= scale;
		}


		ParticleSystem.ForceOverLifetimeModule forceOverLifetime = ps.forceOverLifetime;
		forceOverLifetime.xMultiplier *= scale;
		forceOverLifetime.yMultiplier *= scale;
		forceOverLifetime.zMultiplier *= scale;

	}
	
}
