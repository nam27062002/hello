using UnityEngine;
using System.Collections;

public class SaveDataPill : MonoBehaviour 
{

	public UnityEngine.UI.Text m_softCurrency;
	public UnityEngine.UI.Text m_hardCurrency;
	UserProfile m_user;

	public void Setup( UserProfile user )
	{
		m_user = user;

		m_softCurrency.text =  StringUtils.FormatNumber(m_user.coins);
		m_hardCurrency.text =  StringUtils.FormatNumber(m_user.pc);
	}
}
