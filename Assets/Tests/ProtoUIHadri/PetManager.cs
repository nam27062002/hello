using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PetManager : MonoBehaviour {

    public bool areSlotsFull;

    public GameObject[] slots;
    public GameObject[] selectedFeedback;

    //Save pets equipped for each slots
    public int[] petsInSlots;

    //Pet image possibles
    public Sprite[] sprites;

    //Current Slot
    public Sprite currentSlot1;
    public Sprite currentSlot2;
    public Sprite currentSlot3;

    // Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        //Get sprite for each slots
        attributeSprite();
    }

    public void attributeSlot(Sprite pet, GameObject currentPet, int petID)
    {
        //Case slot available
        if (hasEmptySlot())
        {
            //Check if pet is in a slot
            if (checkSlotWithPet(petID) == false)
            {
                petsInSlots[nextSlot()] = petID;
            }
            else
            {
                if (checkSlotWithPet(petID))
                {
                    unequip(petID);
                }
                else
                {
                    //Display message
                }

            }
        }
        else
        //Case slot full, unequip pet
        {
            //Check if pet is in a slot
            if (checkSlotWithPet(petID))
            {
                unequip(petID);
            }
            else
            {
                //Display message
            }

        }
    }

    public bool checkSlotWithPet(int petID)
    {
        if (petsInSlots[0] == petID || petsInSlots[1] == petID || petsInSlots[2] == petID)
        {
            return true;
        }
        else return false;
    }

    //public void clearFeedback()
    //{
    //    selectedFeedback[0].SetActive(false);
    //    selectedFeedback[1].SetActive(false);
    //    selectedFeedback[2].SetActive(false);
    //    selectedFeedback[3].SetActive(false);
    //}

    void unequip(int petID)
    {
        //Search slot with pet
        int associatedSlot;
        associatedSlot = 0;
        for (int i = 0; i <3; i ++)
        {
            if (petsInSlots[i] == petID) associatedSlot = i;
        }

        petsInSlots[associatedSlot] = 0;
    }

    void attributeSprite()
    {
        slots[0].GetComponent<Image>().sprite = sprites[petsInSlots[0]];
        slots[1].GetComponent<Image>().sprite = sprites[petsInSlots[1]];
        slots[2].GetComponent<Image>().sprite = sprites[petsInSlots[2]];
    }

    bool hasEmptySlot()
    {
        for (int i = 0; i < 3; i++)
        {
            if (petsInSlots[i] == 0) return true;
        }
        return false;
    }

    int nextSlot()
    {
        for (int i = 0; i < 3; i++)
        {
            if (petsInSlots[i] == 0) return i;
        }
        return 0;
    }
}
