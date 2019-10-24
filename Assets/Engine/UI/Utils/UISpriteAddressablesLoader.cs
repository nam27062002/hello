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


    [Tooltip ("Will show this sprite if the requested asset is not found")]
    [SerializeField] private Sprite m_assetLoadFailedImage = null;

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

        // [JOM] In some places the sprite is loaded out of this component,
        // so dont deactive/remove the sprite here (bug HDK-5410)
        //m_image.sprite = null;
        //m_image.enabled = false;

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
        Sprite sprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(m_assetId);
        if (sprite == null)
        {
            if (m_assetLoadFailedImage != null)
            {
                m_image.sprite = m_assetLoadFailedImage;
            }
            
        } else
        {
            m_image.sprite = sprite;
        }

        m_image.enabled = true;
    }

    public void Load(string assetId) {
        m_assetId = assetId;
        Load();        
    }

    public AddressablesOp LoadAsync(string _assetId) {
        m_assetId = _assetId;

        return LoadAsync();
    }

    public AddressablesOp LoadAsync() {
        // Remove the image until the load is finished.
        m_image.enabled = false;
        m_image.sprite = null;

        // If we already have an ongoing request, cancel it
        if (m_loadingRequest != null) {
            m_loadingRequest.Cancel();
            m_loadingRequest = null;
            ShowLoading(false);
        }

        // We don't care if we're already loading another asset, it will be ignored once done loading
        if (!string.IsNullOrEmpty(m_assetId)) {
            m_loadingRequest = HDAddressablesManager.Instance.LoadAssetAsync(m_assetId);
            ShowLoading(true);
        }
        else
        {
            // Trying to load a null asset. Show the 'asset load failed' image if specified.
            ShowFailImage();
        }

        return m_loadingRequest;
    }

    /// <summary>
    /// Update loop.
    /// </summary>
    private void Update() {
        if (m_loadingRequest != null) {
            if (m_loadingRequest.isDone) {
                if (m_loadingRequest.GetAsset<Sprite>() != null)
                {
                    m_image.sprite = m_loadingRequest.GetAsset<Sprite>();
                    m_image.enabled = true;
                    m_loadingRequest = null;

                    // Hide the loading prefab
                    ShowLoading(false);
                }else
                {
                    // The load has failed
                    ShowFailImage();
                }
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

    public bool IsVisible {
        get { return (m_image == null) ? false : m_image.enabled; }
        set { if (m_image != null) m_image.enabled = value; }
    }

    /// <summary>
    /// Assumes that the load has failed. Stop the load and show the proper 'asset load failed' image.
    /// </summary>
    private void ShowFailImage ()
    {
        if (m_assetLoadFailedImage != null)
        {
            m_image.sprite = m_assetLoadFailedImage;

            m_image.enabled = true;
            m_loadingRequest = null;

            // Hide the loading prefab
            ShowLoading(false);
        }
    }
    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

}