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
        COUNT
    };

    public enum FireColorVariants
    {
        DEFAULT,
        TOON,
        EXPLOSION,
        UNDERWATER,
    };
    
    Dictionary<FireColorType, Dictionary<FireColorVariants, FireColorConfig>> m_loadedColors = new Dictionary<FireColorType, Dictionary<FireColorVariants, FireColorConfig>>();
    Dictionary<FireColorType, Material> m_originalBurnMaterial = new Dictionary<FireColorType, Material>();
    Dictionary<FireColorType, List<Material>> m_freeDecorationBurnMaterial = new Dictionary<FireColorType, List<Material>>();
    
    private void Awake()
    {
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
                    Material ashMaterial = new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material);
                    m_originalBurnMaterial.Add(fireColorType, ashMaterial);
                }break;
                case FireColorType.RED:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRed"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedUnderwater"));
                    Material ashMaterial = new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material);
                    m_originalBurnMaterial.Add(fireColorType, ashMaterial);
                }break;
                case FireColorType.ICE:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIce"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceUnderwater"));
                    Material ashMaterial = new Material(Resources.Load("Game/Materials/IceBurnToAshes") as Material);
                    m_originalBurnMaterial.Add(fireColorType, ashMaterial);
                }break;
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

}
