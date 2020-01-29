#if UNITY_IPHONE 
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace IronSource.Editor
{
	public class VungleSettings : IAdapterSettings
	{
		public void updateProject (BuildTarget buildTarget, string projectPath)
		{
			Debug.Log ("IronSource - Update project for Vungle");

			PBXProject project = new PBXProject ();
			project.ReadFromString (File.ReadAllText (projectPath));

			string targetId = project.TargetGuidByName (PBXProject.GetUnityTargetName ());

			// Required System Frameworks
			project.AddFrameworkToProject (targetId, "CoreFoundation.framework", false);
			project.AddFrameworkToProject (targetId, "Foundation.framework", false);
			project.AddFrameworkToProject (targetId, "StoreKit.framework", false);
			
			project.AddFileToBuild (targetId, project.AddFile ("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
			project.AddFileToBuild (targetId, project.AddFile ("usr/lib/libsqlite3.tbd", "Frameworks/libsqlite3.tbd", PBXSourceTree.Sdk));

			// Custom Link Flag
			project.AddBuildProperty (targetId, "OTHER_LDFLAGS", "-ObjC");

			File.WriteAllText (projectPath, project.WriteToString ());
		}

		public void updateProjectPlist (BuildTarget buildTarget, string plistPath)
		{
			Debug.Log ("IronSource - Update plist for Vungle");
		}
	}
}
#endif