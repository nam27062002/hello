using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WorldMemoryController : MonoBehaviour { 

	//---------------------------------------
	[System.Serializable]
	public class UIContainers {
        private static Color COLOR_NORMAL = Color.white;
        private static Color COLOR_OVERFLOW = Color.red;

        [SerializeField] private GameObject root;

        private TMP_Text nameText;

		private Slider allSlider;
		private TMP_Text allText;

		private Slider meshSlider;
		private TMP_Text meshText;

		private Slider textureSlider;
		private TMP_Text textureText;

		private Slider animationsSlider;
		private TMP_Text animationsText;

        private Slider particlesSlider;
        private TMP_Text particlesText;

        private Slider audioSlider;
        private TMP_Text audioText;

        private Slider otherSlider;
        private TMP_Text otherText;

        //---------------------------------------
        private float m_maxMem;

		//---------------------------------------
		public void Setup(string name, float _maxValue) {			
            if (allSlider == null) {
                nameText = root.FindComponentRecursive<TMP_Text>("NameText");

                allSlider = root.FindComponentRecursive<Slider>("AllMemSlider");
                allText = root.FindComponentRecursive<TMP_Text>("AllMemText");

                meshSlider = root.FindComponentRecursive<Slider>("MeshMemSlider");
                meshText = root.FindComponentRecursive<TMP_Text>("MeshMemText");

                textureSlider = root.FindComponentRecursive<Slider>("TextureMemSlider");
                textureText = root.FindComponentRecursive<TMP_Text>("TextureMemText");

                animationsSlider = root.FindComponentRecursive<Slider>("AnimationMemSlider");
                animationsText = root.FindComponentRecursive<TMP_Text>("AnimationMemText");

                particlesSlider = root.FindComponentRecursive<Slider>("ParticlesMemSlider");
                particlesText = root.FindComponentRecursive<TMP_Text>("ParticlesMemText");

                audioSlider = root.FindComponentRecursive<Slider>("AudioMemSlider");
                audioText = root.FindComponentRecursive<TMP_Text>("AudioMemText");

                otherSlider = root.FindComponentRecursive<Slider>("OtherMemSlider");
                otherText = root.FindComponentRecursive<TMP_Text>("OtherMemText");
            }

            m_maxMem = _maxValue;            

            allSlider.minValue = 0f;
			allSlider.maxValue = _maxValue;

            if (nameText != null) {
                nameText.text = name;
            }

			if (meshSlider != null) {
				meshSlider.minValue = 0f;
				meshSlider.maxValue = m_maxMem;
			}

			if (textureSlider != null) {
				textureSlider.minValue = 0f;
				textureSlider.maxValue = m_maxMem;
			}

			if (animationsSlider != null) {
				animationsSlider.minValue = 0f;
				animationsSlider.maxValue = m_maxMem;
			}

            if (particlesSlider != null) {
                particlesSlider.minValue = 0f;
                particlesSlider.maxValue = m_maxMem;
            }

            if (audioSlider != null) {
                audioSlider.minValue = 0f;
                audioSlider.maxValue = m_maxMem;
            }

            if (otherSlider != null) {
                otherSlider.minValue = 0f;
                otherSlider.maxValue = m_maxMem;
            }
        }

        public void Update(MemoryData _data) {
            bool overflow = (_data.all > m_maxMem) ;            

            if (nameText != null) {
                nameText.color = (overflow) ? COLOR_OVERFLOW : COLOR_NORMAL;
            }

			allSlider.value = _data.all;
			allText.text = _data.all.ToString("F2") + "/" + m_maxMem + " MB";
            allText.color = (overflow) ? COLOR_OVERFLOW : COLOR_NORMAL;

			if (meshSlider != null) {
				meshSlider.value = _data.mesh;
				meshText.text = _data.mesh.ToString("F2") + " MB";
			}

			if (textureSlider != null) {
				textureSlider.value = _data.texture;
				textureText.text = _data.texture.ToString("F2") + " MB";
			}

			if (animationsSlider != null) {
				animationsSlider.value = _data.animations;
				animationsText.text = _data.animations.ToString("F2") + " MB";
			}

            if (particlesSlider != null) {
                particlesSlider.value = _data.particles;
                particlesText.text = _data.particles.ToString("F2") + " MB";
            }

            if (audioSlider != null) {
                audioSlider.value = _data.audio;
                audioText.text = _data.audio.ToString("F2") + " MB";
            }

            if (otherSlider != null) {
                otherSlider.value = _data.other;
                otherText.text = _data.other.ToString("F2") + " MB";
            }
        }

        public void SetIsVisible(bool isVisible) {
            root.SetActive(isVisible);
        }
	}

	public class MemoryData {
        public string label;
		public float all;
		public float mesh;
		public float texture;
		public float animations;
        public float particles;
        public float audio;
        public float other;

        public MemoryData(string label) {
            this.label = label;
        }

		public void Reset() {
			all = 0f;
			mesh = 0f;
			texture = 0f;
			animations = 0f;
            particles = 0f;
            audio = 0f;
            other = 0f;
		}

        public override string ToString() {
            return "Label: " + label + " mesh: " + mesh + " texture = " + texture + 
                   " animations = " + animations + " particles = " + particles + 
                   " audio = " + audio + " other = " + other + 
                   " TOTAL = " + all;
        }
	}   
	    
	//---------------------------------------
	[SerializeField] private Slider m_timerUI;
    [SeparatorAttribute]

    [SerializeField]
    private UIContainers m_memoryTotal;

    [SerializeField]
    private UIContainers[] m_containers;

    //---------------------------------------	

    private MemoryData m_totalMemoryData;
    private List<MemoryData> m_memoryDatas;

    private float m_timer;

    public Button m_takeSampleButton;

    //---------------------------------------    
    private HDMemoryProfiler m_memoryProfiler;
    private MemorySampleCollection m_sample;
    //---------------------------------------

    // Use this for initialization
    private void Start() {       
        m_memoryProfiler = new HDMemoryProfiler();

        m_timerUI.minValue = 0f;
		m_timerUI.maxValue = 10f;

        if (m_takeSampleButton != null)
        {
            m_takeSampleButton.onClick.AddListener(TakeASample);
        }

        CategorySet_Setup();
        SizeStrategy_Setup();

        OnReset(true);        
    }

    private void OnDestroy() {
        if (m_takeSampleButton != null) {
            m_takeSampleButton.onClick.RemoveListener(TakeASample);
        }

        CategorySet_OnDestroy();
        SizeStrategy_OnDestroy();
    }

    public void Setup(MemoryProfiler.CategorySet categorySet) {
        m_totalMemoryData = new MemoryData("all");

        if (m_containers != null && categorySet != null) {
            if (categorySet.CategoryConfigs != null) {
                int containersCount = m_containers.Length;
                int configsCount = categorySet.CategoryConfigs.Count;
                if (containersCount < configsCount) {
                    Debug.LogError("Not enouth containers");
                } else {                    
                    m_memoryDatas = new List<MemoryData>();

                    for (int i = 0; i < configsCount; i++) {
                        m_memoryDatas.Add(new MemoryData(categorySet.CategoryConfigs[i].Name));
                        m_containers[i].Setup(categorySet.CategoryConfigs[i].Name, categorySet.CategoryConfigs[i].MaxMemory);
                        m_containers[i].SetIsVisible(true);
                    }

                    for (int i = configsCount; i < containersCount; i++) {
                        m_containers[i].SetIsVisible(false);
                    }
                }
            }
        }

        if (m_memoryTotal != null) {
            m_memoryTotal.Setup("memory usage", categorySet.TotalMaxMemory);
            m_memoryTotal.SetIsVisible(true);
        }        
    }

    private void OnEnable() {
        // Subscribe to external events
        Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);        
    }

    /// <summary>
    /// Component disabled.
    /// </summary>
    private void OnDisable() {
        // Unsubscribe from external events
        Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);        
    }

    private void Update() {
        /*if (FlowManager.IsInGameScene()) {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0f) {
                TakeASample();                
            }

            m_timerUI.value = m_timer;
        } */       
    }

    public void TakeASample() {
        TakeASampleInternal(false);
    }

    private void TakeASampleInternal(bool reuseAnalysis) {
        // Disables the button so the user can't click again until the current sample has been processed
        if (m_takeSampleButton != null) {
            m_takeSampleButton.enabled = false;
        }

        OnReset(!reuseAnalysis);

        if (m_memoryDatas != null) {
            m_sample = m_memoryProfiler.Scene_TakeAGameSampleWithCategories(reuseAnalysis, m_categorySetName) as MemorySampleCollection;
            if (m_sample != null) {
                int count = m_memoryDatas.Count;
                for (int i = 0; i < count; i++) {
                    MemoryData mData = m_memoryDatas[i];
                    SampleToData(m_sample.GetSample(mData.label) as MemorySample, mData);
                }
            }

            m_totalMemoryData.all = BytesToMegaBytes(m_sample.GetTotalMemorySize());

            UpdateUI();
        }

        // Enables the button again as the operation has been performed completely
        if (m_takeSampleButton != null) {
            m_takeSampleButton.enabled = true;
        }
    }

    private void UpdateUIWithSample(MemorySampleCollection sample) {
        if (m_memoryDatas != null && sample != null) {                        
            int count = m_memoryDatas.Count;
            for (int i = 0; i < count; i++)
            {
                MemoryData mData = m_memoryDatas[i];
                SampleToData(sample.GetSample(mData.label) as MemorySample, mData);
            }            

            m_totalMemoryData.all = BytesToMegaBytes(sample.GetTotalMemorySize());

            UpdateUI();
        }
    }

	private void OnReset(bool clearAnalysis) {      
        if (m_memoryDatas != null) {
            int count = m_memoryDatas.Count;
            for (int i = 0; i < count; i++) {
                m_memoryDatas[i].Reset();
            }
        }

		UpdateUI();

		m_timer = 10f;
		m_timerUI.value = m_timer;

        m_memoryProfiler.Clear(clearAnalysis);      
    }
	    
    private void SampleToData(MemorySample sample, MemoryData data) {
        if (sample != null && data != null) {
            sample.TypeGroups_Apply(m_memoryProfiler.GameTypeGroups);
            float mesh = BytesToMegaBytes(sample.GetMemorySizePerTypeGroup(HDMemoryProfiler.GAME_TYPE_GROUPS_MESHES));
            float animation = BytesToMegaBytes(sample.GetMemorySizePerTypeGroup(HDMemoryProfiler.GAME_TYPE_GROUPS_ANIMATIONS));
            float texture = BytesToMegaBytes(sample.GetMemorySizePerTypeGroup(HDMemoryProfiler.GAME_TYPE_GROUPS_TEXTURES));
            float particles = BytesToMegaBytes(sample.GetMemorySizePerTypeGroup(HDMemoryProfiler.GAME_TYPE_GROUPS_PARTICLES));
            float audio = BytesToMegaBytes(sample.GetMemorySizePerTypeGroup(HDMemoryProfiler.GAME_TYPE_GROUPS_AUDIO));
            float other = BytesToMegaBytes(sample.GetMemorySizePerTypeGroup(MemorySample.TYPE_GROUPS_OTHER));
            float total = BytesToMegaBytes(sample.GetTotalMemorySize());

            data.all = total; //should we add the "other" value?
            data.mesh = mesh;
            data.texture = texture;
            data.animations = animation;
            data.particles = particles;
            data.audio = audio;
            data.other = other;
        }
    }

	private void UpdateUI() {
        if (m_totalMemoryData != null && m_memoryTotal != null) {
            m_memoryTotal.Update(m_totalMemoryData);
        }

        if (m_memoryDatas != null && m_containers != null) {
            int count = m_memoryDatas.Count;
            for (int i = 0; i < count; i++) {
                m_containers[i].Update(m_memoryDatas[i]);
            }                
        }       
    }

	private void OnLevelLoaded() {
		OnReset(true);
    }

    private float BytesToMegaBytes(long bytes) {
        return bytes / (1024f * 1024f);
    }

    private float BytesToKiloBytes(long bytes) {
        return bytes / (1024f);
    }

    #region categorySet
    [SerializeField]
    public TMP_Dropdown m_categorySet;

    private string m_categorySetName;
    private List<string> m_categorySetNames;

    private void CategorySet_Setup() {        
        // Options for the prefab drop down have to be created        
        if (m_categorySet != null) {
            m_categorySet.ClearOptions();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            TMP_Dropdown.OptionData optionData;
            m_categorySetNames = m_memoryProfiler.CategorySet_GetNames();
            int count = m_categorySetNames.Count;
            for (int i = 0; i < count; i++) {                                
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = m_categorySetNames[i];
                options.Add(optionData);                               
            }

            m_categorySet.AddOptions(options);

            m_categorySet.onValueChanged.AddListener(CategorySet_OnValueChanged);
        }                

        // We set the default category set
        CategorySet_SetName(HDMemoryProfiler.CATEGORY_SET_NAME_GAME_1_LEVEL);
    }

    private void CategorySet_OnDestroy() {
        if (m_categorySet != null) {
            m_categorySet.onValueChanged.RemoveListener(CategorySet_OnValueChanged);
        }
    }

    private void CategorySet_OnValueChanged(int newValue) {
        if (m_categorySetNames != null && newValue > -1 && newValue < m_categorySetNames.Count) {
            CategorySet_SetName(m_categorySetNames[newValue]);

            // If a sample was taken then we need to take another one to make the UI show the latest information
            if (m_sample != null) {
                TakeASampleInternal(true);
            }            
        } else {
            Debug.LogError("Not valid index for category set: " + newValue);
        }
    }

    private void CategorySet_SetName(string categorySetName)
    {
        if (categorySetName != m_categorySetName) {
            m_categorySetName = categorySetName;

            if (m_categorySet != null && m_categorySetNames != null) {
                m_categorySet.value = m_categorySetNames.IndexOf(m_categorySetName);
            }

            Setup(m_memoryProfiler.CategorySet_Get(m_categorySetName));
        }
    }
    #endregion

    #region sizeStrategy
    [SerializeField]
    public TMP_Dropdown m_sizeStrategy;
    
    private List<MemorySample.ESizeStrategy> m_sizeStrategyValues;

    private void SizeStrategy_Setup() {
        // Options for the prefab drop down have to be created        
        if (m_sizeStrategy != null) {
            m_sizeStrategy.ClearOptions();

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();

            TMP_Dropdown.OptionData optionData;

            m_sizeStrategyValues = new List<MemorySample.ESizeStrategy>();
            foreach (var value in System.Enum.GetValues(typeof(MemorySample.ESizeStrategy))) {
                m_sizeStrategyValues.Add((MemorySample.ESizeStrategy)value);
            }
            
            int count = m_sizeStrategyValues.Count;
            for (int i = 0; i < count; i++) {
                optionData = new TMP_Dropdown.OptionData();
                optionData.text = m_sizeStrategyValues[i].ToString();
                options.Add(optionData);
            }

            m_sizeStrategy.AddOptions(options);

            m_sizeStrategy.onValueChanged.AddListener(SizeStrategy_OnValueChanged);
        }

        // We set the default size strategy
        SizeStrategy_SetValue(MemorySample.ESizeStrategy.DeviceFull);
    }


    private void SizeStrategy_OnDestroy() {
        if (m_categorySet != null) {
            m_categorySet.onValueChanged.RemoveListener(SizeStrategy_OnValueChanged);
        }
    }

    private void SizeStrategy_OnValueChanged(int newValue) {
        if (m_sizeStrategyValues != null && newValue > -1 && newValue < m_sizeStrategyValues.Count) {
            SizeStrategy_SetValue(m_sizeStrategyValues[newValue]);            
        } else {
            Debug.LogError("Not valid index for size strategy: " + newValue);
        }
    }

    private void SizeStrategy_SetValue(MemorySample.ESizeStrategy value) {
        if (m_memoryProfiler.SizeStrategy != value) {                    
            if (m_sizeStrategy != null && m_sizeStrategyValues != null) {
                m_sizeStrategy.value = m_sizeStrategyValues.IndexOf(value);

                m_memoryProfiler.SizeStrategy = value;
            }

            if (m_sample != null) {
                m_sample.SizeStrategy = value;
                UpdateUIWithSample(m_sample);
            }
        }
    }   
    #endregion
}
