using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MenuDragonPreview))]
public class MenuDragonSpecialPower : MonoBehaviour {
    private enum EPowerElement {
        ExtraObject = 0,
        Pet
    }

    [System.Serializable]
    private class PowerElementList {
        public List<PowerElement> element;
    }

    [System.Serializable]
    private class PowerElement {
        public EPowerElement type;
        public string name;
    } 

    [SerializeField] private List<PowerElementList> m_elementsPerPowerLevel = null;

    //
    private MenuDragonPreview m_dragonPreview;


    // Use this for initialization
    void Start() {
        m_dragonPreview = GetComponent<MenuDragonPreview>();

        DragonDataSpecial dataSpecial = null;
        if (SceneController.mode == SceneController.Mode.TOURNAMENT) {
            // Use tmp data
            HDTournamentData tournamentData = HDLiveDataManager.tournament.data as HDTournamentData;
            HDTournamentDefinition def = tournamentData.definition as HDTournamentDefinition;

            dataSpecial = IDragonData.CreateFromBuild(def.m_build) as DragonDataSpecial;
        } else {
            dataSpecial = (DragonDataSpecial)DragonManager.GetDragonData(m_dragonPreview.sku);
        }

        for (int i = 0; i < m_elementsPerPowerLevel.Count; ++i) {
            EnablePowerLevel(i, i <= dataSpecial.powerLevel);
        }

        OnTierUpgrade(dataSpecial);
    }

    private void OnEnable() {
        Messenger.AddListener<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, OnStatUpgraded);
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, OnPowerUpgrade);
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_TIER_UPGRADED, OnTierUpgrade);
	}

    private void OnDisable() {
        Messenger.RemoveListener<DragonDataSpecial, DragonDataSpecial.Stat>(MessengerEvents.SPECIAL_DRAGON_STAT_UPGRADED, OnStatUpgraded);
        Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_POWER_UPGRADED, OnPowerUpgrade);
        Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_TIER_UPGRADED, OnTierUpgrade);
    }
    
    private void OnStatUpgraded(DragonDataSpecial _data, DragonDataSpecial.Stat _stat) {
        // Refresh disguise
        if (m_dragonPreview.equip.dragonDisguiseSku != _data.diguise)
        {
            m_dragonPreview.equip.EquipDisguise( _data.diguise );
        }
    }

    private void OnPowerUpgrade(DragonDataSpecial _data) {
        if (enabled) {
            if (_data.sku == m_dragonPreview.sku)
            {
                EnablePowerLevel(_data.powerLevel, true);
            }
        }
    }

    private void EnablePowerLevel(int _level, bool _enable) {
        if ( _level < m_elementsPerPowerLevel.Count )
        {
            for (int e = 0;  e < m_elementsPerPowerLevel[_level].element.Count; ++e) {
                switch (m_elementsPerPowerLevel[_level].element[e].type) {
                    case EPowerElement.ExtraObject:
                    this.transform.parent.FindTransformRecursive(m_elementsPerPowerLevel[_level].element[e].name).gameObject.SetActive(_enable);
                    break;
    
                    case EPowerElement.Pet:
                    if (_enable) {
                        m_dragonPreview.equip.EquipPet(m_elementsPerPowerLevel[_level].element[e].name, 4);
                    } else {
                        m_dragonPreview.equip.EquipPet("", 4);
                    }
                    break;
                }
            }
        }
    }

    private void OnTierUpgrade(DragonDataSpecial _data) {
        if (enabled) {
            if (_data.sku == m_dragonPreview.sku && InstanceManager.menuSceneController != null) {  // Only on the menu
                transform.localScale = GameConstants.Vector3.one * _data.scaleMenu;
            }
        }
    }
}
