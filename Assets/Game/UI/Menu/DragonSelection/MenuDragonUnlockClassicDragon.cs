
//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using System.Text;


//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Control unlock/acquire/unavailable UI for classic dragons in the menu.
/// Depending on the selected dragon lockState, a different object will be displayed.
/// TODO:
/// - Check active dragon discounts
/// </summary>
public class MenuDragonUnlockClassicDragon : MenuDragonUnlock
{

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

	/// <summary>
	/// Refresh with data from given dragon and trigger animations.
	/// </summary>
	/// <param name="_data">The data of the selected dragon.</param>
	/// <param name="_animate">Whether to trigger animations or not.</param>
	public override void Refresh(IDragonData _data, bool _animate) {

        // Shouldn't be showing classic dragons
        if (!(_data is DragonDataClassic))
        {
            return;
        }

        // Stop any pending coroutines
        if (m_delayedShowCoroutine != null) {
			StopCoroutine(m_delayedShowCoroutine);
			m_delayedShowCoroutine = null;
		}

		// Trigger animation?
		if(_animate && m_changeAnim != null) {
			// If object is visible, hide first and Refresh the info when the object is hidden
			if(m_changeAnim.visible) {
				// Trigger hide animation
				m_changeAnim.Hide();

				// Refresh info once the object is hidden
				m_delayedShowCoroutine = UbiBCN.CoroutineManager.DelayedCall(() => {
					// Refresh info
					RefreshInfo(_data);

					// Trigger show animation
					m_changeAnim.Show();
				}, m_changeAnim.tweenDuration);	// Use hide animation duration as delay for the coroutine
			} else {
				//  Object already hidden, refresh info and trigger show animation
				RefreshInfo(_data);
				m_changeAnim.Show();
			}
		} else {
			// Just refresh info immediately
			RefreshInfo(_data);
			if(m_changeAnim != null) m_changeAnim.Show(false);
		}
	}

	/// <summary>
	/// Refresh texts and visibility to match given dragon.
	/// Doesn't trigger any animation.
	/// </summary>
	/// <param name="_data">Data.</param>
	private void RefreshInfo(IDragonData _data) {
		// Aux vars
		bool show = true;

		// Update hc unlock button
		// Display?
		show = _data.CheckUnlockWithPC();
		Toggle(m_hcRoot, show);

		// Refresh info
		if(show && m_hcPriceSetup != null) {
			// [AOC] UIDragonPriceSetup makes it easy for us!
			m_hcPriceSetup.InitFromData(_data, UserProfile.Currency.HARD);
		}

		// Update sc unlock button
		// Display?
		show = _data.CheckUnlockWithSC();
		Toggle(m_scRoot, show);

		// Refresh info
		if(show && m_scPriceSetup != null) {
			// [AOC] UIDragonPriceSetup makes it easy for us!
			m_scPriceSetup.InitFromData(_data, UserProfile.Currency.SOFT);
		}

		// Update unavailable info
		if(m_unavailableInfoText != null) {
			// Display?
			show = CheckUnavailable(_data);
			Toggle(m_unavailableRoot, show);

			// Refresh info
			if(show) {
				// Get list of dragons required to unlock and compose a string with their names
				string separator = LocalizationManager.SharedInstance.Localize("TID_GEN_LIST_SEPARATOR");
				StringBuilder sb = new StringBuilder();
				IDragonData unlockDragonData = null;
				for(int i = 0; i < _data.unlockFromDragons.Count; ++i) {
					// Get data for pre-requisite dragon
					unlockDragonData = DragonManager.GetDragonData(_data.unlockFromDragons[i]);

					// Attach separator if more than one
					if(i > 0) sb.Append(", ");

					// [AOC] Use dragon's tier color
					//		 We're don't use Colors.Tag to avoid creating a new StringBuilder
					sb.Append("<color=").Append(UIConstants.GetDragonTierColor(unlockDragonData.tier).ToHexString("#")).Append(">");
					sb.Append(unlockDragonData.def.GetLocalized("tidName"));
					sb.Append("</color>");
				}

				// Set text
				m_unavailableInfoText.Localize(
					m_unavailableInfoText.tid,
					sb.ToString()
				);
			}
		}
	}

    

	

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	// Implement in parent class
}
