using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms;
using System;

public class EditorAchievement : IAchievement 
{
	private string mId = "";
	private double mPercentComplete = 0.0f;
	private DateTime mLastReportedDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
	
	
	public void ReportProgress (Action<bool> callback) 
	{
		if (callback != null)
			callback.Invoke(true);
	}
	
	public string id {
		get {
			return mId;
		}
		set {
			mId = value;
		}
	}
	
	public double percentCompleted {
		get {
			return mPercentComplete;
		}
		set {
			mPercentComplete = value;
		}
	}
	
	public bool completed {
		get {
			return false;
		}
	}
	
	public bool hidden {
		get {
			return false;
		}
	}
	
	public DateTime lastReportedDate {
		get {
			// NOTE: we don't implement this field. We always return
			// 1970-01-01 00:00:00
			return mLastReportedDate;
		}
	}
}
