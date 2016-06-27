// PopupEggShopPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pill representing a single egg for the shop popup.
/// </summary>
[RequireComponent(typeof(ScrollRectSnapPoint))]
public class PopupEggShopPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup (temp)
	[SerializeField] private string m_eggSku = "";

	// Exposed References
	[SerializeField] private RawImage m_previewArea = null;
	[SerializeField] private Text m_nameText = null;

	// Data
	private DefinitionNode m_eggDef = null;
	public DefinitionNode eggDef {
		get { return m_eggDef; }
	}

	// Internal References
	private ScrollRectSnapPoint m_snapPoint = null;
	public ScrollRectSnapPoint snapPoint {
		get { return m_snapPoint; }
	}

	// Egg preview 3D scene
	private EggUIScene3D m_eggPreviewScene = null;	// Container holding the preview scene (camera, egg, decos, etc.)

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_previewArea != null, "Required Field!");
		Debug.Assert(m_nameText != null, "Required Field!");

		// Get references
		m_snapPoint = GetComponent<ScrollRectSnapPoint>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize from the given egg sku
		if (m_eggDef == null && m_eggSku != "") {
			InitFromDef(DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, m_eggSku));
		}
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {
		// Destroy 3D scene
		if(m_eggPreviewScene != null) {
			UIScene3DManager.Remove(m_eggPreviewScene);
			m_eggPreviewScene = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this pill with the given egg definition.
	/// </summary>
	/// <param name="_eggDef">The definition to be used, from the EGGS category.</param>
	public void InitFromDef(DefinitionNode _eggDef) {
		// Store definition
		m_eggDef = _eggDef;
		m_eggSku = m_eggDef.sku;

		// Create the 3D preview scene and initialize the raw image
		m_eggPreviewScene = EggUIScene3D.CreateEmpty();
		m_eggPreviewScene.InitRawImage(ref m_previewArea);

		// The scene will take care of everything
		m_eggPreviewScene.SetEgg(Egg.CreateFromDef(_eggDef));

		// Init name
		m_nameText.text = _eggDef.GetLocalized("tidName");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
