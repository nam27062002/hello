using System;
using System.Collections;
using System.Collections.Generic;
// using FGOL.Utilities;

using UnityEngine;
using System.IO;

public class OptimizationDataInfo:Singleton<OptimizationDataInfo>
{
    private string[] levelToLoad = {
        "Hawaii",
        "Arctic",
        "MiddleEast"
    };
    private string[] SpawnerLevelToLoad = {
        "SP_Hawaii",
        "SP_Arctic",
        "SP_MiddleEast"
    };
    public const string AssetFolderName = "Assets";
    public const string AssetFolderPath = AssetFolderName;
    public const string PrefabFolderName = "Prefabs";
    public const string ResourcesFolderName = "Resources";
    public const string DirectorySeparator = "/";
    public const String ResourceFolderPath = AssetFolderPath + DirectorySeparator + ResourcesFolderName;
    public const String MaterialFolderName = "Material";
    public const String ArtFolderName = "Art";
    public const String SceneMaterialSVFolderName = "SceneMaterialSV";
    public const String PrefabsMaterialSVFolderSVName = "PrefabsMaterialSV";
    public const String MaterialResourceSaveFolderPath = AssetFolderPath + DirectorySeparator + ResourcesFolderName + DirectorySeparator + MaterialFolderName;
    public const string MaterialSceneSavePath = AssetFolderPath + DirectorySeparator + ArtFolderName + DirectorySeparator + SceneMaterialSVFolderName;
    public const string MaterialPrefabsSavePath = AssetFolderPath + DirectorySeparator + ArtFolderName + DirectorySeparator + PrefabsMaterialSVFolderSVName;
    public readonly string[] PropertiesNameTofind = {"_NM","_BumpMap","_NormalMap"};
    public const String HDSufix = "HD";
    public const String MDSufix = "MD";
    public const String LDSufix = "LD";
    public const String MetaFileExtension = ".meta";
    public const String MaterialFileExtension = ".mat";
    public const String PrefabsFolderPath = AssetFolderPath + DirectorySeparator + PrefabFolderName;
    public const String MultiSufixFolferName = "MRes";
    public const String PoolsFolderName = "Pools";
    public const String ResourcePoolsFolderPath = PrefabsFolderPath + DirectorySeparator + ResourcesFolderName + DirectorySeparator + PoolsFolderName;
    public const String LightMaplFolderName = "LightMap";
    public const String LightmapResourceSaveFolderPath = AssetFolderPath + DirectorySeparator + ResourcesFolderName + DirectorySeparator + LightMaplFolderName;


	public const string ArtScenePath = "Game/Scenes/Levels/Art/";
	public const string SpawnersScenePath = "Game/Scenes/Levels/Spawners/";


	public OptimizationDataInfo()
	{
		
	}

    public enum QualityLevelsType
    {
        Low=0,
        Medium=1,
        High=2
    }

	public string[] GetArtScenes()
    {
		string scenesPath = Application.dataPath + "/" + ArtScenePath;
		string[] files = Directory.GetFiles(scenesPath, "*.unity");
		int toRemovePath = scenesPath.Length;
		int sufixSize = ".unity".Length;
		for( int i = 0; i<files.Length; i++ )
		{
			files[i] = files[i].Substring(toRemovePath);
			files[i] = files[i].Substring(0, files[i].Length-sufixSize);
		}
		return files;

		// Alternative version
		// return levelToLoad;
    }

    public string[] GetSpawnerScenes()
    {
		string scenesPath = Application.dataPath + "/" + SpawnersScenePath;
		string[] files = Directory.GetFiles(scenesPath, "*.unity");
		int toRemovePath = scenesPath.Length;
		int sufixSize = ".unity".Length;
		for( int i = 0; i<files.Length; i++ )
		{
			files[i] = files[i].Substring(toRemovePath);
			files[i] = files[i].Substring(0, files[i].Length-sufixSize);
		}
    	return files;

    	// Alternative version
		// return SpawnerLevelToLoad;
    }

    public string GetSufix(QualityLevelsType type)
    {
        switch (type)
        {
            case QualityLevelsType.High:
                return HDSufix;
            case QualityLevelsType.Medium:
                return MDSufix;
            case QualityLevelsType.Low:
                return LDSufix;
        }

        return HDSufix;
    }

    public  bool  IsASufix(string suffix)
    {
        foreach (QualityLevelsType qualityLevelsType in Enum.GetValues(typeof(QualityLevelsType)))
        {
            string curSuffix = GetSufix(qualityLevelsType);
            if (curSuffix.CompareTo(suffix) == 0)
                return true;
        }

        return false;
    }
}

