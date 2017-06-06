using UnityEngine;
using UnityEditor;

public class MeshPostprocessor : AssetPostprocessor {

	
    void OnPreprocessModel () {
		ModelImporter importer = (assetImporter as ModelImporter);
		importer.animationCompression = ModelImporterAnimationCompression.Optimal;
    }
}
