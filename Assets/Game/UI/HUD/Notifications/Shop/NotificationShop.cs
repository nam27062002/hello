using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationShop : MonoBehaviour {
	
	[SeparatorAttribute("Currency icon")]
	[SerializeField] private Image m_coin;
	[SerializeField] private Image m_gem;

	[SeparatorAttribute("Currency amount")]
	[SerializeField] private TMPro.TextMeshProUGUI m_value;

	[SeparatorAttribute("Mode")]
	[SerializeField] private bool m_softCurrency = true;
	[SerializeField] private bool m_hardCurrency = false;

	public bool setSoftCurrency {
		set { 
			m_softCurrency = true;
			m_hardCurrency = false;

			m_coin.gameObject.SetActive(true);
			m_gem.gameObject.SetActive(true);
		}
	}

	public bool setHardCurrency {
		set { 
			m_softCurrency = false;
			m_hardCurrency = true;
		}
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
