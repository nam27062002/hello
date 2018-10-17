using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarSkillElement : LabDragonBarLockedElement {
    [Separator("Skill")]
    [SerializeField] private Image m_icon = null;


    public void SetIcon(string _name) {
        //m_icon.sprite = Resources.Load<Sprite>(UIConstants.POWER_ICONS_PATH + _name);
    }
}
