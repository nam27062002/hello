using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabDragonBar : MonoBehaviour {

    [SerializeField] private GameObject m_elementLevelPrefab;
    [SerializeField] private GameObject m_elementSkillPrefab;
    [SerializeField] private GameObject m_elementTierPrefab;

    [SerializeField] private RectTransform m_content;

    [SerializeField] private float m_blankSpace = 5f;
    [SerializeField] private AnimationCurve m_scaleTiersCurve;
    [SerializeField] private AnimationCurve m_scaleLevelsCurve;
    [SerializeField] private AnimationCurve m_positionLevelsCurve;
    [SerializeField] private float m_positionCurveScale = 10f;


    [Separator("debug")]
    [SerializeField] private int m_debugMaxLevel = 30;
    [SerializeField] private int[] m_debugLevelTier = { 1, 10, 20, 30 };
    [SerializeField] private int[] m_debugLevelSkill = { 5, 15, 25 };
    [Space]
    [SerializeField] private int m_debugCurrentLevel = 1;
    [SerializeField] private int m_debugUnlockedTier = 1;


    private List<LabDragonBarElement> m_levelElements;
    private List<LabDragonBarTierElement> m_tierElements;
    private List<LabDragonBarSkillElement> m_skillElements;

    private int m_maxLevel;
    private int[] m_levelTier;
    private int[] m_levelSkill;
    private int m_currentLevel;
    private int m_unlockedTier;


    //---[Generic Methods]------------------------------------------------------
	void Awake() {
        m_levelElements = new List<LabDragonBarElement>();
        m_tierElements = new List<LabDragonBarTierElement>();
        m_skillElements = new List<LabDragonBarSkillElement>();
	}
	

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
        for (int i = 0; i < m_levelElements.Count; ++i) {
            m_levelElements[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < m_tierElements.Count; ++i) {
            m_tierElements[i].gameObject.SetActive(false);               
        }

        for (int i = 0; i < m_skillElements.Count; ++i) {
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

        int m_tierElementIndex = 0;
        int m_levelElementIndex = 0;
        int m_skillElementIndex = 0;

        for (int i = 1; i <= m_maxLevel; ++i) {
            float scaleFactor = 1f;
            float posY = 0f;
            LabDragonBarElement element;

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

            Debug.Log(offsetY);

            deltaX += (width + m_blankSpace) * 0.5f;
            element.SetPos(deltaX, posY + offsetY);
            deltaX += (width + m_blankSpace) * 0.5f;

            element.gameObject.SetActive(true);

            if (i <= m_currentLevel) {
                element.SetState(LabDragonBarElement.State.OWNED);
            } else {
                int lockedLevels = m_levelTier[m_unlockedTier - 1];
                if (i > lockedLevels) {
                    element.SetState(LabDragonBarElement.State.LOCKED);
                } else {
                    element.SetState(LabDragonBarElement.State.AVAILABLE);
                }
            }
        }
    }

    private void BuildUsingDragonData() {
        
    }

    public void BuildUsingDebugValues() {
        m_maxLevel = m_debugMaxLevel;
        m_levelTier = m_debugLevelTier;
        m_levelSkill = m_debugLevelSkill;
        m_currentLevel = m_debugCurrentLevel;
        m_unlockedTier = m_debugUnlockedTier;

        CreateElements();
        ArrangeElements();
    }


    //---[Callbacks]----------------------------------------------------------//
}
