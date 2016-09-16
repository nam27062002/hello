// MonoBehaviourTemplateEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;

public enum TexFormats
{
    GRAYSCALE = 0,
    RGB = 1,
    RGBA = 2
}

public abstract class TextureGenBase : ScriptableObject
{
    protected Vector2 iResolution;

    public string serializedName;

    abstract public void initGen(Texture2D canvas);
    abstract public Vector4 doGen(Vector2 iFragCoord);
    abstract public void guiGen();
}

public class Perlin : TextureGenBase
{
    public int detail = 4;
    public float scale = 8.0f;
    public float seed = 100.0f;
    public bool tileable = true;

    private Vector2 internalSeed = new Vector2(0.0f, 157.0f);
    private Vector2 internalSeed2 = new Vector2(1.0f, 157.0f);



    //
    public Perlin()
    {
        serializedName = "PerlinPrefs";
    }

    public override void initGen(Texture2D canvas)
    {
        iResolution.x = canvas.width;
        iResolution.y = canvas.height;

        internalSeed = new Vector2(0.0f, seed / iResolution.x);
        internalSeed2 = new Vector2(1.0f, seed / iResolution.x);
    }

    Vector2 Sin(Vector2 v)
    {
        return new Vector2(Mathf.Sin(v.x), Mathf.Sin(v.y));
    }

    Vector2 Fract(Vector2 v)
    {
        return new Vector2(v.x - Mathf.Floor(v.x), v.y - Mathf.Floor(v.y));
    }

    public Vector2 divide(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x / v2.x, v1.y / v2.y);
    }
    public Vector2 divide(Vector2 v1, float v2)
    {
        return new Vector2(v1.x / v2, v1.y / v2);
    }

    public Vector2 multiply(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x * v2.x, v1.y * v2.y);
    }


    Vector2 h(Vector2 uv)
    {
//        return Fract(multiply(uv , new Vector2(Random.value, Random.value)));
        return Fract(Sin(uv + internalSeed) * seed);
    }

    public override Vector4 doGen(Vector2 iFragCoord)
    {
        Vector2 uv = (scale * divide(iFragCoord, iResolution.y));// - new Vector2(7.0f, 4.0f);
        //        Debug.Log("h seed: " + h(uv));
        Vector2 m, r;
        float l, s = 1.0f;

        float ret = 0.0f;

        for (int c = 0; c < detail; c++)
        {
            m = Fract(uv * s);
            l = Vector2.Dot((uv * s) - m, internalSeed2);
            Vector2 ll = new Vector2(l, l);
            s += s;
            m = multiply(m, multiply(m, new Vector2(3.0f, 3.0f) - m - m));
            r = Vector2.Lerp(h(ll), h(ll + Vector2.one), m.x);
            ret += Mathf.Lerp(r.x, r.y, m.y) / s;
        }
        return new Vector4(ret, ret, ret, 1.0f);
    }

    public override void guiGen()
    {
        EditorGUILayout.BeginVertical();
        detail = EditorGUILayout.IntField("detail:", detail);
        seed = EditorGUILayout.FloatField("seed:", seed);
        scale = EditorGUILayout.FloatField("scale:", scale);
        EditorGUILayout.EndVertical();
    }

}


//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor window.
/// </summary>
public class GenTexTool : EditorWindow {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Window instance
	private static GenTexTool m_instance = null;

	public static GenTexTool instance {
		get {
			if(m_instance == null) {
				m_instance = (GenTexTool)EditorWindow.GetWindow(typeof(GenTexTool));
			}
			return m_instance;
		}
	}


    Texture2D backgroundImage = null;
    Vector2 textureSize = new Vector2(256.0f, 256.0f);
    TexFormats texFormat = TexFormats.RGBA;
    string textureName = "test.png";

    TextureGenBase currentTexGen = new Perlin();

    public int selectedTab = 0;
    public string[] toolbarOptions = new string[] { "Perlin Noise", "Voronoise", "Gradients" };

    private TextureFormat[] mapFormat = new TextureFormat[]
    {
        TextureFormat.Alpha8,
        TextureFormat.RGB24,
        TextureFormat.RGBA32
    };

    //------------------------------------------------------------------//
    // METHODS															//
    //------------------------------------------------------------------//
    /// <summary>
    /// Opens the window.
    /// </summary>
    [MenuItem("Tools/GenTexTool")]	// UNCOMMENT TO ADD MENU ENTRY!!!
	public static void ShowWindow() {
        Debug.Log("GenTexTool.ShowWindow - Opening Tools/GenTexTool ...");
        instance.Show();
	}

	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
    }

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called 100 times per second on all visible windows.
	/// </summary>
	public void Update() {
		
	}

	/// <summary>
	/// OnInspectorUpdate is called at 10 frames per second to give the inspector a chance to update.
	/// Called less times as if it was OnGUI/Update
	/// </summary>
	public void OnInspectorUpdate() {
		
	}

    void Generate(Texture2D frameBuffer, TextureGenBase texGen )
    {
        texGen.initGen(frameBuffer);

        Vector2 fragCoord = Vector2.zero;
        int idx = 0;

        Color[] pix = frameBuffer.GetPixels();

        for (int y = 0; y < frameBuffer.height; y++)
        {
            for (int x = 0; x < frameBuffer.width; x++)
            {
                fragCoord.Set(x, y);
                Vector4 frag = texGen.doGen(fragCoord);
                pix[idx++] = frag;
            }
        }

        frameBuffer.SetPixels(pix);
        frameBuffer.Apply();

    }


    Texture2D createFrameBuffer(int width, int height, TexFormats tFormat)
    {
        TextureFormat format = mapFormat[(int)tFormat];

        Texture2D newTexture = new Texture2D(width, height, format, false);
        newTexture.Fill(Color.green);
        newTexture.Apply();

        return newTexture;
    }
    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI() {
        EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

//                backgroundImage = (Texture2D)EditorGUILayout.ObjectField("Image", backgroundImage, typeof(Texture2D), false);
                GUILayout.Label(backgroundImage);

                EditorGUILayout.BeginHorizontal();
                    textureSize = EditorGUILayout.Vector2Field("Texture Size:", textureSize);
                    texFormat = (TexFormats)EditorGUILayout.EnumPopup("Texture format:", texFormat);

                    if (GUILayout.Button("Generate!"))
                    {
                        backgroundImage = createFrameBuffer((int)textureSize.x, (int)textureSize.y, texFormat);
                        if (currentTexGen != null)
                        {
                            Generate(backgroundImage, currentTexGen);
                        }
                    }


                    if (GUILayout.Button("Save"))
                    {
                        int barIdx = textureName.LastIndexOf('/');
                        string fName;
                        string path;

                        if (barIdx != -1)
                        {
                            fName = textureName.Substring(barIdx + 1);
                            path = textureName.Substring(0, barIdx);
                        }
                        else
                        {
                            fName = "";
                            path = "";
                        }

                        //                        string path = EditorUtility.SaveFilePanel("Save png", "", textureName, "png");
                        textureName = EditorUtility.SaveFilePanel("Save png", path, fName, "png");
                        if (textureName.Length != 0)
                        {
                            if (backgroundImage.format != TextureFormat.ARGB32 && backgroundImage.format != TextureFormat.RGB24)
                            {
				                Texture2D tempTexture = new Texture2D(backgroundImage.width, backgroundImage.height );
                                tempTexture.SetPixels(backgroundImage.GetPixels( 0 ), 0 );
                                backgroundImage = tempTexture;                            
                            }

                            byte[] pngData = backgroundImage.EncodeToPNG();
                            if (pngData != null)
                            {
                                File.WriteAllBytes(textureName, pngData);
                                AssetDatabase.Refresh();
                            }
                        }
                    }
                EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(0.0f);    

            EditorGUILayout.BeginVertical();
                selectedTab = GUILayout.Toolbar(selectedTab, toolbarOptions);
                if (currentTexGen != null)
                {
                    currentTexGen.guiGen();
                }
            EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

}