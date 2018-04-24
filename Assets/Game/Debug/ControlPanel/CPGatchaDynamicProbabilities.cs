using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CPGatchaDynamicProbabilities : MonoBehaviour {

	[SerializeField] private TMP_InputField m_gatchaTriesInput;
	[SerializeField] private Image m_circleCommon;
	[SerializeField] private Image m_circleRare;
	[SerializeField] private Image m_circleEpic;


	// Use this for initialization
	void Start () {
		
	}

	void OnEnable() {
		m_gatchaTriesInput.text = UsersManager.currentUser.openEggTriesWithoutRares.ToString();

		UpdateProbs();
	}


	//
	public void OnSetTry(string _str) {
		UsersManager.currentUser.openEggTriesWithoutRares = int.Parse(_str);

		UpdateProbs();
	}

	public void OnAddTry() {
		UsersManager.currentUser.openEggTriesWithoutRares++;
		m_gatchaTriesInput.text = UsersManager.currentUser.openEggTriesWithoutRares.ToString();

		UpdateProbs();
	}

	public void OnRemoveTry() {
		UsersManager.currentUser.openEggTriesWithoutRares--;
		if (UsersManager.currentUser.openEggTriesWithoutRares < 0)
			UsersManager.currentUser.openEggTriesWithoutRares = 0;
		m_gatchaTriesInput.text = UsersManager.currentUser.openEggTriesWithoutRares.ToString();

		UpdateProbs();
	}

	private void UpdateProbs() {
		EggManager.BuildDynamicProbabilities();

		float probCommon = EggManager.getProbabilityCommon();
		float probRare = EggManager.getProbabilityRare();
		float probEpic = EggManager.getProbabilityEpic();

		m_circleCommon.fillAmount 	= probCommon;
		m_circleRare.fillAmount 	= probCommon + probRare;
		m_circleEpic.fillAmount 	= probCommon + probRare + probEpic;
	}
}
