using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Image))]
public class TintByTierColor : MonoBehaviour
{

    [SerializeField]
    private DragonTier m_tier;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Image>().color = UIConstants.GetDragonTierColor(m_tier);
    }


}
