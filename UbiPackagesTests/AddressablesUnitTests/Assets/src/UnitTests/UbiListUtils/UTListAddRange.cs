using System.Collections.Generic;
using UnityEngine;
public class UTListAddRange<T> : UnitTest
{
    public static UnitTestBatch GetUnitTestBatch()
    {
        UnitTestBatch batch = new UnitTestBatch("UTListAddRange");

        UTListAddRange<string> test;
                
        List<string> result1 = new List<string>() { "1", "2", "3" };
        List<string> result1Copy = new List<string>(result1);
        List<string> result2 = new List<string>() { "4", "5", "6" };
        List<string> result3 = new List<string>() { "1", "2", "3" , "4", "5", "6" };
        
        List<string> result2_A = new List<string>() { "1", "2", "4" };
        List<string> result3_A = new List<string>() { "1", "2", "3", "4" };
        List<string> result3_A_with_duplicates = new List<string>() { "1", "2", "3", "1", "2", "4" };
        
        //----------------------------------------------
        // SUCCESS
        //----------------------------------------------                
        
        test = new UTListAddRange<string>(null, null, false, false, null);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, null, false, true, null);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, null, true, false, null);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, null, true, true, null);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(result1, null, true, true, result1);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(result1, null, true, false, result1);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(result1, null, false, true, result1);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(result1, null, false, false, result1);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, result1, true, true, result1);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, result1, true, false, result1);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, result1, false, true, null);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, result1, false, false, null);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2, false, false, result3);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2, false, true, result3);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2, true, false, result3);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2, true, true, result3);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2_A, false, false, result3_A_with_duplicates);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2_A, false, true, result3_A);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2_A, true, false, result3_A_with_duplicates);
        batch.AddTest(test, true);

        result1 = new List<string>(result1Copy);
        test = new UTListAddRange<string>(result1, result2_A, true, true, result3_A);
        batch.AddTest(test, true);

        test = new UTListAddRange<string>(null, result1, true, true, result1Copy);
        batch.AddTest(test, true);

        //----------------------------------------------
        // FAIL
        //----------------------------------------------        

        // Error
        //test = new UnitTestAddRange<string>(result1, null, true, true, result1);      
        //batch.AddTest(test, false);

        // Error
        test = new UTListAddRange<string>(null, null, true, true, result1);
        batch.AddTest(test, false);

        return batch;
    }

    private List<T> m_list1;    
    private List<T> m_list2;
    private List<T> m_list1Orig;
    private bool m_newList;
    private bool m_avoidDuplicates;
    private List<T> m_result;
    
    public UTListAddRange(List<T> l1, List<T> l2, bool newList, bool avoidDuplicates, List<T> result)
    {        
        m_list1 = l1;
        if (m_list1 == null)
        {
            m_list1Orig = null;
        }
        else
        {
            m_list1Orig = new List<T>(m_list1);
        }

        m_list2 = l2;
        m_newList = newList;
        m_avoidDuplicates = avoidDuplicates;
        m_result = result;
    }

    protected override void ExtendedPerform()
    {        
        List<T> resultList = UbiListUtils.AddRange(m_list1, m_list2, m_newList, m_avoidDuplicates);
        bool success = CompareLists(resultList, m_result);
        if (success)
        {
            if (m_newList)
            {
                // Checks that the original list1 wasn't changed
                success = CompareLists(m_list1, m_list1Orig);                
            }
            else
            {
                // Checks that the original list1 and result list have the same elements
                success = CompareLists(m_list1, resultList);
            }
        }

        NotifyPasses(success);
    }

    private bool CompareLists(List<T> l1, List<T> l2)
    {
        bool returnValue = l1 == l2;
        if (!returnValue)
        {
            if (l1 != null && l2 != null)
            {
                returnValue = l1.Count == l2.Count;
                if (returnValue)
                {
                    int count = l1.Count;                    
                    for (int i = 0; i < count; i++)
                    {
                        returnValue = l1[i].Equals(l2[i]);
                    }
                }
            }
        }

        return returnValue;
    }
}
