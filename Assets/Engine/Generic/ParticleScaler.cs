using UnityEngine;
using System.Collections;


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

	// Use this for initialization
	void Start () 
	{
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
		ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
		foreach( ParticleSystem p in childs )
			ScaleParticle( p, scale );
	}
	
	void ScaleParticle( ParticleSystem ps, float scale)
	{
		ParticleSystem.MainModule mainModule = ps.main;
		mainModule.startSizeMultiplier *= scale;
		mainModule.gravityModifierMultiplier *= scale;
		mainModule.startSpeedMultiplier *= scale;
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
	}
	
}
