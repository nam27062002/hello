/// <summary>
/// This class is responsible for simulating different cases for debug purposes
/// </summary>
using System.Collections.Generic;
public class PersistenceManagerDebug : PersistenceManagerImp
{    
    public PersistenceManagerDebug(string name)
    {
        Name = name;        
    }

    public string Name { get; set; }    
    
    public Queue<PersistenceStates.LoadState> ForcedLoadStates { get; set; }        
    public Queue<PersistenceStates.SaveState> ForcedSaveStates { get; set; }                

    public override PersistenceData LocalProgress_Load(string id)
    {
        PersistenceData returnValue = base.LocalProgress_Load(id);        
        
        if (ForcedLoadStates != null && ForcedLoadStates.Count > 0)
        {            
            LocalProgress_Data.LoadState = ForcedLoadStates.Dequeue();
        }        
        
        return returnValue;
    }

    public override PersistenceStates.SaveState LocalProgress_SaveToDisk()
    {
        PersistenceStates.SaveState returnValue;
        if (ForcedSaveStates != null && ForcedSaveStates.Count > 0)
        {
            returnValue = ForcedSaveStates.Dequeue();
        }
        else
        {
            returnValue = base.LocalProgress_SaveToDisk();
        }

        return returnValue;
    }   
}
