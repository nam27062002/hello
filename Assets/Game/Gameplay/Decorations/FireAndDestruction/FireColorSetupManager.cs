using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireColorSetupManager : MonoBehaviour {

    protected static FireColorSetupManager m_instance;
    public static FireColorSetupManager instance {
        get {
            return m_instance; 
        }
    }

    public enum FireColorType
    {
        RED,
        BLUE,
        ICE,
        LAVA,
        COUNT
    };

    // COMPARER. Use this on all your Dictionaries
    public struct FireColorTypeComparer : IEqualityComparer<FireColorType>
    {
        public bool Equals(FireColorType b1, FireColorType b2)
        {
            return b1 == b2;
        }
        public int GetHashCode(FireColorType bx)
        {
            return (int)bx;
        }
    }


    public enum FireColorVariants
    {
        DEFAULT,
        TOON,
        EXPLOSION,
        UNDERWATER,
    };
    
    FireColorTypeComparer m_fireColorTypeComparer;
    Dictionary<FireColorType, Dictionary<FireColorVariants, FireColorConfig>> m_loadedColors;
        // Materials used when a decoration is burning
    Dictionary<FireColorType, Material> m_originalBurnMaterial;
    Dictionary<FireColorType, List<Material>> m_freeDecorationBurnMaterial;
    
        // Materials used when a decoration is burned. This is the destroyed mesh under the normal mesh
    Dictionary<FireColorType, Material> m_burnedViewMaterial;
    
    Dictionary<FireColorConfig, List<Material>> m_freeConfigMaterials = new Dictionary<FireColorConfig, List<Material>>();
    
    private void Awake()
    {
        m_fireColorTypeComparer = new FireColorTypeComparer();
        m_loadedColors = new Dictionary<FireColorType, Dictionary<FireColorVariants, FireColorConfig>>( m_fireColorTypeComparer );
        m_originalBurnMaterial = new Dictionary<FireColorType, Material>(m_fireColorTypeComparer);
        m_freeDecorationBurnMaterial = new Dictionary<FireColorType, List<Material>>(m_fireColorTypeComparer);

        m_burnedViewMaterial = new Dictionary<FireColorType, Material>(m_fireColorTypeComparer);

        m_instance = this;
        int max = (int)FireColorType.COUNT;
        for (int i = 0; i < max; i++)
        {
            m_freeDecorationBurnMaterial.Add((FireColorType)i, new List<Material>());
        }
        LoadColor(FireColorType.RED);   // Red is default
    }

    private void OnDestroy()
    {
        if (m_instance == this)
        {
            m_instance = null;
        }
    }

    public void LoadColor(FireColorType fireColorType)
    {
        if (!m_loadedColors.ContainsKey( fireColorType ))
        {
            MachineInflammableManager.instance.RegisterColor(fireColorType);
            Dictionary<FireColorVariants, FireColorConfig> loadColors = new Dictionary<FireColorVariants, FireColorConfig>();
            switch( fireColorType )
            {
                case FireColorType.BLUE:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlue"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlueExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlueToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlueUnderwater"));
                    m_originalBurnMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material));
                    m_burnedViewMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/InflammableDestructible/burnt_texture") as Material));
                }break;
                case FireColorType.RED:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRed"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedUnderwater"));
                    m_originalBurnMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material));
                    m_burnedViewMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/InflammableDestructible/burnt_texture") as Material));
                }break;
                case FireColorType.ICE:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIce"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceUnderwater"));
                    m_originalBurnMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/IceBurnToAshes") as Material));
                    m_burnedViewMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/InflammableDestructible/Ice_burnt_texture") as Material));
                }break;
                case FireColorType.LAVA:
                    {
                        loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireLava"));
                        loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireLavaExplosion"));
                        loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireLavaToon"));
                        loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireLavaUnderwater"));
                        m_originalBurnMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material));
                        m_burnedViewMaterial.Add(fireColorType, new Material(Resources.Load("Game/Materials/InflammableDestructible/burnt_texture") as Material));
                    }
                    break;
            }
            m_loadedColors.Add( fireColorType, loadColors );
        }
    }
    
    public FireColorConfig GetColorConfig( FireColorType fireColorType, FireColorVariants fireColorVariants)
    {
        FireColorConfig ret = null;
        if (m_loadedColors.ContainsKey(fireColorType))
            ret = m_loadedColors[fireColorType][fireColorVariants];
        return ret;
    }
    
    
    // Burning Decocations materials
    public Material GetDecorationBurnMaterial( FireColorType colorType )
    {
        if ( m_freeDecorationBurnMaterial[colorType].Count <= 0 )
        {
            Material newMat = new Material(m_originalBurnMaterial[colorType]);
            m_freeDecorationBurnMaterial[colorType].Add( newMat );
        }
        Material ret = m_freeDecorationBurnMaterial[colorType][0];
        m_freeDecorationBurnMaterial[colorType].RemoveAt(0);
        return ret;
    }
    
    public void ReturnDecorationBurnMaterial( FireColorType colorType, Material mat )
    {
        m_freeDecorationBurnMaterial[colorType].Add( mat );
    }

    // Burned Decoration Materials
    public Material GetDecorationBurnedMaterial(FireColorType colorType)
    {
        return m_burnedViewMaterial[colorType];
    }
    
    // Material for FyreTypeAutoselector 
    public Material GetConfigMaterial( FireColorConfig fireColorConfig)
    {
        Material ret = null;
        if (!m_freeConfigMaterials.ContainsKey( fireColorConfig ))
        {
            m_freeConfigMaterials.Add(fireColorConfig, new List<Material>());
        }
        if ( m_freeConfigMaterials[fireColorConfig].Count > 0 )
        {
            ret = m_freeConfigMaterials[fireColorConfig][0];
            m_freeConfigMaterials[fireColorConfig].RemoveAt(0);
        }
        else
        {
            ret = new Material(fireColorConfig.m_fireMaterial);
            ret.SetFloat("_Seed", Random.value);
        }
        return ret;
    }
    
    public void ReturnConfigMaterial(FireColorConfig _fireColorConfig, Material _mat)
    {
        m_freeConfigMaterials[_fireColorConfig].Add( _mat );
    }

}
