using UnityEngine;
using System.Collections;

public class UsersManager : Singleton<UsersManager> 
{
	public UserProfile m_currentUser;
	public static UserProfile currentUser { get { return instance.m_currentUser; } }


	/// <summary>
	/// Initialization.
	/// </summary>
	protected override void OnCreateInstance()
	{
		m_currentUser = new UserProfile();
	}


    public static void Reset()
    {
        instance.m_currentUser = new UserProfile();
    }
}
