using System.Collections.Generic;
using UnityEngine;
public class UnitTestBatch
{
    public string Name { get; private set; }

    /// <summary>
    /// List of unit test that are suppossed to succeed
    /// </summary>
    private List<UnitTest> m_successTest = new List<UnitTest>();

    /// <summary>
    /// List of unit test that are suppossed to fail
    /// </summary>
    private List<UnitTest> m_failTest = new List<UnitTest>();

    public UnitTestBatch(string name)
    {
        Name = name;
    }

    public void AddTest(UnitTest test, bool successExpected)
    {
        List<UnitTest> list = (successExpected) ? m_successTest : m_failTest;
        list.Add(test);
    }

    public static void PrintSuccessHeader()
    {
        Debug.Log("SUCCESS TESTS:");
    }

    public static void PrintFailHeader()
    {
        Debug.Log("");
        Debug.Log("FAIL TESTS:");
    }

    public void PerformAllTests()
    {
        PrintSuccessHeader();
        PerformSuccessTests();

        PrintFailHeader();
        PerformFailTests();
    }

    public void PerformSuccessTests()
    {
        int count = m_successTest.Count;
        for (int i = 0; i < count; i++)
        {
            m_successTest[i].Perform();
        }
    }

    public void PerformFailTests()
    {
        int count = m_failTest.Count;
        for (int i = 0; i < count; i++)
        {
            m_failTest[i].Perform();
        }
    }
}
