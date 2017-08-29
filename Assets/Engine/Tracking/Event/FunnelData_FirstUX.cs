using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunnelData_FirstUX : FunnelData {
	//-- Step index ------------------------
	public enum Steps {
		Step_0 = 0,

		Count
	};
	//--------------------------------------


	//-- Step Names ------------------------
	private const string STEP_0 = "Step_0";
	//--------------------------------------


	//-- Public Methods --------------------
	public FunnelData_FirstUX() : base("custom.game.first.ux") {
		Setup((int)Steps.Count);

		SetupStep((int)Steps.Step_0, STEP_0);
		//etc
	}

	public string GetStepName(Steps _step)	 	{ return base.GetStepName((int)_step); }
	public int GetStepDuration(Steps _step)  	{ return base.GetStepDuration((int)_step); }
	public int GetStepTotalTime(Steps _step) 	{ return base.GetStepTotalTime((int)_step); }
	//--------------------------------------
}
