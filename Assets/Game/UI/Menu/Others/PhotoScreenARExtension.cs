// PhotoScreenARExtension.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace PhotoScreenAR {
	/// <summary>
	/// To listen to ARGameManager events.
	/// </summary>
	public class ARGameListener : ARGameManager.ARGameListenerBase {
		public PhotoScreenARFlow parentFlow = null;

		public ARGameListener(PhotoScreenARFlow _parentFlow) {
			parentFlow = _parentFlow;
		}

		public override void onProceedWithARSurfaceSelector(bool bCameraIsGranted) {
			parentFlow.OnCameraPermission(bCameraIsGranted);
		}

		public override void onNeedToAskForCameraPermission() {
			ARGameManager.SharedInstance.RequestNativeCameraPermission();
		}
	}
}