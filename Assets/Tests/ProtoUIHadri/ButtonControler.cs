using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ButtonControler : MonoBehaviour {

    public GameObject MYui;
    public GameObject otherUI1;
    public GameObject otherUI2;
    public GameObject otherUI3;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void displayMyMenu()
    {
        MYui.SetActive(true);
        MYui.GetComponent<DOTweenAnimation>().DORestart();
        otherUI1.SetActive(false);
        otherUI2.SetActive(false);
        otherUI3.SetActive(false);
    }
}
