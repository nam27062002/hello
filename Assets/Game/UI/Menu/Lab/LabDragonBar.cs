﻿using System.Collections.Generic;
using UnityEngine;

public class LabDragonBar : MonoBehaviour {

    [SerializeField] private GameObject m_elementLevelPrefab = null;
    [SerializeField] private GameObject m_elementSkillPrefab = null;
    [SerializeField] private GameObject m_elementTierPrefab = null;

    [SerializeField] private RectTransform m_content = null;
    [SerializeField] private UITooltip m_tooltip = null;

    [SerializeField] private float m_blankSpace = 5f;
    [SerializeField] private AnimationCurve m_scaleTiersCurve = null;
    [SerializeField] private AnimationCurve m_scaleLevelsCurve = null;
    [SerializeField] private AnimationCurve m_positionLevelsCurve = null;
    [SerializeField] private float m_positionCurveScale = 10f;


    [Separator("debug")]
    [SerializeField] private int m_debugMaxLevel = 30;
    [SerializeField] private int[] m_debugLevelTier = { 0, 10, 20, 30 };
    [SerializeField] private int[] m_debugLevelSkill = { 5, 15, 25 };
    [Space]
    [SerializeField] private int m_debugCurrentLevel = 1;
    [SerializeField] private int m_debugMaxTierUnlocked = 3;


    private List<LabDragonBarElement> m_levelElements = new List<LabDragonBarElement>();
    private List<LabDragonBarTierElement> m_tierElements = new List<LabDragonBarTierElement>();
    private List<LabDragonBarSkillElement> m_skillElements = new List<LabDragonBarSkillElement>();

    private List<LabDragonBarElement> m_sortedElements = new List<LabDragonBarElement>();

    private int m_maxLevel;
    private int[] m_levelTier;
    private int[] m_levelSkill;
    private int m_currentLevel;
    private int[] m_unlockClassicTier;
    private int m_maxTierUnlocked;

    private List<DefinitionNode> m_definitionSkill;


    //---[Generic Methods]------------------------------------------------------


    //---[Build Methods]--------------------------------------------------------
    private void CreateElements() {
        int levelElementsCount = m_maxLevel - m_levelTier.Length - m_levelSkill.Length;

        if (levelElementsCount > m_levelElements.Count) {
            for (int i = m_levelElements.Count; i < levelElementsCount; ++i) {
                GameObject go = Instantiate(m_elementLevelPrefab);
                go.transform.SetParent(m_content.transform, false);
                m_levelElements.Add(go.GetComponent<LabDragonBarElement>());
            }
        }

        if (m_levelSkill.Length > m_skillElements.Count) {
            for (int i = m_skillElements.Count; i < m_levelSkill.Length; ++i) {
                GameObject go = Instantiate(m_elementSkillPrefab);
                go.transform.SetParent(m_content.transform, false);
                m_skillElements.Add(go.GetComponent<LabDragonBarSkillElement>());
            }
        }

        if (m_levelTier.Length > m_tierElements.Count) {
            for (int i = m_tierElements.Count; i < m_levelTier.Length; ++i) {
                GameObject go = Instantiate(m_elementTierPrefab);
                LabDragonBarTierElement tierElement = go.GetComponent<LabDragonBarTierElement>();
                tierElement.SetTier(i);
                go.transform.SetParent(m_content.transform, false);
                m_tierElements.Add(tierElement);
            }
        }

        //Hide everything
        if (m_tooltip != null) {
            m_tooltip.gameObject.SetActive(false);
        }

        for (int i = 0; i < m_levelElements.Count; ++i) {
            m_levelElements[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < m_tierElements.Count; ++i) {
            m_tierElements[i].gameObject.SetActive(false);               
        }

        for (int i = 0; i < m_skillElements.Count; ++i) {
            if (i < m_definitionSkill.Count) {
                m_skillElements[i].SetDefinition(m_definitionSkill[i]);
            }
            m_skillElements[i].SetTooltip(m_tooltip);
            m_skillElements[i].gameObject.SetActive(false);
        }
    }

    public void DestroyElements() {
        for (int i = 0; i < m_levelElements.Count; ++i) {
            if (m_levelElements[i] != null) 
                Object.DestroyImmediate(m_levelElements[i].gameObject);
        }
        m_levelElements.Clear();

        for (int i = 0; i < m_tierElements.Count; ++i) {
            if (m_tierElements[i] != null) 
                Object.DestroyImmediate(m_tierElements[i].gameObject);
        }
        m_tierElements.Clear();

        for (int i = 0; i < m_skillElements.Count; ++i) {
            if (m_skillElements[i] != null) 
                Object.DestroyImmediate(m_skillElements[i].gameObject);
        }
        m_skillElements.Clear();

        m_sortedElements.Clear();
    }

    private void ArrangeElements() {
        float contentWidth = m_content.sizeDelta.x;

        // sum up the space reserved for Tier icons
        float tiersWidth = 0f;

        for (int i = 0; i < m_levelTier.Length; ++i) {
            float scale = m_scaleTiersCurve.Evaluate(m_levelTier[i] / m_maxLevel);
            float width = m_tierElements[0].GetWidth() * scale;

            tiersWidth += width + m_blankSpace * 2f;
        }

        // sum up the space reserved levels and skills
        float levelCount = (m_maxLevel - m_tierElements.Count);

        // get the width of each level slot
        float levelWidth = (contentWidth - tiersWidth - m_blankSpace * levelCount) / levelCount;
        float levelScale = levelWidth / m_levelElements[0].GetWidth();

        // order all the elements. Each element has its pivot at bottom center corner
        float deltaX = 0f;

        m_sortedElements.Clear();

        int m_tierElementIndex = 0;
        int m_levelElementIndex = 0;
        int m_skillElementIndex = 0;

        for (int i = 0; i < m_maxLevel; ++i) {
            LabDragonBarElement.State elementState;
            LabDragonBarElement element;
            float scaleFactor = 1f;
            float posY = 0f;

            if (i <= m_currentLevel) {
                elementState = LabDragonBarElement.State.OWNED;
            } else {
                if (m_unlockClassicTier[m_tierElementIndex] <= m_maxTierUnlocked) {
                    elementState = LabDragonBarElement.State.AVAILABLE;
                } else {
                    elementState = LabDragonBarElement.State.LOCKED;
                }
            }

            if (m_levelTier.IndexOf(i) >= 0) {
                // this level is a Tier icon
                scaleFactor = m_scaleTiersCurve.Evaluate((float)i / m_maxLevel);
                posY = m_levelElements[0].GetHeight() * m_scaleLevelsCurve.Evaluate((float)i / m_maxLevel) * 0.5f;

                element = m_tierElements[m_tierElementIndex];
                element.SetGlobalScale(scaleFactor, scaleFactor);

                m_tierElementIndex++;
            } else if (m_levelSkill.IndexOf(i) >= 0) {
                // this is a level with a skill
                scaleFactor = levelScale;

                element = m_skillElements[m_skillElementIndex];
                element.SetLocalScale(scaleFactor, 1f);

                m_skillElementIndex++;
            } else {
                // this is a standard level
                scaleFactor = levelScale;

                element = m_levelElements[m_levelElementIndex];
                element.SetLocalScale(scaleFactor, m_scaleLevelsCurve.Evaluate((float)i / m_maxLevel));

                m_levelElementIndex++;
            }

            float offsetY = m_positionLevelsCurve.Evaluate(((float)i / m_maxLevel)) * m_positionCurveScale;
            float width = element.GetWidth() * scaleFactor;

            deltaX += (width + m_blankSpace) * 0.5f;
            element.SetPos(deltaX, posY + offsetY);
            deltaX += (width + m_blankSpace) * 0.5f;

            element.gameObject.SetActive(true);
            element.SetState(elementState);

            //
            m_sortedElements.Add(element);
        }
    }

    public void BuildFromDragonData(DragonDataSpecial _dragonData) {
        m_maxLevel = _dragonData.maxLevel + 1;

        // tier data
        List<DefinitionNode> tierDefs = _dragonData.specialTierDefsByOrder;
        m_levelTier = new int[tierDefs.Count];
        m_unlockClassicTier = new int[tierDefs.Count];
        for (int i = 0; i < tierDefs.Count; ++i) {
            m_levelTier[i] = tierDefs[i].GetAsInt("upgradeLevelToUnlock");
            m_unlockClassicTier[i] = (int)IDragonData.SkuToTier(tierDefs[i].GetAsString("mainProgressionRestriction"));
        }

        //skills
        m_definitionSkill = _dragonData.specialPowerDefsByOrder;
        m_levelSkill = new int[m_definitionSkill.Count];
        for (int i = 0; i < m_definitionSkill.Count; ++i) {
            m_levelSkill[i] = m_definitionSkill[i].GetAsInt("upgradeLevelToUnlock");
        }

        m_currentLevel = _dragonData.GetLevel();
        m_maxTierUnlocked = (int)DragonManager.biggestOwnedDragon.tier;

        CreateElements();
        ArrangeElements();
    }

    public void BuildUsingDebugValues() {
        m_maxLevel = m_debugMaxLevel;
        m_levelTier = m_debugLevelTier;
        m_levelSkill = m_debugLevelSkill;
        m_currentLevel = m_debugCurrentLevel;
        m_unlockClassicTier = new int[] {1, 2, 3, 4};

        m_maxTierUnlocked = m_debugMaxTierUnlocked;

        m_definitionSkill = new List<DefinitionNode>(3);

        CreateElements();
        ArrangeElements();
    }

    public void AddLevel() {
        m_currentLevel++;
        if (m_currentLevel < m_maxLevel) {
            m_sortedElements[m_currentLevel].SetState(LabDragonBarElement.State.OWNED);
        }
    }

    //---[Callbacks]----------------------------------------------------------//
}
