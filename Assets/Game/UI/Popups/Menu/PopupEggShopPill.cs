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
	[SerializeField] private GameObject m_preview3DPrefab = null;
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
	private UIScene3D m_eggPreviewScene = null;	// Container holding the preview scene (camera, egg, decos, etc.)
	private EggController m_eggPreview = null;		// Actual egg 3D object

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_preview3DPrefab != null, "Required Field!");
		Debug.Assert(m_previewArea != null, "Required Field!");
		Debug.Assert(m_nameText != null, "Required Field!");

		// Get references
		m_snapPoint = GetComponent<ScrollRectSnapPoint>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Instantiate the 3D scene
		GameObject sceneObj = GameObject.Instantiate<GameObject>(m_preview3DPrefab);
		m_eggPreviewScene = sceneObj.GetComponent<UIScene3D>();
		UIScene3DManager.Add(m_eggPreviewScene);

		// Initialize the raw image
		m_previewArea.texture = m_eggPreviewScene.renderTexture;
		m_previewArea.color = Colors.white;

		// Initialize from the given egg sku
		InitFromDef(Definitions.GetDefinition(Definitions.Category.EGGS, m_eggSku));
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

		// Add some offset to avoid all eggs being rendered at the same place!
		if(m_eggPreviewScene != null) {
			m_eggPreviewScene.transform.Translate(_eggDef.GetAsFloat("shopOrder") * 100f, 0f, 0f);
		}

		// If we already have an egg loaded, destroy it
		if(m_eggPreview != null) {
			GameObject.Destroy(m_eggPreview.gameObject);
			m_eggPreview = null;
		}

		// Create a new dummy egg with the given definition
		Egg newEgg = Egg.CreateFromDef(_eggDef);
		newEgg.ChangeState(Egg.State.SHOP);

		// Load preview
		m_eggPreview = newEgg.CreateView();
		m_eggPreview.transform.SetParent(m_eggPreviewScene.transform, false);
		m_eggPreview.transform.localPosition = Vector3.zero;
		m_eggPreview.gameObject.SetLayerRecursively(UIScene3DManager.LAYER_NAME);

		// Init name
		//m_nameText.text = _eggDef.GetLocalized("tidName");
		m_nameText.text = _eggDef.sku;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
