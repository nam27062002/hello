using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class TintByTierColor : MonoBehaviour
{
    public enum ColorSet{
        DRAGON_TIER_COLOR
    }

    [SerializeField]
    private DragonTier m_tier;

    [SerializeField]
    private ColorSet m_colorSet = ColorSet.DRAGON_TIER_COLOR;


    // Start is called before the first frame update
    void Start()
    {

        Apply(m_tier, m_colorSet);
    }

    public void Apply(DragonTier _tier, ColorSet _colorSet)
    {
        m_tier = _tier;
        m_colorSet = _colorSet;

        switch (m_colorSet)
        {
            case ColorSet.DRAGON_TIER_COLOR:
                GetComponent<Image>().color = UIConstants.GetDragonTierColor(m_tier);
                break;

        }
    }


}
