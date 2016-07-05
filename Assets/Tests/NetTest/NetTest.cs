using UnityEngine;
using System.Collections;

public class NetTest : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void Login()
	{
		GameServerManager.SharedInstance.LoginToServer();
	}

	public void TestAction()
	{
		GameServerManager.SharedInstance.SendTestAction();
	}
}
