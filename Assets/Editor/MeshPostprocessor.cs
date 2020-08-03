using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshPostprocessor : AssetPostprocessor {

	
    void OnPreprocessModel () {
		ModelImporter importer = (assetImporter as ModelImporter);

		// We only want to modify import settings the first time
		string name = importer.assetPath.ToLower();
		if (File.Exists(AssetDatabase.GetTextMetaFilePathFromAssetPath(name)))
            return;

		importer.animationCompression = ModelImporterAnimationCompression.Optimal;
		importer.importMaterials = false;

        importer.importLights = false;
        importer.importCameras = false;
        importer.isReadable = false;    // Disble Read/Write to avoid multiple instances on memory

        importer.meshCompression = ModelImporterMeshCompression.High;
    }
    
    public Material OnAssignMaterialModel(Material material, Renderer renderer)
    {
        var materialPath = "Assets/Art/DefaultMaterial.mat";
        material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        return material;
    }
}
