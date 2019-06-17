// UISpriteAddressablesLoader.cs

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to load 3D prefabs into a UI canvas.
/// </summary>
public class UISpriteAddressablesLoader : MonoBehaviour {
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed

    [SerializeField] private string m_assetId = "";
    [SerializeField] private bool m_loadOnAwake = false;

    [Space]
    [SerializeField] private Image m_image = null;

    [Space]
    [SerializeField] private GameObject m_loadingPrefab = null;
    public GameObject loadingPrefab {
        get { return m_loadingPrefab; }
        set { m_loadingPrefab = value; }
    }

    // Internal
    private AddressablesOp m_loadingRequest = null;
    public AddressablesOp loadingRequest {
        get { return m_loadingRequest; }
    }

    private GameObject m_loadingSymbol = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        m_image.sprite = null;
        m_image.enabled = false;

        // Show loading icon from start
        ShowLoading(true);

        // If defined, start loading
        if (m_loadOnAwake) {
            LoadAsync();
        }
    }

    /// <summary>
    /// Component has been enabled.
    /// </summary>
    private void OnEnable() {

    }

    /// <summary>
    /// A change has been done in the inspector.
    /// </summary>
    private void OnDestroy() {
        // Delete pending requests
        if (m_loadingRequest != null) {
            m_loadingRequest.Cancel();
            m_loadingRequest = null;
        }

        // Delete instance
        m_image.sprite = null;

        // Destroy loading icon
        ShowLoading(false);
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    public void Load() {
        // If we have an async request running, kill it
        m_loadingRequest = null;

        // Load and instantiate the prefab
        m_image.sprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(m_assetId);
        m_image.enabled = true;
    }

    public AddressablesOp LoadAsync(string _assetId) {
        m_assetId = _assetId;

        return LoadAsync();
    }

    public AddressablesOp LoadAsync() {
        // Disable the image component to avoid the white placeholder until the load is finished.
        m_image.enabled = false;
        m_image.sprite = null;

        // We don't care if we're already loading another asset, it will be ignored once done loading
        m_loadingRequest = HDAddressablesManager.Instance.LoadAssetAsync(m_assetId);

        ShowLoading(true);

        return m_loadingRequest;
    }

    /// <summary>
    /// Update loop.
    /// </summary>
    private void Update() {
        if (m_loadingRequest != null) {
            if (m_loadingRequest.isDone) {
                m_image.sprite = m_loadingRequest.GetAsset<Sprite>();
                m_image.enabled = true;
                m_loadingRequest = null;
            }
        }
    }

    /// <summary>
    /// Show/hide loading icon.
    /// Since loading is usually only done once, the icon will be 
    /// instantiated/destroyed every time to free resources.
    /// </summary>
    /// <param name="_show">Whether to show or hide the icon.</param>
    private void ShowLoading(bool _show) {
        if (_show) {
            // If loading icon not instantiated, do it now
            if (m_loadingSymbol == null) {
                if (m_loadingPrefab != null) {
                    m_loadingSymbol = Instantiate(m_loadingPrefab, this.transform, false);
                }
            } else {
                m_loadingSymbol.SetActive(true);
            }
        } else {
            if (m_loadingSymbol != null) {
                Destroy(m_loadingSymbol);
                m_loadingSymbol = null;
            }
        }
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

}