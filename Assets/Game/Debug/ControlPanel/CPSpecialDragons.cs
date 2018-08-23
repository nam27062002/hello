using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CPSpecialDragons : MonoBehaviour {

    public TMPro.TMP_Dropdown m_specialDragonsDropDown; 
    
    public void SetSpecialDragon( int _item  )
    {
        PlayerPrefs.SetString(DebugSettings.SPECIAL_DRAGON_SKU, m_specialDragonsDropDown.options[_item].text );
    }
    
    public void SetSpecialDragonPowerLevel( int _level )
    {
        PlayerPrefs.SetInt(DebugSettings.SPECIAL_DRAGON_POWER_LEVEL, _level );
    }
}
