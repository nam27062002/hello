using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CPModCustom : MonoBehaviour {
    [Separator("GameObjects")]
    [SerializeField] private TextMeshProUGUI m_label = null;
	[SerializeField] private Toggle m_toggle = null;
    [SerializeField] private TMP_InputField m_param1 = null;
    [SerializeField] private TMP_InputField m_param2 = null;

    [Separator("Mod Data")]
    [SerializeField] private string m_type = "";
    [SerializeField] private string m_target = "";
    [SerializeField] private string m_sku = "custom";
    [SerializeField] private string m_uiCategory = "";
    [SerializeField] private string m_iconPath = "";
    [SerializeField] private string m_tidName = "";
    [SerializeField] private string m_tidDesc = "";
    [SerializeField] private string m_tidDescShort = "";

    private bool m_initialized = false;
    private Modifier m_mod;


    void OnEnable() {
        if (!m_initialized) {
            Init();
            m_initialized = true;
        }
    }


    public void Init() {
		m_toggle.isOn = false;
		m_toggle.onValueChanged.AddListener(OnValueChanged);
    }


	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The toggle has changed.
	/// </summary>
	public void OnValueChanged(bool _newValue) {
		if (_newValue) {
            CreateMod();
			if (m_mod is ModifierDragon) {
				CPModifiers.CreateDragonMod(m_mod as ModifierDragon);
			} else {
				m_mod.Apply();
			}
		} else {
			if (m_mod is ModifierDragon) {
				CPModifiers.DestroyDragonMod(m_mod as ModifierDragon);
			} else {
				m_mod.Remove();
			}
            m_mod = null;
		}
	}

    private void CreateMod() {
        SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();
        data.Add("type", m_type);
        data.Add("target", m_target);
        data.Add("sku", m_sku);
        data.Add("param1", m_param1.text);
        data.Add("param2", m_param2.text);
        data.Add("uiCategory", m_uiCategory);
        data.Add("icon", m_iconPath);
        data.Add("tidName", m_tidName);
        data.Add("tidDesc", m_tidDesc);
        data.Add("tidDescShort", m_tidDescShort);

        m_mod = Modifier.CreateFromJson(data);
    }
}
