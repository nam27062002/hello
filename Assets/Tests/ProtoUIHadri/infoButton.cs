using UnityEngine;
using System.Collections;

public class infoButton : MonoBehaviour
{

    public GameObject popup;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClick()
    {
        popup.SetActive(true);
    }

    public void closePopup()
    {
        popup.SetActive(false);
    }
}