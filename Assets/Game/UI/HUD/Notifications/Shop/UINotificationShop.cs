using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINotificationShop : MonoBehaviour {
	public static string DEFAULT_PREFAB_PATH = "UI/Popups/ResourcesFlow/PF_UINotificationShop";	// Just for comfort, change it if path changes


	[SerializeField] private float m_timeOnScreen;
	[SerializeField] private TMPro.TextMeshProUGUI m_value;


	private ShowHideAnimator m_animator;

	private float m_timer;
	private bool m_visible;


	void Awake() {
		m_animator = GetComponent<ShowHideAnimator>();
		m_visible = false;
	}

	private void Show(UserProfile.Currency _currency, long _amount) {		
		m_value.text = UIConstants.GetIconString(_amount, _currency, UIConstants.IconAlignment.LEFT);
		m_animator.Show(true);
	}

	void Update() {
		if (m_visible) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				m_animator.Hide(true);
				m_visible = false;
			}
		}
	}

	public void OnShowPostAnimation() {
		m_timer = m_timeOnScreen;
		m_visible = true;		
	}

	public void OnHidePostAnimation() {		
		Destroy(gameObject);
	}

	//
	// Factory
	//
	public static UINotificationShop CreateAndLaunch(UserProfile.Currency _currency, long _amount, Vector3 _pos, RectTransform _parent) {
		// Don't do anything if parent is not valid
		if(_parent == null) return null;

		// Load prefab
		string prefabPath = DEFAULT_PREFAB_PATH;
		GameObject prefab = Resources.Load<GameObject>(prefabPath);
		Debug.Assert(prefab != null, "Prefab " + prefabPath + " for NotificationShop not found!");

		// Create a new instance
		GameObject newObj = GameObject.Instantiate<GameObject>(prefab);
		newObj.name = "NotificationShop";

		// Get the NotificationShop component
		UINotificationShop notification = newObj.GetComponent<UINotificationShop>();

		// Attach to parent
		RectTransform notificationRt = notification.transform as RectTransform;
		notificationRt.localPosition = _pos;
		notificationRt.SetParent(_parent, false);
				
		// Generate and start animation
		notification.Show(_currency, _amount);

		return notification;
	}
}