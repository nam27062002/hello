using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CPMod : MonoBehaviour {
	[SerializeField] private TextMeshProUGUI m_label;
	[SerializeField] private Toggle m_toggle;

	private Modifier m_mod;


	public void Init(DefinitionNode _def) {
		m_mod = Modifier.CreateFromDefinition(_def);

		m_label.text = _def.sku;

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
			if (m_mod is ModifierDragon) {
				CPModifiers.CreateDragonMod(m_mod as ModifierDragon);
			} else if ( m_mod.isLateModifier() ){
				CPModifiers.AddLaterMod(m_mod);
			}else{
				m_mod.Apply();
			}
		} else {
			if (m_mod is ModifierDragon) {
				CPModifiers.DestroyDragonMod(m_mod as ModifierDragon);
			} else if (m_mod.isLateModifier()){
				CPModifiers.RemoveLaterMod(m_mod);
			}else{
				m_mod.Remove();
			}
		}
	}
}
