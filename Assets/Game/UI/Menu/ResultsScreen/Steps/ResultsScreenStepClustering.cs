// ResultsScreenStepClustering.cs
// Hungry Dragon
// 
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepClustering : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {

		int gamesPlayed = UsersManager.currentUser.gamesPlayed;

		// Do not calcultate if we already know it
		if (!string.IsNullOrEmpty(UsersManager.currentUser.GetClusterId()))
			return false;

		DefinitionNode offerSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "offerSettings");

		// Calculate clustering at the end of the spcified run
		if (gamesPlayed != offerSettingsDef.GetAsInt("calculateClusterAtRun"))
			return false;


		return true;

	}

	/// <summary>
	/// Initialize and launch this step.
	/// </summary>
	override protected void DoLaunch() {

		// Request the cluster ID. At this moment we dont use it, but anyway send the request
        // to the server. So in the future, when we need the cluster id value it will
        // be already in the client
		ClusteringManager.Instance.GetClusterId();

		// Notify we're finished
		OnFinished.Invoke();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
}