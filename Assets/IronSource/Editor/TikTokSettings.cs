#if UNITY_IPHONE 
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace IronSource.Editor
{
	public class TikTokSettings : IAdapterSettings
	{
		public void updateProject (BuildTarget buildTarget, string projectPath)
		{
			Debug.Log ("IronSource - Update project for TikTok");

			PBXProject project = new PBXProject ();
			project.ReadFromString (File.ReadAllText (projectPath));

			string targetId = project.TargetGuidByName (PBXProject.GetUnityTargetName ());

			// Required System Frameworks
			project.AddFrameworkToProject (targetId, "StoreKit.framework", false);
			project.AddFrameworkToProject (targetId, "MobileCoreServices.framework", false);
			project.AddFrameworkToProject (targetId, "WebKit.framework", false);
			project.AddFrameworkToProject (targetId, "MediaPlayer.framework", false);
			project.AddFrameworkToProject (targetId, "CoreMedia.framework", false);
			project.AddFrameworkToProject (targetId, "AVFoundation.framework", false);
			project.AddFrameworkToProject (targetId, "CoreLocation.framework", false);
			project.AddFrameworkToProject (targetId, "CoreTelephony.framework", false);
			project.AddFrameworkToProject (targetId, "SystemConfiguration.framework", false);
			project.AddFrameworkToProject (targetId, "Photos.framework", false);
			project.AddFrameworkToProject (targetId, "AdSupport.framework", false);
			project.AddFrameworkToProject (targetId, "CoreMotion.framework", false);

			project.AddFileToBuild (targetId, project.AddFile ("usr/lib/libresolv.9.tbd", "Frameworks/libresolv.9.tbd", PBXSourceTree.Sdk));
			project.AddFileToBuild (targetId, project.AddFile ("usr/lib/libz.tbd", "Frameworks/libz.tbd", PBXSourceTree.Sdk));
			project.AddFileToBuild (targetId, project.AddFile ("usr/lib/libc++.tbd", "Frameworks/libc++.tbd", PBXSourceTree.Sdk));


			File.WriteAllText (projectPath, project.WriteToString ());
		}

		public void updateProjectPlist (BuildTarget buildTarget, string plistPath)
		{
			Debug.Log ("IronSource - Update plist for TikTok");
		}
	}
}
#endif
