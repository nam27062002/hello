﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunnelData_FirstUX : FunnelData {
	//-- Step index ------------------------
	public enum Steps {
		_01_loading_done = 0,
    	_02_clicked_play,
		_03_game_started,
    	_04_run_is_done,
    	_05_continue_clicked,
    	_06a_load_done,
		_06b_animation_done,
    	_07_close_tier_popup,
    	_08_continue_clicked,
    	_09_close_missions_popup,
    	_10_continue_clicked,
    	_11_load_is_done,

		Count
	};
	//--------------------------------------


	//-- Step Names ------------------------
	private const string STEP_01_loading_done	 		= "01.loading_done";
	private const string STEP_02_clicked_play	 		= "02.clicked_play";
	private const string STEP_03_game_started	 		= "03.game_started";
	private const string STEP_04_run_is_done	 		= "04.run_is_done";
	private const string STEP_05_continue_clicked		= "05.continue_clicked";
	private const string STEP_06a_load_done 			= "06.a load_done";
	private const string STEP_06b_animation_done	 	= "06.b animation_done";
	private const string STEP_07_close_tier_popup 		= "07.close_tier_popup";
	private const string STEP_08_continue_clicked		= "08.continue_clicked";
	private const string STEP_09_close_missions_popup	= "09.close_missions_popup";
	private const string STEP_10_continue_clicked 		= "10.continue_clicked";
	private const string STEP_11_load_is_done 			= "11.load_is_done";
	//--------------------------------------


	//-- Public Methods --------------------
	public FunnelData_FirstUX() : base("custom.game.first.ux") {
		Setup((int)Steps.Count);

		SetupStep((int)Steps._01_loading_done, 			STEP_01_loading_done);
		SetupStep((int)Steps._02_clicked_play, 			STEP_02_clicked_play);
		SetupStep((int)Steps._03_game_started, 			STEP_03_game_started);
		SetupStep((int)Steps._04_run_is_done, 			STEP_04_run_is_done);
		SetupStep((int)Steps._05_continue_clicked, 		STEP_05_continue_clicked);
		SetupStep((int)Steps._06a_load_done,		 	STEP_06a_load_done);
		SetupStep((int)Steps._06b_animation_done, 		STEP_06b_animation_done);
		SetupStep((int)Steps._07_close_tier_popup, 		STEP_07_close_tier_popup);
		SetupStep((int)Steps._08_continue_clicked, 		STEP_08_continue_clicked);
		SetupStep((int)Steps._09_close_missions_popup, 	STEP_09_close_missions_popup);
		SetupStep((int)Steps._10_continue_clicked, 		STEP_10_continue_clicked);
		SetupStep((int)Steps._11_load_is_done, 			STEP_11_load_is_done);
	}

	public string GetStepName(Steps _step)	 	{ return base.GetStepName((int)_step); }
	public int GetStepDuration(Steps _step)  	{ return base.GetStepDuration((int)_step); }
	public int GetStepTotalTime(Steps _step) 	{ return base.GetStepTotalTime((int)_step); }
	//--------------------------------------
}
