/*
Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.
 
This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
the appropriate menu item from "Custom->Export". Exported models are put in a folder called
"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
same folder.
N.B. there may be a bug so if the custom option doesn't come up refer to this thread http://answers.unity3d.com/questions/317951/how-to-use-editorobjexporter-obj-saving-script-fro.html 
 
Updated for Unity 5.3
*/
 
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
 
struct ObjMaterial
{
	public string name;
	public string diffuseMapName;
    public string normalMapName;
    public string alphaMapName;
    public float specular_pow;
}
 
public class EditorObjExporter : ScriptableObject
{
	private static int vertexOffset = 0;
	private static int normalOffset = 0;
	private static int uvOffset = 0;
 
 
	//User should probably be able to change this. It is currently left as an excercise for
	//the reader.
	private readonly static string targetFolder = "ExportedObj";
    private readonly static string textureFolder = "Textures";

    private static string texturePath;
 
 
	private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList) 
	{
		Mesh m = mf.sharedMesh;

        Renderer rnd = mf.GetComponent<Renderer>();
        Material[] mats = rnd != null ? rnd.sharedMaterials : null;
 
		StringBuilder sb = new StringBuilder();
 
		sb.Append("g ").Append(trimSpaces(mf.name)).Append("\n");
		foreach(Vector3 lv in m.vertices) 
		{
			Vector3 wv = mf.transform.TransformPoint(lv);
 
			//This is sort of ugly - inverting x-component since we're in
			//a different coordinate system than "everyone" is "used to".
			sb.Append(string.Format("v {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}
		sb.Append("\n");
 
		foreach(Vector3 lv in m.normals) 
		{
			Vector3 wv = mf.transform.TransformDirection(lv);
 
			sb.Append(string.Format("vn {0} {1} {2}\n",-wv.x,wv.y,wv.z));
		}
		sb.Append("\n");
 
		foreach(Vector3 v in m.uv) 
		{
			sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
		}

        if (mats != null)
        {
            for (int material = 0; material < m.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append(trimSpaces(mats[material].name)).Append("\n");
                sb.Append("usemap ").Append(trimSpaces(mats[material].name)).Append("\n");

                //See if this material is already in the materiallist.
                try
                {
                    ObjMaterial objMaterial = new ObjMaterial();
                    Material unityMaterial = mats[material];

                    objMaterial.name = trimSpaces(unityMaterial.name);

                    if (unityMaterial.mainTexture)
                        objMaterial.diffuseMapName = AssetDatabase.GetAssetPath(unityMaterial.mainTexture);
                    else
                        objMaterial.diffuseMapName = null;

                    objMaterial.specular_pow = unityMaterial.IsKeywordEnabled("SPECULAR") ? unityMaterial.GetFloat("_SpecularPower") : 0.01f;

                    if (unityMaterial.IsKeywordEnabled("NORMALMAP"))
                    {
                        objMaterial.normalMapName = AssetDatabase.GetAssetPath(unityMaterial.GetTexture("_NormalTex"));
                    }
                    else
                    {
                        objMaterial.normalMapName = null;
                    }

                    if (unityMaterial.GetFloat("_BlendMode") != 0.0f)
                    {
                        objMaterial.alphaMapName = objMaterial.diffuseMapName;
                    }

                    materialList.Add(objMaterial.name, objMaterial);
                }
                catch (ArgumentException)
                {
                    //Already in the dictionary
                }


                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    //Because we inverted the x-component, we also needed to alter the triangle winding.
                    sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                        triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
                }
            }
        }
 
		vertexOffset += m.vertices.Length;
		normalOffset += m.normals.Length;
		uvOffset += m.uv.Length;
 
		return sb.ToString();
	}
 
	private static void Clear()
	{
		vertexOffset = 0;
		normalOffset = 0;
		uvOffset = 0;
	}
 
	private static Dictionary<string, ObjMaterial> PrepareFileWrite()
	{
		Clear();
 
		return new Dictionary<string, ObjMaterial>();
	}
 
	private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
	{
		using (StreamWriter sw = new StreamWriter(folder + Path.DirectorySeparatorChar + filename + ".mtl")) 
		{
			foreach( KeyValuePair<string, ObjMaterial> kvp in materialList )
			{
				sw.Write("\n");
				sw.Write("newmtl {0}\n", kvp.Key);
				sw.Write("Ka  0.6 0.6 0.6\n");
				sw.Write("Kd  0.6 0.6 0.6\n");
//				sw.Write("Ks  0.9 0.9 0.9\n");
                sw.Write(string.Format("Ks {0} {0} {0}\n", kvp.Value.specular_pow > 0.1f ? 0.75f: 0.01f));
                sw.Write(string.Format("Ns {0}\n", kvp.Value.specular_pow));

                if (kvp.Value.alphaMapName != null)
                {
                    sw.Write("Ke  0.0 0.0 0.0\n");
                    sw.Write("Ni  1.0\n");
                    sw.Write("d  0.0\n");
//                    sw.Write("illum 2\n");

                }
                else
                {
                    sw.Write("d  1.0\n");
                    //                    sw.Write("illum 2\n");
                }
                sw.Write("illum 2\n");

                if (kvp.Value.diffuseMapName != null)
				{
					string destinationFile = kvp.Value.diffuseMapName; 
 
					int stripIndex = destinationFile.LastIndexOf(Path.AltDirectorySeparatorChar);
 
					if (stripIndex >= 0)
                    {
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();
                    }
                    else
                    {
                        stripIndex = destinationFile.LastIndexOf('/');
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();
                    }

//                    string relativeFile = textureFolder + '/' + destinationFile;
                    string relativeFile = textureFolder + Path.AltDirectorySeparatorChar + destinationFile;
                    //                    string relativeFile = destinationFile;


                    destinationFile = texturePath + Path.AltDirectorySeparatorChar + destinationFile;
//                    destinationFile = texturePath + '/' + destinationFile;

                    Debug.Log("Copying texture from " + kvp.Value.diffuseMapName + " to " + destinationFile);
 
					try
					{
						//Copy the source file
						File.Copy(kvp.Value.diffuseMapName, destinationFile);
					}
					catch
					{
 
					}

                    sw.Write("map_Ka {0}\n", relativeFile);
                    sw.Write("map_Kd {0}\n", relativeFile);
                    sw.Write("map_Ks {0}\n", relativeFile);

                    if (kvp.Value.alphaMapName != null)
                    {
                        sw.Write("map_d {0}\n", relativeFile);
//                        sw.Write("map_d - mm 0.200 0.800 {0}\n", relativeFile);                        
                    }
                    else
                    {
//                        sw.Write("d  1.0\n");
                    }
                }

                if (kvp.Value.normalMapName != null)
                {
                    string destinationFile = kvp.Value.normalMapName;

                    int stripIndex = destinationFile.LastIndexOf(Path.AltDirectorySeparatorChar);

                    if (stripIndex >= 0)
                    {
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();
                    }
                    else
                    {
                        stripIndex = destinationFile.LastIndexOf('/');
                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();
                    }

                    string relativeFile = textureFolder + Path.AltDirectorySeparatorChar + destinationFile;

                    destinationFile = texturePath + Path.AltDirectorySeparatorChar + destinationFile;

                    Debug.Log("Copying texture from " + kvp.Value.normalMapName + " to " + destinationFile);

                    try
                    {
                        //Copy the source file
                        File.Copy(kvp.Value.normalMapName, destinationFile);
                    }
                    catch
                    {

                    }

                    sw.Write("map_bump {0}\n", relativeFile);

                }
                sw.Write("\n\n\n");
			}
		}
	}
 
	private static void MeshToFile(MeshFilter mf, string folder, string filename) 
	{
		Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
 
		using (StreamWriter sw = new StreamWriter(folder +Path.DirectorySeparatorChar + filename + ".obj")) 
		{
			sw.Write("mtllib ./" + filename + ".mtl\n");
 
			sw.Write(MeshToString(mf, materialList));
		}
 
		MaterialsToFile(materialList, folder, filename);
	}
 
	private static void MeshesToFile(MeshFilter[] mf, string folder, string filename) 
	{
		Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();
 
		using (StreamWriter sw = new StreamWriter(folder +Path.DirectorySeparatorChar + filename + ".obj")) 
		{
			sw.Write("mtllib ./" + filename + ".mtl\n");
 
			for (int i = 0; i < mf.Length; i++)
			{
				sw.Write(MeshToString(mf[i], materialList));
			}
		}
 
		MaterialsToFile(materialList, folder, filename);
	}
 
	private static bool CreateTargetFolder()
	{
		try
		{
			System.IO.Directory.CreateDirectory(targetFolder);
            texturePath = targetFolder + Path.DirectorySeparatorChar + textureFolder;
            System.IO.Directory.CreateDirectory(texturePath);

        }
        catch
		{
			EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
			return false;
		}
 
		return true;
	}

    static private string trimSpaces(string trim)
    {
        string trimed = trim.Replace(' ', '_');
        trimed = trimed.Replace('(', '_');
        trimed = trimed.Replace(')', '_');
        Debug.Log("Trim before: " + trim + " after: " + trimed);
        return trimed;
    }
 
	[MenuItem ("Custom/Export/Export all MeshFilters in selection to separate OBJs")]
	static void ExportSelectionToSeparate()
	{
		if (!CreateTargetFolder())
			return;
 
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
 
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
 
		int exportedObjects = 0;
 
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
 
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				MeshToFile((MeshFilter)meshfilter[m], targetFolder, trimSpaces(selection[i].name) + "_" + i + "_" + m);
			}
		}
 
		if (exportedObjects > 0)
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}
 
	[MenuItem ("Custom/Export/Export whole selection to single OBJ")]
	static void ExportWholeSelectionToSingle()
	{
		if (!CreateTargetFolder())
			return;
 
 
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
 
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
 
		int exportedObjects = 0;
 
		ArrayList mfList = new ArrayList();
 
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
 
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mfList.Add(meshfilter[m]);
			}
		}
 
		if (exportedObjects > 0)
		{
			MeshFilter[] mf = new MeshFilter[mfList.Count];
 
			for (int i = 0; i < mfList.Count; i++)
			{
				mf[i] = (MeshFilter)mfList[i];
			}
 
			string filename = EditorSceneManager.GetActiveScene().name + "_" + exportedObjects;
 
			int stripIndex = filename.LastIndexOf(Path.DirectorySeparatorChar);
 
			if (stripIndex >= 0)
				filename = filename.Substring(stripIndex + 1).Trim();
 
			MeshesToFile(mf, targetFolder, filename);
 
 
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
		}
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}


    [MenuItem("Custom/Export/Export whole collision map to single OBJ")]
    static void ExportCollidersToSingle()
    {
        if (!CreateTargetFolder())
            return;

        PolyMesh[] selection;
        AssetFinder.FindAssetInScene<PolyMesh>(out selection);

        if (selection.Length == 0)
        {
            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
            return;
        }

        int exportedObjects = 0;

        ArrayList mfList = new ArrayList();

        for (int i = 0; i < selection.Length; i++)
        {
            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

            for (int m = 0; m < meshfilter.Length; m++)
            {
                Renderer rnd = meshfilter[m].gameObject.GetComponent<Renderer>();
                if (rnd != null && Mathf.Abs(rnd.transform.position.z) < 0.2f)
                {
                    exportedObjects++;
                    mfList.Add(meshfilter[m]);
                }
            }
        }

        if (exportedObjects > 0)
        {
            MeshFilter[] mf = new MeshFilter[mfList.Count];

            for (int i = 0; i < mfList.Count; i++)
            {
                mf[i] = (MeshFilter)mfList[i];
            }

            string filename = EditorSceneManager.GetActiveScene().name + "_" + exportedObjects;

            int stripIndex = filename.LastIndexOf(Path.DirectorySeparatorChar);

            if (stripIndex >= 0)
                filename = filename.Substring(stripIndex + 1).Trim();

            MeshesToFile(mf, targetFolder, filename);


            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
        }
        else
            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
    }



    [MenuItem ("Custom/Export/Export each selected to single OBJ")]
	static void ExportEachSelectionToSingle()
	{
		if (!CreateTargetFolder())
			return;
 
		Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);
 
		if (selection.Length == 0)
		{
			EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
			return;
		}
 
		int exportedObjects = 0;
 
 
		for (int i = 0; i < selection.Length; i++)
		{
			Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));
 
			MeshFilter[] mf = new MeshFilter[meshfilter.Length];
 
			for (int m = 0; m < meshfilter.Length; m++)
			{
				exportedObjects++;
				mf[m] = (MeshFilter)meshfilter[m];
			}
 
			MeshesToFile(mf, targetFolder, trimSpaces(selection[i].name) + "_" + i);
		}
 
		if (exportedObjects > 0)
		{
			EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
		}
		else
			EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
	}
 
}