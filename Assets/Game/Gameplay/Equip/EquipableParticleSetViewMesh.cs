using UnityEngine;

public class EquipableParticleSetViewMesh : MonoBehaviour {

	public void Start()
	{
		DragonEquip dragonEquip = GetComponentInParent<DragonEquip>();
        if ( dragonEquip != null )
        {
            Setup( dragonEquip ); 
        }
        
	}

	public void Setup( DragonEquip dragon )
	{
		SkinnedMeshRenderer skinned = null;
		ParticleSystem[] childs = gameObject.GetComponentsInChildren<ParticleSystem>(true);
		if ( childs != null && childs.Length > 0 )
		{
			Transform view = dragon.transform.Find("view");
			if ( view != null )
			{
				SkinnedMeshRenderer[] skinneds = view.GetComponentsInChildren<SkinnedMeshRenderer>();
				if ( skinneds != null && skinneds.Length > 0 ) 
				{
					skinned = skinneds[0];	// Maybe check name to make sure it is the body of the dragon??
				}
			}
		}


		foreach( ParticleSystem p in childs )
		{
			if ( p.shape.shapeType == ParticleSystemShapeType.SkinnedMeshRenderer )
			{
				// Use dragon view to set mesh
				ParticleSystem.ShapeModule shape = p.shape;
				shape.skinnedMeshRenderer = skinned;
			}
		}
	}
}
