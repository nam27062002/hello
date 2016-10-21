using FGOL.Save;
using FGOL.Save.SaveStates;
using System.Collections.Generic;

namespace FGOL.Save
{
    public abstract class SaveGameComparator
    {
        public abstract ConflictState CompareSaves(SaveData local, SaveData cloud);
        public abstract void ReconcileData(SaveData local, SaveData cloud);
        public abstract object GetLocalProgress();
        public abstract object GetCloudProgress();
    }
}