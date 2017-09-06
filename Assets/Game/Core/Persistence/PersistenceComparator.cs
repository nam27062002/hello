public abstract class PersistenceComparator
{
    public abstract PersistenceStates.ConflictState CompareSaves(PersistenceData local, PersistenceData cloud);
    public abstract void ReconcileData(PersistenceData local, PersistenceData cloud);
    public abstract object GetLocalProgress();
    public abstract object GetCloudProgress();
}