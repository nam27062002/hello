using UnityEngine;
using UnityEngine.UI;

public class LabDragonBarTierElement : LabDragonBarLockedElement {
    [Separator("Tier icons")]
    [SerializeField] private Sprite[] m_tierIconSprites;

    [SerializeField] private Image m_icon;


    public void SetTier(int _index) {
        m_icon.sprite = m_tierIconSprites[_index];
    }
}
