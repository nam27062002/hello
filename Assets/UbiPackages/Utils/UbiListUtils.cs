using System.Collections.Generic;

public class UbiListUtils
{
    /// <summary>
    /// Compares the elements of two lists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="l1"></param>
    /// <param name="l2"></param>
    /// <returns>Returns <c>true</c> if both lists contain the same elements (elements that are equal, not the same instances)</returns>
    public static bool Compare<T>(List<T> l1, List<T> l2)
    {        
        int count1 = (l1 == null) ? 0 : l1.Count;
        int count2 = (l2 == null) ? 0 : l2.Count;
        bool returnValue = count1 == count2;
        if (returnValue && l1 != null && l2 != null)
        {                        
            for (int i = 0; i < count1; i++)
            {
                returnValue = l1[i].Equals(l2[i]);
            }            
        }       

        return returnValue;
    }

    /// <summary>
    /// Split <c>list1</c> in two lists:
    /// 1)<c>intersection</c> which contains the elements of <c>list1</c> that are in both <c>list1</c> and <c>list2</c>
    /// 2)<c>disjoint</c> which contains the elements of <c>list1</c> that are not in <c>list2</c>
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    /// <param name="intersection">List containing the elements of <c>list1</c> that are in both <c>list1</c> and <c>list2</c></param>
    /// <param name="disjoint">List containing the elements of <c>list1</c> that are not in <c>list2</c></param>
    public static void SplitIntersectionAndDisjoint<T>(List<T> list1, List<T> list2, out List<T> intersection, out List<T> disjoint)
    {
        if (list1 == null)
        {
            intersection = null;
            disjoint = null;
        }
        else if (list2 == null)
        {
            intersection = null;
            disjoint = list1;
        }
        else
        {
            intersection = new List<T>();
            disjoint = new List<T>();

            int count1 = list1.Count;
            int count2 = list2.Count;
            int j;
            T element;
            for (int i = 0; i < count1; i++)
            {
                element = list1[i];
                for (j = 0; j < count2; j++)
                {
                    if (element.Equals(list2[j]) && !intersection.Contains(element))
                    {
                        intersection.Add(element);
                        break;
                    }
                }

                if (j == count2 && !disjoint.Contains(element))
                {
                    disjoint.Add(element);
                }
            }
        }
    }

    /// <summary>
    /// Joins two lists
    /// </summary>
    /// <typeparam name="T">Type of the lists</typeparam>
    /// <param name="l1">First list to join</param>
    /// <param name="l2">Second list to join</param>
    /// <param name="newList">when <c>true</c> a new list is created to store the result. When <c>false</c> <c>l1</c> is used to store the result</param>
    /// <param name="avoidDuplicates">When <c>true</c> all elements in the list returned are different. When <c>false</c> all elements of both lists are stored</param>
    /// <returns></returns>
    public static List<T> AddRange<T>(List<T> l1, List<T> l2, bool newList, bool avoidDuplicates)
    {
        List<T> returnValue = null;

        if (newList)
        {
            if (l1 != null || l2 != null)
            {
                returnValue = new List<T>();
            }
        }
        else
        {
            returnValue = l1;
        }

        if (returnValue != null)
        {
            if (newList && l1 != null)
            {
                returnValue.AddRange(l1);
            }

            if (l2 != null)
            {
                int count = l2.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!avoidDuplicates ||
                        (avoidDuplicates && (l1 == null || !l1.Contains(l2[i]))))
                    {
                        returnValue.Add(l2[i]);
                    }
                }
            }
        }

        return returnValue;
    }

    /*
    public void ClearDuplicates<T>(List<T> list)
    {
        if (list != null)
        {         
            int count = list.Count;
            for (int i = count - 1; i > 0; i--)
            {
                for (int j = i - 1; j > -1; j--)
                {
                    if (list[i].Equals(list[j]))
                    {
                        list.RemoveAt(i);
                    }
                } 
            }
        }        
    }
    */
}
