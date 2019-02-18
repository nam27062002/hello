using System.Collections;
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

    private enum EPerformStep
    {
        None,
        PerformingSuccess,
        PerformingFail,
        Done
    };

    private EPerformStep m_performStep = EPerformStep.None;

    public UnitTestBatch(string name)
    {
        Name = name;
    }

    public void AddTest(UnitTest test, bool successExpected)
    {
        List<UnitTest> list = (successExpected) ? m_successTest : m_failTest;
        list.Add(test);
    }

    public void AddBatch(UnitTestBatch batch)
    {
        for (int i = 0; i < batch.m_successTest.Count; i++)
        {
            AddTest(batch.m_successTest[i], true);
        }

        for (int i = 0; i < batch.m_failTest.Count; i++)
        {
            AddTest(batch.m_failTest[i], false);
        }
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
   
    private void SetPerformStep(EPerformStep step)
    {
        m_performStep = step;

        switch(m_performStep)
        {
            case EPerformStep.PerformingSuccess:
                PrintSuccessHeader();
                if (m_successTest.Count > 0)
                {
                    m_successTest[0].Perform();
                }
                break;

            case EPerformStep.PerformingFail:
                PrintFailHeader();
                if (m_failTest.Count > 0)
                {
                    m_failTest[0].Perform();
                }
                break;
        }
    }

    public void PerformAllTests()
    {        
        SetPerformStep(EPerformStep.PerformingSuccess);
    }        
   
    private bool IsStepDone(List<UnitTest> list)
    {        
        if (list.Count > 0)
        {
            list[0].Update();
            if (list[0].IsDone())
            {
                list.RemoveAt(0);
                if (list.Count > 0)
                {
                    list[0].Perform();
                }
            }
        }

        return (list.Count == 0);
    }

    public bool Update()
    {
        bool isStepDone;

        switch (m_performStep)
        {
            case EPerformStep.PerformingSuccess:
                isStepDone = IsStepDone(m_successTest);
                if (isStepDone)
                {
                    SetPerformStep(EPerformStep.PerformingFail);
                }                   
                break;

            case EPerformStep.PerformingFail:
                isStepDone = IsStepDone(m_failTest);
                if (isStepDone)
                {
                    SetPerformStep(EPerformStep.Done);
                }
                break;
        }

        return m_performStep == EPerformStep.Done;
    }   
}
