public class FunnelData_Load : FunnelData {
	//-- Step index ------------------------
    public enum Steps {
        _01_copa_gpr = 0,
		_02_persistance,
		_03_game_loaded,
		_04_click_play,

		Count
	};
	//--------------------------------------


	//-- Step Names ------------------------
    private const string STEP_01_copa_gpr               = "01.copa_gpr";
    private const string STEP_02_persistance 			= "02.persistance";
    private const string STEP_03_game_loaded 			= "03.game_loaded";
	private const string STEP_04_click_play 			= "04.click_play";
	//--------------------------------------


	//-- Public Methods --------------------
	public FunnelData_Load() : base("custom.game.load.funnel") {
		Setup((int)Steps.Count);

        SetupStep((int)Steps._01_copa_gpr,              STEP_01_copa_gpr);
		SetupStep((int)Steps._02_persistance, 			STEP_02_persistance);
		SetupStep((int)Steps._03_game_loaded, 			STEP_03_game_loaded);
		SetupStep((int)Steps._04_click_play, 			STEP_04_click_play);
		//etc
	}

	public string GetStepName(Steps _step)	 	{ return base.GetStepName((int)_step); }
	public int GetStepDuration(Steps _step)  	{ return base.GetStepDuration((int)_step); }
	public int GetStepTotalTime(Steps _step) 	{ return base.GetStepTotalTime((int)_step); }
	//--------------------------------------
}
