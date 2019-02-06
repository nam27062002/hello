using System.Collections.Generic;
public class UnitTestAddRange<T> : UnitTest
{
    public static void TestBatch()
    {
        UnitTestAddRange<string> test;
                
        List<string> result1 = new List<string>() { "1", "2", "3" };
        List<string> result1Copy = new List<string>(result1);
        List<string> result2 = new List<string>() { "4", "5", "6" };
        List<string> result3 = new List<string>() { "1", "2", "3" , "4", "5", "6" };
        
        List<string> result2_A = new List<string>() { "1", "2", "4" };
        List<string> result3_A = new List<string>() { "1", "2", "3", "4" };
        List<string> result3_A_with_duplicates = new List<string>() { "1", "2", "3", "1", "2", "4" };
        // Error
        //test = new UnitTestAddRange<string>(result1, null, true, true, result1);
        //test.Perform();
        
        // Error
        test = new UnitTestAddRange<string>(null, null, true, true, result1);
        test.Perform();

        // Error
        test = new UnitTestAddRange<string>(null, result1, true, true, result1Copy);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(null, null, false, false, null);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(null, null, false, true, null);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(null, null, true, false, null);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(null, null, true, true, null);
        test.Perform();                

        // Success
        test = new UnitTestAddRange<string>(result1, null, true, true, result1);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(result1, null, true, false, result1);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(result1, null, false, true, result1);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(result1, null, false, false, result1);
        test.Perform();           
        
        // Success
        test = new UnitTestAddRange<string>(null, result1, true, true, result1);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(null, result1, true, false, result1);
        test.Perform();
        
        // Success
        test = new UnitTestAddRange<string>(null, result1, false, true, null);
        test.Perform();

        // Success
        test = new UnitTestAddRange<string>(null, result1, false, false, null);
        test.Perform();

        // Success    
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2, false, false, result3);
        test.Perform();

        // Success   
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2, false, true, result3);
        test.Perform();

        // Success      
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2, true, false, result3);
        test.Perform();

        // Success       
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2, true, true, result3);
        test.Perform();        

        // Success    
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2_A, false, false, result3_A_with_duplicates);
        test.Perform();

        // Success   
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2_A, false, true, result3_A);
        test.Perform();

        // Success      
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2_A, true, false, result3_A_with_duplicates);
        test.Perform();

        // Success       
        result1 = new List<string>(result1Copy);
        test = new UnitTestAddRange<string>(result1, result2_A, true, true, result3_A);
        test.Perform();
    }  

    private List<T> m_list1;    
    private List<T> m_list2;
    private List<T> m_list1Orig;
    private bool m_newList;
    private bool m_avoidDuplicates;
    private List<T> m_result;
    
    public UnitTestAddRange(List<T> l1, List<T> l2, bool newList, bool avoidDuplicates, List<T> result)
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
