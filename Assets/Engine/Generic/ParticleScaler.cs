using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	protected class PSDataRegistry
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

        //Custom particle data
        //Emmision radius
        public float m_radius;

        //Scale
        public Range m_scaleRange;

        //Velocity
        public Range m_VelX;
        public Range m_VelY;
        public Range m_VelZ;

        public ParticleSystem m_psystem;
        public CustomParticleSystem m_cpsystem;
	}
	protected List<PSDataRegistry> m_originalData = new List<PSDataRegistry>();


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
			switch( mainModule.startSize.mode )
        	{
				case ParticleSystemCurveMode.Constant:
        		{
					data.m_startSizeXMultiplier = mainModule.startSizeMultiplier;	
        		}break;
				case ParticleSystemCurveMode.TwoConstants:
				{
					data.m_startSizeXMultiplier = mainModule.startSize.constantMin;
					data.m_startSizeYMultiplier = mainModule.startSize.constantMax;
				}break;
				default:
        		{
					data.m_startSizeXMultiplier = mainModule.startSize.curveMultiplier;	
        		}break;
        	}
			
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

        data.m_psystem = ps;
        data.m_cpsystem = null;

        m_originalData.Add(data);
	}

    void SaveParticleData(CustomParticleSystem ps)
    {
        PSDataRegistry data = new PSDataRegistry();

        data.m_radius = ps.m_radius;
        data.m_scaleRange = ps.m_scaleRange;
        data.m_VelX = ps.m_VelX;
        data.m_VelY = ps.m_VelY;
        data.m_VelZ = ps.m_VelZ;

        data.m_cpsystem = ps;
        data.m_psystem = null;

        m_originalData.Add(data);

    }



    void ResetOriginalData()
	{
		if ( m_scaleAllChildren )
		{
			foreach(PSDataRegistry p in m_originalData)
				ResetParticleData( p );
		}
		else
		{
			ResetParticleData(m_originalData[0]);
		}
	}


	void ResetParticleData(PSDataRegistry data)
	{
        if (data.m_psystem != null)
        {
            ParticleSystem ps = data.m_psystem;
            ParticleSystem.MainModule mainModule = ps.main;
            if (mainModule.startSize3D)
            {
                mainModule.startSizeXMultiplier = data.m_startSizeXMultiplier;
                mainModule.startSizeYMultiplier = data.m_startSizeYMultiplier;
                mainModule.startSizeZMultiplier = data.m_startSizeZMultiplier;
            }
            else
            {
            	switch( mainModule.startSize.mode )
            	{
					case ParticleSystemCurveMode.Constant:
            		{
						mainModule.startSizeMultiplier = data.m_startSizeXMultiplier;
            		}break;
            		case ParticleSystemCurveMode.TwoConstants:
            		{
						ParticleSystem.MinMaxCurve curve = mainModule.startSize;
						curve.constantMin = data.m_startSizeXMultiplier;
						curve.constantMax = data.m_startSizeYMultiplier;
						mainModule.startSize = curve;
            		}break;
					default:
            		{
						ParticleSystem.MinMaxCurve curve = mainModule.startSize;
						curve.curveMultiplier = data.m_startSizeXMultiplier;
						mainModule.startSize = curve;
            		}break;
            	}
            }
            mainModule.gravityModifierMultiplier = data.m_gravityModifierMultiplier;
            mainModule.startSpeedMultiplier = data.m_startSpeedMultiplier;
            mainModule.startLifetimeMultiplier = data.m_startLifetimeMultiplier;

            ParticleSystem.ShapeModule shape = ps.shape;
            switch (shape.shapeType)
            {
                case ParticleSystemShapeType.Sphere:
                case ParticleSystemShapeType.SphereShell:
                    {
                        shape.radius = data.m_shapeSize;
                    }
                    break;
                case ParticleSystemShapeType.Hemisphere:
                case ParticleSystemShapeType.HemisphereShell:
                    {
                        shape.radius = data.m_shapeSize;
                    }
                    break;
                case ParticleSystemShapeType.Cone:
                case ParticleSystemShapeType.ConeShell:
                case ParticleSystemShapeType.ConeVolume:
                case ParticleSystemShapeType.ConeVolumeShell:
                    {
                        shape.radius = data.m_shapeSize;
                        shape.length = data.m_shapeLengthSize;
                    }
                    break;
                case ParticleSystemShapeType.Box:
                case ParticleSystemShapeType.BoxShell:
                case ParticleSystemShapeType.BoxEdge:
                    {
                        shape.box = data.m_boxShapeSize;
                    }
                    break;
                case ParticleSystemShapeType.Mesh:
                    {
                        shape.meshScale = data.m_shapeSize;
                    }
                    break;
                case ParticleSystemShapeType.MeshRenderer:
                    {
                        shape.meshScale = data.m_shapeSize;
                    }
                    break;
                case ParticleSystemShapeType.SkinnedMeshRenderer:
                    {
                        shape.meshScale = data.m_shapeSize;
                    }
                    break;
                case ParticleSystemShapeType.CircleEdge:
                case ParticleSystemShapeType.Circle:
                    {
                        shape.radius = data.m_shapeSize;
                    }
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    {
                        shape.radius = data.m_shapeSize;
                    }
                    break;
            }

            ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = ps.velocityOverLifetime;
            velocityOverLifetime.xMultiplier = data.m_velocityOverLifetimeX;
            velocityOverLifetime.yMultiplier = data.m_velocityOverLifetimeY;
            velocityOverLifetime.zMultiplier = data.m_velocityOverLifetimeZ;

            ParticleSystem.LimitVelocityOverLifetimeModule limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
            if (limitVelocityOverLifetime.separateAxes)
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
        else if (data.m_cpsystem != null)
        {
            CustomParticleSystem cps = data.m_cpsystem;

            cps.m_radius = data.m_radius;
            cps.m_scaleRange = data.m_scaleRange;
            cps.m_VelX = data.m_VelX;
            cps.m_VelY = data.m_VelY;
            cps.m_VelZ = data.m_VelZ;
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
				if (InstanceManager.player != null)
					Scale(InstanceManager.player.transform.lossyScale.x);
			}break;
			case ScaleOrigin.TRANSFORM_SCALE:
			{
				Scale( m_transform.lossyScale.x );
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
			foreach(PSDataRegistry pdata in m_originalData )
				ScaleParticle( pdata, scale );
		}
		else
		{
			ScaleParticle( m_originalData[0], scale );
		}

	}
	
	void ScaleParticle(PSDataRegistry data, float scale)
	{
        if (data.m_psystem != null)
        {
            ParticleSystem ps = data.m_psystem;

            ParticleSystem.MainModule mainModule = ps.main;
            if (mainModule.startSize3D)
            {
                mainModule.startSizeXMultiplier *= scale;
                mainModule.startSizeYMultiplier *= scale;
                mainModule.startSizeZMultiplier *= scale;
            }
            else
            {
            	switch( mainModule.startSize.mode )
            	{
            		case ParticleSystemCurveMode.Constant:
            		{
						mainModule.startSizeMultiplier *= scale;
            		}break;
					case ParticleSystemCurveMode.TwoConstants:
					{
						ParticleSystem.MinMaxCurve curve = mainModule.startSize;
						curve.constantMin *= scale;
						curve.constantMax *= scale;
						mainModule.startSize = curve;
					}break;
					default:
            		{
						ParticleSystem.MinMaxCurve curve = mainModule.startSize;
						curve.curveMultiplier *= scale;
						mainModule.startSize = curve;
            		}break;
            	}
                
            }

            mainModule.gravityModifierMultiplier *= scale;
            mainModule.startSpeedMultiplier *= scale;
            if (m_scaleLifetime)
                mainModule.startLifetimeMultiplier *= scale;

            ParticleSystem.ShapeModule shape = ps.shape;
            switch (shape.shapeType)
            {
                case ParticleSystemShapeType.Sphere:
                case ParticleSystemShapeType.SphereShell:
                    {
                        shape.radius *= scale;
                    }
                    break;
                case ParticleSystemShapeType.Hemisphere:
                case ParticleSystemShapeType.HemisphereShell:
                    {
                        shape.radius *= scale;
                    }
                    break;
                case ParticleSystemShapeType.Cone:
                case ParticleSystemShapeType.ConeShell:
                case ParticleSystemShapeType.ConeVolume:
                case ParticleSystemShapeType.ConeVolumeShell:
                    {
                        shape.radius *= scale;
                        shape.length *= scale;
                    }
                    break;
                case ParticleSystemShapeType.Box:
                case ParticleSystemShapeType.BoxShell:
                case ParticleSystemShapeType.BoxEdge:
                    {
                        shape.box *= scale;
                    }
                    break;
                case ParticleSystemShapeType.Mesh:
                    {
                        shape.meshScale *= scale;
                    }
                    break;
                case ParticleSystemShapeType.MeshRenderer:
                    {
                        shape.meshScale *= scale;
                    }
                    break;
                case ParticleSystemShapeType.SkinnedMeshRenderer:
                    {
                        shape.meshScale *= scale;
                    }
                    break;
                case ParticleSystemShapeType.CircleEdge:
                case ParticleSystemShapeType.Circle:
                    {
                        shape.radius *= scale;
                    }
                    break;
                case ParticleSystemShapeType.SingleSidedEdge:
                    {
                        shape.radius *= scale;
                    }
                    break;
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
        else if (data.m_cpsystem != null)
        {
            CustomParticleSystem cps = data.m_cpsystem;

            cps.m_radius = data.m_radius * scale;
            cps.m_scaleRange = data.m_scaleRange * scale;
            cps.m_VelX = data.m_VelX * scale;
            cps.m_VelY = data.m_VelY * scale;
            cps.m_VelZ = data.m_VelZ * scale;
        }
    }	
}
