﻿// OpenClosePopupButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace PopupEditor {
	/// <summary>
	/// 
	/// </summary>
	[RequireComponent(typeof(Button))]
	public class OpenClosePopupButton : MonoBehaviour, IBroadcastListener {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		
		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//
		[FileList("Resources/UI/Popups", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
		[Comment("This script if for testing purposes, use <color=lightblue><b>PopupLauncher</b></color> instead!")]
		public string m_popupPath = "";
		public PopupController m_popup = null;

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		private void Awake() {
			GetComponent<Button>().onClick.AddListener(OnClick);
		}

		/// <summary>
		/// First update call.
		/// </summary>
		private void Start() {
			if(m_popup != null) {
				GameObject.Destroy(m_popup.gameObject);
			}
		}

		/// <summary>
		/// Component has been enabled.
		/// </summary>
		private void OnEnable() {
			Broadcaster.AddListener(BroadcastEventType.POPUP_DESTROYED, this);
		}

		/// <summary>
		/// Component has been disabled.
		/// </summary>
		private void OnDisable() {
			Broadcaster.RemoveListener(BroadcastEventType.POPUP_DESTROYED, this);
		}

        public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
        {
            switch(eventType)
            {
                case BroadcastEventType.POPUP_DESTROYED:
                {
                    PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                    OnPopupDestroyed(info.popupController);
                }break;
            }
        }
		//------------------------------------------------------------------//
		// OTHER METHODS													//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The linked button has been clicked.
		/// </summary>
		private void OnClick() {
			/*if(m_popup == null) {
				m_popup = PopupManager.OpenPopupInstant(m_popupPath);
			} else {
				m_popup.Close(true);
				m_popup = null;
			}*/
			m_popup = PopupManager.OpenPopupInstant(m_popupPath);
		}

		/// <summary>
		/// A popup has been destroyed.
		/// </summary>
		/// <param name="_popup">The popup that has just been destroyed.</param>
		private void OnPopupDestroyed(PopupController _popup) {
			if(_popup == m_popup) {
				m_popup = null;
			}
		}
	}
}