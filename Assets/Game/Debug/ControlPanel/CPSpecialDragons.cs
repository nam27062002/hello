using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CPSpecialDragons : MonoBehaviour {

    public TMPro.TMP_Dropdown m_specialDragonsDropDown;
    
    public TMPro.TextMeshProUGUI m_specialPowerLevelLabel;
    public TMPro.TextMeshProUGUI m_specialHPBoostLevelLabel;
    public TMPro.TextMeshProUGUI m_specialSpeedBoostLevelLabel;
    public TMPro.TextMeshProUGUI m_specialEnergyBoostLevelLabel;

	[Space]
	public CPBoolPref m_useSpecialToggle = null;
	public CanvasGroup m_interactableGroup = null;

    public void Awake()
    {
        m_specialPowerLevelLabel.text = DebugSettings.specialDragonPowerLevel.ToString(); 
        m_specialHPBoostLevelLabel.text = DebugSettings.specialDragonHpBoostLevel.ToString(); 
        m_specialSpeedBoostLevelLabel.text = DebugSettings.specialDragonSpeedBoostLevel.ToString(); 
        m_specialEnergyBoostLevelLabel.text = DebugSettings.specialDragonEnergyBoostLevel.ToString();

        int max = m_specialDragonsDropDown.options.Count;
        string dragonSku = DebugSettings.Prefs_GetStringPlayer(DebugSettings.SPECIAL_DRAGON_SKU, "dragon_helicopter");
        for (int i = 0; i < max; i++)
        {
            if ( m_specialDragonsDropDown.options[i].text.Equals(dragonSku) )
            {
                m_specialDragonsDropDown.value = i;
            }
        }
    }

	public void OnEnable() {
		m_interactableGroup.interactable = Prefs.GetBoolPlayer(m_useSpecialToggle.id);
	}

	public void SetSpecialDragon( int _item  )
    {
        DebugSettings.Prefs_SetStringPlayer(DebugSettings.SPECIAL_DRAGON_SKU, m_specialDragonsDropDown.options[_item].text );
    }
    
    // Power level
    public void IncreaseSpecialDragonPower()
    {
        IncreaseDebugSettings(DebugSettings.SPECIAL_DRAGON_POWER_LEVEL , m_specialPowerLevelLabel);
    }
    
    public void DecreaseSpecialDragonPower()
    {
        DecraseDebugSettings(DebugSettings.SPECIAL_DRAGON_POWER_LEVEL , m_specialPowerLevelLabel);
    }
    
    // HP Boost
    public void IncreaseSpecialDragonHPBoost()
    {
        IncreaseDebugSettings(DebugSettings.SPECIAL_DRAGON_HP_BOOST_LEVEL , m_specialHPBoostLevelLabel);
    }
    
    public void DecreaseSpecialDragonHPBoost()
    {
        DecraseDebugSettings(DebugSettings.SPECIAL_DRAGON_HP_BOOST_LEVEL , m_specialHPBoostLevelLabel);
    }
    
    // Speed Boost
    public void IncreaseSpecialDragonSpeedBoost()
    {
        IncreaseDebugSettings(DebugSettings.SPECIAL_DRAGON_SPEED_BOOST_LEVEL, m_specialSpeedBoostLevelLabel);
    }
    
    public void DecreaseSpecialDragonSpeedBoost()
    {
        DecraseDebugSettings(DebugSettings.SPECIAL_DRAGON_SPEED_BOOST_LEVEL , m_specialSpeedBoostLevelLabel);
    }
    
    // Energy Boost
    public void IncreaseSpecialDragonEnergyBoost()
    {
        IncreaseDebugSettings(DebugSettings.SPECIAL_DRAGON_ENERGY_BOOST_LEVEL, m_specialEnergyBoostLevelLabel);
    }
    
    public void DecreaseSpecialDragonEnergyBoost()
    {
        DecraseDebugSettings(DebugSettings.SPECIAL_DRAGON_ENERGY_BOOST_LEVEL , m_specialEnergyBoostLevelLabel);
    }
    
    
    private void IncreaseDebugSettings( string param , TextMeshProUGUI text)
    {
        int level = DebugSettings.Prefs_GetIntPlayer(param, 0);
        level++;
        // Update Text
        text.text = level.ToString();
        DebugSettings.Prefs_SetIntPlayer(param, level );
    }
    
    private void DecraseDebugSettings( string param , TextMeshProUGUI text)
    {
        int level = DebugSettings.Prefs_GetIntPlayer(param, 0);
        level--;
        if (level < 0)
            level = 0;
        // Update Text
        text.text = level.ToString();
        DebugSettings.Prefs_SetIntPlayer(param, level );
    }
    
	public void OnResetSpecialDragons() {
		List<IDragonData> dragons = DragonManager.GetDragonsByOrder(IDragonData.Type.SPECIAL);
		if(dragons != null) {
			int i;
			int count = dragons.Count;
			for(i = 0; i < count; i++) {
				// Reset dragon data
				dragons[i].ResetLoadedData();
			}

			// Save persistence
			PersistenceFacade.instance.Save_Request(false);
		}
	}

    public void OnResetDragonLevel()
    {
        IDragonData current = DragonManager.CurrentDragon;
        
        if (current != null && current.type == IDragonData.Type.SPECIAL)
        {
            DragonDataSpecial special = current as DragonDataSpecial;

            // Reset level progression
            special.ResetProgression();

            // Save persistence
            PersistenceFacade.instance.Save_Request(false);

            // Simulate a dragon selected event so everything is refreshed
            Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_SELECTED, current.sku);
        }
    }

}
