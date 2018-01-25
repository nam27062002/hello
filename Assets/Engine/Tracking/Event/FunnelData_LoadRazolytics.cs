public class FunnelData_LoadRazolytics : FunnelData {
	//-- Step index ------------------------
	public enum Steps {
        _00_start = 0,
		_01_persistance,
		_02_game_loaded,		
		Count
	};
    //--------------------------------------


    //-- Step Names ------------------------
    private const string STEP_00_start                  = "00.start";
    private const string STEP_01_persistance 			= "01.persistance";
	private const string STEP_02_game_loaded 			= "02.game_loaded";	
	//--------------------------------------


	//-- Public Methods --------------------
	public FunnelData_LoadRazolytics() : base("razolytics.custom.game.load.funnel") {
		Setup((int)Steps.Count);

        SetupStep((int)Steps._00_start,                 STEP_00_start);
        SetupStep((int)Steps._01_persistance, 			STEP_01_persistance);
		SetupStep((int)Steps._02_game_loaded, 			STEP_02_game_loaded);				
		//etc
	}

	public string GetStepName(Steps _step)	 	{ return base.GetStepName((int)_step); }
	public int GetStepDuration(Steps _step)  	{ return base.GetStepDuration((int)_step); }
	public int GetStepTotalTime(Steps _step) 	{ return base.GetStepTotalTime((int)_step); }
	//--------------------------------------
}
