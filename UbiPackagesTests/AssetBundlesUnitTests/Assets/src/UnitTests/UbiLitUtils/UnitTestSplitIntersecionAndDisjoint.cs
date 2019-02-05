using System.Collections.Generic;

public class UnitTestSplitIntersecionAndDisjoint<T> : UnitTest
{
    public static void TestBatch()
    {
        UnitTestSplitIntersecionAndDisjoint<string> test;

        List<string> A_l1 = new List<string>() { "1", "2", "3" };        
        List<string> A_l2 = new List<string>() { "4", "5", "6" };

        List<string> B_l2 = new List<string>() { "2", "5", "6" };

        List<string> C_l1 = new List<string>() { "2" };
        List<string> C_l2 = new List<string>() { "2" };
        
        // Error
        test = new UnitTestSplitIntersecionAndDisjoint<string>(null, A_l2, null, A_l2);
        test.Perform();

        // Success
        test = new UnitTestSplitIntersecionAndDisjoint<string>(null, null, null, null);
        test.Perform();

        // Success
        test = new UnitTestSplitIntersecionAndDisjoint<string>(null, A_l2, null, null);
        test.Perform();

        // Success
        test = new UnitTestSplitIntersecionAndDisjoint<string>(A_l1, null, null, A_l1);
        test.Perform();        

        // Success
        test = new UnitTestSplitIntersecionAndDisjoint<string>(A_l1, A_l2, null, A_l1);
        test.Perform();        

        // Success
        test = new UnitTestSplitIntersecionAndDisjoint<string>(A_l1, B_l2, new List<string> { "2" }, new List<string> { "1", "3" });
        test.Perform();        

        // Success
        test = new UnitTestSplitIntersecionAndDisjoint<string>(C_l1, C_l2, C_l1, new List<string>());
        test.Perform();
    }

    private List<T> m_list1;
    private List<T> m_list2;
    private List<T> m_resultIntersection;
    private List<T> m_resultDisjoint;

    public UnitTestSplitIntersecionAndDisjoint(List<T> list1, List<T> list2, List<T> resultIntersection, List<T> resultDisjoint)
    {
        m_list1 = list1;
        m_list2 = list2;
        m_resultIntersection = resultIntersection;
        m_resultDisjoint = resultDisjoint;
    }

    protected override void ExtendedPerform()
    {
        List<T> intersection;
        List<T> disjoint;
        UbiListUtils.SplitIntersectionAndDisjoint(m_list1, m_list2, out intersection, out disjoint);
        bool success = UbiListUtils.Compare(intersection, m_resultIntersection);
        if (success)
        {
            success = UbiListUtils.Compare(disjoint, m_resultDisjoint);
        }

        NotifyPasses(success);
    }
}

