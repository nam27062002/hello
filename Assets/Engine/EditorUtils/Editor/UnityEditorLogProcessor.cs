using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class UnityEditorLog
{
#if UNITY_EDITOR_OSX
	private static readonly string EDITOR_LOG_PATH = "/Library/Logs/Unity/Editor.log";				//MAC
#elif UNITY_EDITOR_WIN
	private static readonly string EDITOR_LOG_PATH = "\AppData\Local\Unity\Editor\Editor.log";		//WINDOWS
#else
	private static readonly string EDITOR_LOG_PATH = "/.config/unity3d/Editor.log";					//LINUX
#endif

	private static List<string> parseUnityEditorLog(string appVersion, bool assetBundles, string editorLogPath)
	{
		if (string.IsNullOrEmpty(editorLogPath))
		{
			editorLogPath = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal) + EDITOR_LOG_PATH;
		}

		string[] lines = File.ReadAllLines(editorLogPath);
		List<string> outputList = new List<string>();
		bool capturing = false;
		string searchToken = assetBundles ? "Bundle Name" : "Build Report";
		int offset = assetBundles ? 0 : 14;    //14 lines until asset list

		for (int c = 0; c < lines.Length; c++)
		{
			string line = lines[c];

			if (capturing)
			{
				outputList.Add(line);
				if (assetBundles)
				{
					if (line.Contains("--------------"))
					{
						string line1 = lines[c + 1];
						if (!(line1.Contains("--------------")))
						{
							capturing = false;
						}
						else
						{
							outputList.Add(line);
							c++;
						}
					}
				}
				else
				{
					if (line.Contains("--------------"))
					{
						capturing = false;
					}
				}
			}
			else
			{
				if (line.Contains("--------------") && lines[c + 1].Contains(searchToken))
				{
					outputList.Clear();
					outputList.Add(line);

					string buildVersion = "Build version: " + appVersion;

					outputList.Add(buildVersion);
					outputList.Add(line);

					capturing = true;

					c += offset;

				}
			}

		}

		if (capturing)
		{
			outputList.Add(">>>>>>>> Incomplete asset list.");
		}

		return outputList;
	}

	//Get Editor.log asset list
	public static void exportAssetList(string appVersion, bool assetBundles = false, string editorLogPath = "" )
	{

		string assetsPath = Application.dataPath;
#if UNITY_EDITOR_WIN
		string projectPath = assetsPath.Substring(0, assetsPath.LastIndexOf("\Assets"));

		string ASSET_LIST;

		if (assetBundles)
		{
			ASSET_LIST = projectPath + "\BuildAssetbundlesList.txt";
		}
		else
		{
			ASSET_LIST = projectPath + "\BuildAssetsList.txt";
		}
#else
		string projectPath = assetsPath.Substring(0, assetsPath.LastIndexOf("/Assets"));

		string ASSET_LIST;

		if (assetBundles)
		{
			ASSET_LIST = projectPath + "/BuildAssetbundlesList.txt";
		}
		else
		{
			ASSET_LIST = projectPath + "/BuildAssetsList.txt";
		}
#endif

		List<string> outputList = parseUnityEditorLog(appVersion, assetBundles, editorLogPath);

		if (outputList.Count > 0)
		{
			if (File.Exists(ASSET_LIST))
			{
				FileUtil.DeleteFileOrDirectory(ASSET_LIST);
			}
			File.WriteAllLines(ASSET_LIST, outputList.ToArray());
			Debug.Log("UnityEditorLog.exportAssetList: " + ASSET_LIST + " created ok.");
		}
		else
		{
			Debug.Log("UnityEditorLog.exportAssetList: No data found in editor.log");
		}
	}


	[MenuItem("Tech/Build/Export build asset list")]
	public static void callExportAssetListBuild()
	{
		exportAssetList(Application.version);
	}

	[MenuItem("Tech/Build/Export asset bundles detailed list")]
	public static void callExportAssetListBundles()
	{
		exportAssetList(Application.version, true);
	}

}
