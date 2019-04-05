using Downloadables;
using UnityEngine;

public class UTDownloadablesDisk : UnitTest
{    
    public static string DISK_FILE_NAME = "file";

    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTDownloadablesDisk");
        MockDiskDriver diskDriver = new MockDiskDriver(null);
        diskDriver.SetExceptionTypeToThrow(MockDiskDriver.EExceptionType.UnauthorizedAccess);        

        UTDownloadablesDisk test;
        
        //
        // SUCCESS
        //        
        // PURPOSE: An issue happens every second and the notification is sent every second too        
        test = new UTDownloadablesDisk();
        test.Setup(diskDriver, 0, 10, 1, 10, 0, 0);
        batch.AddTest(test, true);        

        // PURPOSE: An issue happens every second and the notification is sent every two second
        test = new UTDownloadablesDisk();
        test.Setup(diskDriver, 0, 10, 2, 5, 0, 0);
        batch.AddTest(test, true);
        
        // PURPOSE: two different issues happen every second and a notification per issue is sent every second
        test = new UTDownloadablesDisk();
        test.Setup(diskDriver, 1, 10, 1, 10, 10, 0);
        batch.AddTest(test, true);

        //
        // FAIL
        //

        return batch;
    }

    private Disk m_disk;
    private MockDiskDriver m_diskDriver;
    private int m_unauthorizedAccessIssueCount;
    private int m_outOfSpaceIssueCount;
    private int m_otherIssueCount;

    private int m_resultUnauthorizedAccessIssueCount;
    private int m_resultOutOfSpaceIssueCount;
    private int m_resultOtherIssueCount;

    private int m_type;

    private float m_duration;
    private float m_latestOpAt;
    private int m_opCount;

    public void Setup(MockDiskDriver diskDriver, int type, int duration, int issueNotifPeriod, int resultUnauthorizedAccessIssueCount, int resultOutOfSpaceIssueCount, int resultOtherIssueCount)
    {
        m_diskDriver = diskDriver;
        m_disk = new Disk(diskDriver, "manifests", "downloads", "groups", issueNotifPeriod, OnDiskIssue);      

        m_type = type;
        m_duration = duration;
        m_resultUnauthorizedAccessIssueCount = resultUnauthorizedAccessIssueCount;
        m_resultOutOfSpaceIssueCount = resultOutOfSpaceIssueCount;
        m_resultOtherIssueCount = resultOtherIssueCount;
    }

    protected override void ExtendedPerform()
    {
        m_unauthorizedAccessIssueCount = 0;
        m_outOfSpaceIssueCount = 0;
        m_otherIssueCount = 0;

        m_opCount = 0;
        m_latestOpAt = Time.realtimeSinceStartup;
        PerformOp();
    }

    private void OnDiskIssue(Error.EType type)
    {
        switch (type)
        {
            case Error.EType.Disk_IOException:
                m_outOfSpaceIssueCount++;
                break;

            case Error.EType.Disk_UnauthorizedAccess:
                m_unauthorizedAccessIssueCount++;
                break;

            default:
                m_otherIssueCount++;
                break;
        }
    }

    private void PerformOp()
    {        
        m_opCount++;

        Error error;
        switch (m_type)
        {
            case 0:
                m_disk.File_Exists(Disk.EDirectoryId.Manifests, DISK_FILE_NAME, out error);
                break;

            case 1:
                m_diskDriver.SetExceptionTypeToThrow(MockDiskDriver.EExceptionType.UnauthorizedAccess);
                m_disk.File_Exists(Disk.EDirectoryId.Manifests, DISK_FILE_NAME, out error);

                m_diskDriver.SetExceptionTypeToThrow(MockDiskDriver.EExceptionType.IOException);
                m_disk.File_WriteAllText(Disk.EDirectoryId.Manifests, DISK_FILE_NAME, "test", out error);
                break;
        }
    }

    public override void Update()
    {
        if (HasStarted())
        {
            m_disk.Update();

            float diff = Time.realtimeSinceStartup - m_latestOpAt;
            if (diff >= 1f)
            {
                m_latestOpAt = Time.realtimeSinceStartup - (diff - 1f);
                PerformOp();
            }

            if (Time.realtimeSinceStartup - m_timeStartAt >= m_duration)
            {
                bool passes = m_resultOutOfSpaceIssueCount == m_outOfSpaceIssueCount &&
                              m_resultUnauthorizedAccessIssueCount == m_unauthorizedAccessIssueCount &&
                              m_otherIssueCount == m_resultOtherIssueCount;

                NotifyPasses(passes);

            }
        }
    }
}