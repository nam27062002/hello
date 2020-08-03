public class FunnelData_LoadRazolytics : FunnelData {
	//-- Step index ------------------------
	public enum Steps {
        _00_start = 0,                  // Sent when managers have been created
		_01_persistance,                // Sent right after the persistence is loaded
        _01_01_persistance_applied,     // Sent right after objects on game side have processed the persistence
        _01_02_persistance_ready,       // Sent when the persistence flow is complete done 
        _01_03_loading_done,            // Sent when the loading process is completed and right before main menu is loaded
		_02_game_loaded,		        // Sent when the main menu is loaded
		Count
	};
    //--------------------------------------


    //-- Step Names ------------------------
    private const string STEP_00_start                  = "00.start";
    private const string STEP_01_persistance 			= "01.persistance";
    private const string STEP_01_01_persistance_applied = "01.01.persistance_applied";
    private const string STEP_01_02_persistance_ready   = "01.02.persistance_ready";
    private const string STEP_01_03_loading_done        = "01.03.loading_done";
    private const string STEP_02_game_loaded 			= "02.game_loaded";	
	//--------------------------------------


	//-- Public Methods --------------------
	public FunnelData_LoadRazolytics() : base("razolytics.custom.game.load.funnel") {
		Setup((int)Steps.Count);

        SetupStep((int)Steps._00_start,                     STEP_00_start);
        SetupStep((int)Steps._01_persistance, 			    STEP_01_persistance);
        SetupStep((int)Steps._01_01_persistance_applied,    STEP_01_01_persistance_applied);
        SetupStep((int)Steps._01_02_persistance_ready,      STEP_01_02_persistance_ready);        
        SetupStep((int)Steps._01_03_loading_done,           STEP_01_03_loading_done);
        SetupStep((int)Steps._02_game_loaded, 			    STEP_02_game_loaded);				
		//etc
	}

	public string GetStepName(Steps _step)	 	{ return base.GetStepName((int)_step); }
	public int GetStepDuration(Steps _step)  	{ return base.GetStepDuration((int)_step); }
	public int GetStepTotalTime(Steps _step) 	{ return base.GetStepTotalTime((int)_step); }
	//--------------------------------------
}
