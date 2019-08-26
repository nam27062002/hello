using System.Collections.Generic;
using UnityEngine;

public class SpecialDragonBar : MonoBehaviour {

    [SerializeField] private GameObject m_elementLevelPrefab = null;
    [SerializeField] private GameObject m_elementSkillPrefab = null;
    [SerializeField] private GameObject m_elementTierPrefab = null;
	[Space]
    [SerializeField] private RectTransform m_content = null;
	[SerializeField] private Localizer m_levelText = null;
	[SerializeField] private SpecialDragonBarTooltip m_skillTooltip = null;
	[SerializeField] private SpecialDragonBarTooltip m_tierTooltip = null;
	[Space]
    [SerializeField] private float m_blankSpace = 5f;
    [SerializeField] private AnimationCurve m_scaleTiersCurve = null;
    [SerializeField] private AnimationCurve m_scaleLevelsCurve = null;
    [SerializeField] private AnimationCurve m_positionLevelsCurve = null;
    [SerializeField] private float m_positionCurveScale = 10f;

    [Separator("Debug")]
    [SerializeField] private int m_debugMaxLevel = 30;
    [SerializeField] private int[] m_debugLevelTier = { 0, 10, 20, 30 };
    [SerializeField] private int[] m_debugLevelSkill = { 5, 15, 25 };
    [Space]
    [SerializeField] private int m_debugCurrentLevel = 1;
    [SerializeField] private int m_debugMaxTierUnlocked = 3;


    private List<SpecialDragonBarElement> m_levelElements = new List<SpecialDragonBarElement>();
    private List<SpecialDragonBarTierElement> m_tierElements = new List<SpecialDragonBarTierElement>();
    private List<SpecialDragonBarSkillElement> m_skillElements = new List<SpecialDragonBarSkillElement>();

    private List<SpecialDragonBarElement> m_sortedElements = new List<SpecialDragonBarElement>();

    private int m_maxLevel;
    private int[] m_levelSkill;
    private int[] m_levelTier;
    private int m_currentLevel;
    private int[] m_unlockClassicTier;
    private int m_maxTierUnlocked;

    private List<DefinitionNode> m_skillsDefinitions;
    private List<DefinitionNode> m_tiersDefinitions;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    private void Awake() {
		// Clear any placeholder bar left from UI editing
		m_content.DestroyAllChildren(false);
		DestroyElements();

    }

    private void OnEnable ()
    {
        Messenger.AddListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnDragonLevelUpgraded);

    }

    private void OnDisable()
    {
        Messenger.RemoveListener<DragonDataSpecial>(MessengerEvents.SPECIAL_DRAGON_LEVEL_UPGRADED, OnDragonLevelUpgraded);
    }


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    public void BuildFromDragonData(DragonDataSpecial _dragonData)
    {
        m_maxLevel = _dragonData.MaxLevel;

        // tier data
        m_tiersDefinitions = _dragonData.specialTierDefsByOrder;
        m_levelTier = new int[m_tiersDefinitions.Count - 1];

        // Skip the base level (0)
        for (int i = 1; i < m_tiersDefinitions.Count; i++)
        {
            m_levelTier[i - 1] = m_tiersDefinitions[i].GetAsInt("upgradeLevelToUnlock");
        }

        //skills
        m_skillsDefinitions = _dragonData.specialPowerDefsByOrder;
        m_levelSkill = new int[m_skillsDefinitions.Count];
        for (int i = 0; i < m_skillsDefinitions.Count; ++i)
        {
            m_levelSkill[i] = m_skillsDefinitions[i].GetAsInt("upgradeLevelToUnlock");
        }


        SetLevel(_dragonData.Level);
        m_maxTierUnlocked = (int)DragonManager.biggestOwnedDragon.tier;

        CreateElements();
        ArrangeElements();
    }


    private void SetLevel(int _level) {
		// Update level var
		m_currentLevel = _level;

        Refresh();
	}


	private void CreateElements() {
        int levelElementsCount = m_maxLevel - m_levelSkill.Length;

        if (levelElementsCount > m_levelElements.Count) {
            for (int i = m_levelElements.Count; i < levelElementsCount; ++i) {
                GameObject go = Instantiate(m_elementLevelPrefab);
                go.transform.SetParent(m_content.transform, false);
                m_levelElements.Add(go.GetComponent<SpecialDragonBarElement>());
            }
        }

        if (m_levelSkill.Length > m_skillElements.Count) {
            for (int i = m_skillElements.Count; i < m_levelSkill.Length; ++i) {
                GameObject go = Instantiate(m_elementSkillPrefab);
                go.transform.SetParent(m_content.transform, false);
				m_skillElements.Add(go.GetComponent<SpecialDragonBarSkillElement>());
            }
        }

        if (m_levelTier.Length > m_tierElements.Count) {
            for (int i = m_tierElements.Count; i < m_levelTier.Length; ++i) {
                GameObject go = Instantiate(m_elementTierPrefab);
                SpecialDragonBarTierElement tierElement = go.GetComponent<SpecialDragonBarTierElement>();
                tierElement.SetTier(m_tiersDefinitions[i + 1]); // Plus 1 because tier 0 is the starting tier
                go.transform.SetParent(m_content.transform, false);
                m_tierElements.Add(tierElement);
            }
        }

        //Hide everything
        if (m_skillTooltip != null) {
            m_skillTooltip.gameObject.SetActive(false);
        }

        for (int i = 0; i < m_levelElements.Count; ++i) {
            m_levelElements[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < m_tierElements.Count; ++i) {
			m_tierElements[i].SetTooltip(m_tierTooltip);
            m_tierElements[i].gameObject.SetActive(false);   
        }

        for (int i = 0; i < m_skillElements.Count; ++i) {
            if (i < m_skillsDefinitions.Count) {
                m_skillElements[i].SetDefinition(m_skillsDefinitions[i]);
            }
            m_skillElements[i].SetTooltip(m_skillTooltip);
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
        float contentWidth = m_content.rect.width;

        // sum up the space reserved for Tier icons
        float tiersWidth = 0f;

        for (int i = 0; i < m_levelTier.Length; ++i)
        {
            float scale = m_scaleTiersCurve.Evaluate((float)m_levelTier[i] / m_maxLevel);
            float width = m_tierElements[0].GetWidth() * scale;

            tiersWidth += width + m_blankSpace;
        }

        // sum up the space reserved for levels 
        float levelCount = (m_maxLevel - m_skillElements.Count - m_tierElements.Count);

        // get the width of each level slot
        float levelWidth = (contentWidth - tiersWidth - m_blankSpace * levelCount) / levelCount;
        float levelScale = levelWidth / m_levelElements[0].GetWidth();

        // order all the elements. Each element has its pivot at bottom center corner
        float deltaX = -m_content.rect.width * m_content.pivot.x;
        float deltaY = -(m_content.rect.height * 0.5f) * m_content.pivot.y;

        m_sortedElements.Clear();

        int m_tierElementIndex = 0;
        int m_levelElementIndex = 0;
        int m_skillElementIndex = 0;

        
        for (int i = 1; i <= m_maxLevel; ++i) {
            SpecialDragonBarElement.State elementState;
            SpecialDragonBarElement element;
            float scaleFactor = 1f;
            float posY = 0f;

            if (i <= m_currentLevel ) { 
                elementState = SpecialDragonBarElement.State.OWNED;
            } else {
                elementState = SpecialDragonBarElement.State.LOCKED;
            }

            if (m_levelTier.IndexOf(i) >= 0)
            {
                // this level is a Tier icon
                scaleFactor = m_scaleTiersCurve.Evaluate((float)i / m_maxLevel);
                posY = m_levelElements[0].GetHeight() * m_scaleLevelsCurve.Evaluate((float)i / m_maxLevel) * 0.5f;

                element = m_tierElements[m_tierElementIndex];
                element.SetGlobalScale(scaleFactor, scaleFactor);

                (element as SpecialDragonBarTierElement).SetUnlockLevel(i);

                m_tierElementIndex++;
            }
            else if (m_levelSkill.IndexOf(i) >= 0) {
                // this is a level with a skill
                scaleFactor = levelScale;

                element = m_skillElements[m_skillElementIndex];
                element.SetLocalScale(scaleFactor, 1f);

				(element as SpecialDragonBarSkillElement).SetUnlockLevel(i);

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
            element.SetPos(deltaX, deltaY + posY + offsetY);
            deltaX += (width + m_blankSpace) * 0.5f;

            element.gameObject.SetActive(true);

            // Update the element look
            element.SetState(elementState);

            m_sortedElements.Add(element);
        }
    }

    

    public void BuildUsingDebugValues() {
        m_maxLevel = m_debugMaxLevel;
        m_levelSkill = m_debugLevelSkill;
        m_levelTier = m_debugLevelTier;
        SetLevel(m_debugCurrentLevel);
        m_unlockClassicTier = new int[] {1, 2, 3, 4};

        m_maxTierUnlocked = m_debugMaxTierUnlocked;

        m_skillsDefinitions = new List<DefinitionNode>(3);

        CreateElements();
        ArrangeElements();
    }

    public void AddLevel() {
		SetLevel(m_currentLevel + 1);
        if (m_currentLevel <= m_maxLevel) {
            // The level 1 element is in the position 0 of the array
            m_sortedElements[m_currentLevel - 1].SetState(SpecialDragonBarElement.State.OWNED);
        }
    }

    /// <summary>
    /// Refresh the GUI
    /// </summary>
    private void Refresh()
    {
        // Refresh visuals
        if (m_levelText != null)
        {
            int level = m_currentLevel; //Mathf.Min(m_currentLevel, m_maxLevel - 1);	// Cap level?
            m_levelText.Localize(
                m_levelText.tid,
                StringUtils.FormatNumber(level),
                StringUtils.FormatNumber(m_maxLevel)        // If using "Level 14" format, this parameter will just be ignored
            );
        }
    }

    //---[Callbacks]----------------------------------------------------------//
    /// <summary>
    /// A dragon stat has been upgraded.
    /// </summary>
    private void OnDragonLevelUpgraded(DragonDataSpecial _dragonData)
    {
        // Let's just refresh for now
        AddLevel();
    }

  
}
