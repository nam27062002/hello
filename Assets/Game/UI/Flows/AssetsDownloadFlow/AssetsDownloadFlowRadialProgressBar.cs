// AssetsDownloadFlowRadialProgressBar.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 03/06/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class AssetsDownloadFlowRadialProgressBar : AssetsDownloadFlowProgressBar
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//


    [Space(10)]
    [Header("Radial Progress bar elements")]
    [SerializeField] private GameObject m_progressCircle;
    [SerializeField] private GameObject m_progressIcon;

    [SerializeField] private Transform m_inProgressGroup;
    [SerializeField] private GameObject m_inProgressArrow;
    [SerializeField] private GameObject m_inProgressBucket;

    [SerializeField] private Transform m_completedGroup;

    [SerializeField] private Transform m_errorGroup;
    [SerializeField] private GameObject m_errorIcon1;
    [SerializeField] private GameObject m_errorIcon2;
    [SerializeField] private GameObject m_errorBucket;



    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Show the proper progress bar elements and color them
    /// according to the new state of the download.
    /// </summary>
    /// <param name="_handle">Handle to be used.</param>
    public override void RefreshProgressBarElements(State _newState)
    {

        if (m_state != _newState || m_state == State.NOT_INITIALIZED)
        {

            // Get the proper colors according to the new state of the progress bar
            switch (_newState)
            {

                case State.IN_PROGRESS:
                    {

                        m_progressCircle.GetComponent<UIGradient>().SetValues(AssetsDownloadFlowSettings.progressBarDownloadingColor);
                        m_progressIcon.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconDownloadingColor;

                        m_inProgressGroup.gameObject.SetActive(true);
                        m_completedGroup.gameObject.SetActive(false);
                        m_errorGroup.gameObject.SetActive(false);


                        m_inProgressArrow.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconDownloadingColor;
                        m_inProgressBucket.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconDownloadingColor;

                        break;
                    }
                case State.COMPLETED:
                    {

                        m_progressCircle.GetComponent<UIGradient>().SetValues(AssetsDownloadFlowSettings.progressBarFinishedColor);
                        m_progressIcon.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconFinishedColor;

                        m_inProgressGroup.gameObject.SetActive(false);
                        m_completedGroup.gameObject.SetActive(true);
                        m_errorGroup.gameObject.SetActive(false);

                        break;
                    }
                default: // Error case
                    {

                        m_progressCircle.GetComponent<UIGradient>().SetValues(AssetsDownloadFlowSettings.progressBarErrorColor);
                        m_progressIcon.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconErrorColor;

                        m_inProgressGroup.gameObject.SetActive(false);
                        m_completedGroup.gameObject.SetActive(false);
                        m_errorGroup.gameObject.SetActive(true);

                        m_errorIcon1.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconErrorColor;
                        m_errorIcon2.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconErrorColor;
                        m_errorBucket.GetComponent<Image>().color = AssetsDownloadFlowSettings.iconErrorColor;

                        break;
                    }
            }

        }

        m_state = _newState;

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}