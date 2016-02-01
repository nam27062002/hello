using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using System;

public class EditorLocalUser : ILocalUser 
{
    private const bool SIMULATE_AUTH = false;

	private string mUserName = "YouNameHere";
	private string mId = "1560";
    private bool mAuthenticated = false;

    private IUserProfile[] mFriends;

    public EditorLocalUser()
	{
        Setup(System.Environment.UserName, System.Environment.UserName);		
	}

    public void Setup(string userName, string id)
    {
        mUserName = userName;
        mId = id;
    }

	public void Authenticate(Action<bool> callback) 
	{
        if (SIMULATE_AUTH)
        {
            mAuthenticated = true;
        }

        if (callback != null)
        {
            callback.Invoke(authenticated);
        }
	}

    public void Unauthenticate()
    {
        mAuthenticated = false;
    }
	
	public void LoadFriends (Action<bool> callback) 
	{
		if (callback != null) {
			callback.Invoke(false);
		}
	}
	
	public IUserProfile[] friends 
	{
		get 
		{
            if (!authenticated)
            {
                return new IUserProfile[0];
            }
            else
            {
                if (mFriends == null)
                {
                    int count = 3;
                    mFriends = new IUserProfile[count];

                    int i;                    
                    for (i = 0; i < count; i++)
                    {
                        SetupFriend(i);
                    }                                        
                }

                return mFriends;
            }
		}
	}

    private void SetupFriend(int i)
    {
        if (mFriends != null && i > -1 && i < mFriends.Length)
        {            
            if (mFriends[i] == null)
            {
                mFriends[i] = new EditorLocalUser();
            }

            EditorLocalUser user = mFriends[i] as EditorLocalUser;
            if (user != null)
            {
                user.Authenticate(null);
                user.Setup("Friend_" + i, 1000 + i + "");
            }
        }                
    }
	
	public bool authenticated 
	{
		get 
		{
			return mAuthenticated;
		}
	}
	
	public bool underage 
	{
		get 
		{
			return true;
		}
	}
	
	public string userName {
		get 
		{
			return authenticated ? mUserName : "";
		}
	}
	
	public string id 
	{
		get 
		{
			return authenticated ? mId : "";
		}
	}
	
	public  bool isFriend {
		get {
			return true;
		}
	}
	
	public  UserState state {
		get {
			return UserState.Online;
		}
	}
	
	public UnityEngine.Texture2D image {
		get {
			return null;
		}
	}
}
