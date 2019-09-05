

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a dragon level bar in the menu.
/// </summary>
public class MenuDragonInfo : MonoBehaviour {
    //------------------------------------------------------------------------//
    // PROPERTIES															  //
    //------------------------------------------------------------------------//

    [SerializeField] protected Localizer m_dragonNameText;
    public Localizer dragonNameText
    {
        get { return m_dragonNameText; }
    }

    [SerializeField] protected Localizer m_dragonDescText;
    public Localizer dragonDescText
    {
        get { return m_dragonDescText; }
    }

    [SerializeField] protected MenuDragonUnlock m_dragonUnlock;
    public MenuDragonUnlock dragonUnlock
    {
        get { return m_dragonUnlock; }
    }

    // Internal
    protected IDragonData m_dragonData = null;    // Last used dragon data

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    private void Awake()
    {
        // Subscribe to external events
        Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
        Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);
    }

    /// <summary>
    /// First update
    /// </summary>
    protected void OnEnable() {

        // Do a first refresh
        Refresh(InstanceManager.menuSceneController.selectedDragon);

	}

	/// <summary>
	/// Destructor
	/// </summary>
	protected void OnDisable() {

    }

    private void OnDestroy()
    {
        // Unsubscribe from external events
        Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
        Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);
    }



    //------------------------------------------------------------------------//
    // INTERNAL METHODS														  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// Refresh with data from a target dragon.
    /// </summary>
    /// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
    /// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
    /// <param name="_force">If true forces the refresh, even if the dragon has not changed since the las refresh</param>
    private void Refresh(string _sku, float _delay = -1f, bool _force = false)
    {
        // Ignore delay if disabled (coroutines can't be started with the component disabled)
        if (_delay > 0)
        {
            // Make a delayed call using the corouine manager
            UbiBCN.CoroutineManager.DelayedCall(
                () =>
                {
                    Refresh(DragonManager.GetDragonData(_sku), _force);
                }, _delay);

        }
        else
        {
            // Get new dragon's data from the dragon manager and do the refresh logic
            Refresh(DragonManager.GetDragonData(_sku), _force);
        }
    }


    /// <summary>
    /// Delayed refresh.
    /// </summary>
    /// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
    /// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
    /// <param name="_force">If true forces the refresh, even if the dragon has not changed since the las refresh</param>
    private IEnumerator RefreshDelayed(string _sku, float _delay = -1f, bool _force = false)
    {
        // If there is a delay, respect it
        if (_delay > 0f)
        {
            yield return new WaitForSeconds(_delay);
        }

        // Get new dragon's data from the dragon manager
        IDragonData data = DragonManager.GetDragonData(_sku);
        if (data == null) yield break;

        // Call virtual method
        Refresh(data, _force);
    }


    /// <summary>
	/// Update all fields with given dragon data.
	/// Must be override in each type of dragon
	/// </summary>
	/// <param name="_data">Dragon data.</param>
    /// <param name="_force">If true forces the refresh, even if the dragon has not changed since the las refresh</param>
	protected virtual void Refresh(IDragonData _data, bool _force = false)
    {
        // Implemented in children
    }

    /// <summary>
    /// Refresh the fields with the current dragon
    /// </summary>
    public void Refresh()
    {
        // Force update, even if the dragon didnt change
        Refresh(DragonManager.CurrentDragon, true);
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// A new dragon has been selected.
    /// </summary>
    /// <param name="_sku">The sku of the selected dragon.</param>
    private void OnDragonSelected(string _sku)
    {
        // Refresh after some delay to let the animation finish
        Refresh(_sku, 0, false);
    }


    /// <summary>
    /// The screen has changed
    /// </summary>
    /// <param name="_from">The source screen</param>
    /// <param name="_to">The destination screen</param>
	public void OnScreenChanged(MenuScreen _from, MenuScreen _to)
    {
        // If not yet initialized, get out
        if (m_dragonData == null) return;

        // Refresh after some delay to let the animation finish
        Refresh(m_dragonData.sku, 0.25f, true);
        
    }

    /// <summary>
    /// Info button has been pressed.
    /// </summary>
    public virtual void OnInfoButton() {
	}

}
