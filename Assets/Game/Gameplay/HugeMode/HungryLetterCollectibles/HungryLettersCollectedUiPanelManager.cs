using FGOL;
using UnityEngine;

public class HungryLettersCollectedUiPanelManager : MonoBehaviour
{
	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	private GameObject[] m_lettersSprite;

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void OnEnable()
	{
		bool[] lettersCollected = HungryLettersManager.lettersCollected;
		Assert.Fatal(lettersCollected.Length == m_lettersSprite.Length, "The sprites lenght needs to be the same than the number of collectible letters !!");

		for(int i = 0; i < lettersCollected.Length; i++)
		{
			m_lettersSprite[i].SetActive(lettersCollected[i]);
		}
	}
}