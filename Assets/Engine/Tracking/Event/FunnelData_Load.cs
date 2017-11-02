using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunnelData_Load : FunnelData {
	//-- Step index ------------------------
	public enum Steps {
		_01_persistance = 0,
		_02_game_loaded,
		_03_terms_and_conditions,
		_04_click_play,

		Count
	};
	//--------------------------------------


	//-- Step Names ------------------------
	private const string STEP_01_persistance 			= "01.persistance";
	private const string STEP_02_game_loaded 			= "02.game_loaded";
	private const string STEP_03_terms_and_conditions 	= "03.a terms_and_conditions";
	private const string STEP_04_click_play 			= "03.b click_play";
	//--------------------------------------


	//-- Public Methods --------------------
	public FunnelData_Load() : base("custom.game.load.funnel") {
		Setup((int)Steps.Count);

		SetupStep((int)Steps._01_persistance, 			STEP_01_persistance);
		SetupStep((int)Steps._02_game_loaded, 			STEP_02_game_loaded);
		SetupStep((int)Steps._03_terms_and_conditions, 	STEP_03_terms_and_conditions);
		SetupStep((int)Steps._04_click_play, 			STEP_04_click_play);
		//etc
	}

	public string GetStepName(Steps _step)	 	{ return base.GetStepName((int)_step); }
	public int GetStepDuration(Steps _step)  	{ return base.GetStepDuration((int)_step); }
	public int GetStepTotalTime(Steps _step) 	{ return base.GetStepTotalTime((int)_step); }
	//--------------------------------------
}
