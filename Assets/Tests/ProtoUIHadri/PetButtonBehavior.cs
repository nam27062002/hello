using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PetButtonBehavior : MonoBehaviour {

    //Get Manager
    public GameObject manager;

    //Sprite for every State
    public Sprite petDefault;
    public Sprite petGlow;
    public Sprite petGlowSelected;
    public Sprite petSelected;
    public int petID;

    //Feedback selected
    public GameObject feedback;

    //Locked pet
    public bool isLocked;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void onClick()
    {

        if (isLocked) return;
        //manager.GetComponent<PetManager>().clearFeedback();
        manager.GetComponent<PetManagerHadri>().attributeSlot(petSelected, this.gameObject, petID);
        //Check if equipped to display or no the check icon
        if(manager.GetComponent<PetManagerHadri>().checkSlotWithPet(petID))
        {
            this.GetComponent<Image>().sprite = petSelected;
        }
        else this.GetComponent<Image>().sprite = petDefault;

        //feedback.SetActive(true); 
    }
}
