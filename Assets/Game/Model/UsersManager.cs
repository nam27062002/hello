using UnityEngine;
using System.Collections;

public class UsersManager : Singleton<UsersManager> 
{
	public UserProfile m_currentUser = new UserProfile();
	public static UserProfile currentUser { get { return instance.m_currentUser; }}
}
