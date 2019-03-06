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
        ICE
    };

    public enum FireColorVariants
    {
        DEFAULT,
        TOON,
        EXPLOSION,
        UNDERWATER,
    };
    
    Dictionary<FireColorType, Dictionary<FireColorVariants, FireColorConfig>> m_loadedColors = new Dictionary<FireColorType, Dictionary<FireColorVariants, FireColorConfig>>();
    
    
    private void Awake()
    {
        m_instance = this;
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
            Dictionary<FireColorVariants, FireColorConfig> loadColors = new Dictionary<FireColorVariants, FireColorConfig>();
            switch( fireColorType )
            {
                case FireColorType.BLUE:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlue"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlueExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlueToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireBlueUnderwater"));
                }break;
                case FireColorType.RED:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRed"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireRedUnderwater"));
                }break;
                case FireColorType.ICE:
                {
                    loadColors.Add(FireColorVariants.DEFAULT, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIce"));
                    loadColors.Add(FireColorVariants.EXPLOSION, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceExplosion"));
                    loadColors.Add(FireColorVariants.TOON, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceToon"));
                    loadColors.Add(FireColorVariants.UNDERWATER, Resources.Load<FireColorConfig>("Game/Fire/ColorConfigs/FireIceUnderwater"));
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
    
}
