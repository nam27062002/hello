public abstract class PersistenceComparator
{
    public abstract void Reset();
    public abstract PersistenceStates.EConflictState Compare(PersistenceData local, PersistenceData cloud);
    public abstract void ReconcileData(PersistenceData local, PersistenceData cloud);
    public abstract object GetLocalProgress();
    public abstract object GetCloudProgress();
}