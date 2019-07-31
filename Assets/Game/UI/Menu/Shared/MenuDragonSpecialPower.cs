using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MenuDragonPreview))]
public class MenuDragonSpecialPower : MonoBehaviour {
    private enum EPowerElement {
        ExtraObject = 0,
        Pet,
        AnimParam
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

    // internal
    private MenuDragonPreview m_dragonPreview;
    private DragonDataSpecial m_data;

    // Use this for initialization
    void Start() {
        m_dragonPreview = GetComponent<MenuDragonPreview>();

        DragonDataSpecial dataSpecial = null;
        if(SceneController.mode == SceneController.Mode.TOURNAMENT) {
			dataSpecial = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData as DragonDataSpecial;
        } else {
            dataSpecial = (DragonDataSpecial)DragonManager.GetDragonData(m_dragonPreview.sku);
        }

        for (int i = 0; i < m_elementsPerPowerLevel.Count; ++i) {
            EnablePowerLevel(i, i <= dataSpecial.powerLevel);
        }

        UpdateTierSize(dataSpecial);
    }

    private void OnEnable() {
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnLevelUpgraded);
	}

    private void OnDisable() {
        Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnLevelUpgraded);
    }
    
    private void OnLevelUpgraded(DragonDataSpecial _data) {

        // Only if this is the upgraded dragon
        if (_data.sku == m_dragonPreview.sku)
        {
            // Refresh disguise
            if (m_dragonPreview.equip.dragonDisguiseSku != _data.disguise)
            {
                // Store the data for internal use
                m_data = _data;

                // Get all the dependencies needed for the current skin
                AddressablesBatchHandle handle = HDAddressablesManager.Instance.GetHandleForDragonDisguise(_data);
                List<string> dependencyIds = handle.DependencyIds;

                // Load de the dependencies asynchronously
                AddressablesOp op = HDAddressablesManager.Instance.LoadDependencyIdsListAsync(dependencyIds);
                op.OnDone = OnDisguiseLoaded;

            }

            // Update powers if needed
            EnablePowerLevel(_data.powerLevel, true);

            // Update the size if needed
            UpdateTierSize(_data);
        }
    }

    /// <summary>
    /// Callback function called after the the disguise assest are loaded
    /// </summary>
    private void OnDisguiseLoaded(AddressablesOp op)
    {

        if (op.Error != null)
        {
            Debug.LogError("Error loading the disguise " + m_data.disguise);
            return;
        }

        // Load successful. Equip the disguise.
        m_dragonPreview.equip.EquipDisguise(m_data.disguise);
    
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
                    case EPowerElement.AnimParam:
                        {
                            if (_enable)
                            {
                                string[] _params = m_elementsPerPowerLevel[_level].element[e].name.Split(':');
                                m_dragonPreview.animator.SetInteger(_params[0], int.Parse(_params[1]));
                            }
                        }break;
                }
            }
        }
    }

    private void UpdateTierSize(DragonDataSpecial _data) {
        if (enabled) {
            if (_data.sku == m_dragonPreview.sku && InstanceManager.menuSceneController != null) {  // Only on the menu
                transform.localScale = GameConstants.Vector3.one * _data.scaleMenu;
            }
        }
    }
}
