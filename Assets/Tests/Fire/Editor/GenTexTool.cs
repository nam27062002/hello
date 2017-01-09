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

static class Mathv
{
    public static Vector2 Sin(Vector2 v)
    {
        return new Vector2(Mathf.Sin(v.x), Mathf.Sin(v.y));
    }

    public static Vector2 Floor(Vector2 v)
    {
        return new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
    }

    public static Vector2 Fract(Vector2 v)
    {
        return v - Floor(v);
    }

    public static float Fract(float v)
    {
        return v - Mathf.Floor(v);
    }

    public static Vector2 Mod(Vector2 v, float d)
    {
        return v - d * Floor(Div(v, d));
    }

    public static Vector2 Div(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x / v2.x, v1.y / v2.y);
    }
    public static Vector2 Div(Vector2 v1, float v2)
    {
        return new Vector2(v1.x / v2, v1.y / v2);
    }

    public static Vector2 Mul(Vector2 v1, Vector2 v2)
    {
        return new Vector2(v1.x * v2.x, v1.y * v2.y);
    }
    public static Vector2 Mul(Vector2 v1, float v2)
    {
        return new Vector2(v1.x * v2, v1.y * v2);
    }
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
    Vector2 m_v10 = new Vector2(1.0f, 0.0f);
    Vector2 m_v01 = new Vector2(0.0f, 1.0f);
    Vector2 m_v11 = new Vector2(1.0f, 1.0f);

    Vector2 m_three = new Vector2(3.0f, 3.0f);
    Vector2 m_vHashSeed = new Vector2(35.6898f, 24.3563f);
    float m_fHashSeed = 353753.373453f;
    //
    public float m_Tiles = 1.0f;
    public float m_Scale = 14.0f;
    public float m_Amplitude = 0.55f;
    public int m_Iterations = 8;
    //

    public Perlin()
    {
        serializedName = "PerlinPrefs";
    }

    //----------------------------------------------------------------------------------------
    float Hash(Vector2 p, float scale)
    {
        // This is tiling part, adjusts with the scale...
        p = Mathv.Mod(p, scale);
        return Mathv.Fract(Mathf.Sin(Vector2.Dot(p, m_vHashSeed)) * m_fHashSeed);
    }

    float Noise(Vector2 x, float scale)
    {
        x = Mathv.Mul(x, scale);

        Vector2 p = Mathv.Floor(x);
        Vector2 f = x - p;  //Fract
        f = Mathv.Mul(Mathv.Mul(f, f), m_three - Mathv.Mul(f, 2.0f));

        float res = Mathf.Lerp(Mathf.Lerp(Hash(p, scale),
            Hash(p + m_v10, scale), f.x),
            Mathf.Lerp(Hash(p + m_v01, scale),
            Hash(p + m_v11, scale), f.x), f.y);

        return res;
    }


    float fBm(Vector2 p)
    {
        float f = 0.4f;

        // Change starting scale to any integer value...
        float scale = m_Scale;
        float amp = m_Amplitude;
        for (int i = 0; i < m_Iterations; i++)
        {
            f += Noise(p, scale) * amp;
            amp *= -0.65f;
            // Scale must be multiplied by an integer value...
            scale *= 2.0f;
        }
        return f;
    }

    public override void initGen(Texture2D canvas)
    {
        iResolution.x = canvas.width;
        iResolution.y = canvas.height;
    }

    public override Vector4 doGen(Vector2 iFragCoord)
    {
        Vector2 uv = Mathv.Div(iFragCoord, iResolution) * m_Tiles;

        // Do the noise cloud (fractal Brownian motion)
        float bri = fBm(uv);

        bri = Mathf.Min(bri * bri, 1.0f);   // ...cranked up the contrast for no reason.


        return new Vector4(bri, bri, bri, 1.0f);
    }

    public override void guiGen()
    {
        EditorGUILayout.BeginVertical();
        m_Scale = EditorGUILayout.FloatField("Initial Scale:", m_Scale);
        m_Amplitude = EditorGUILayout.FloatField("Initial Amplitude:", m_Amplitude);
        m_Iterations = EditorGUILayout.IntField("Iterations:", m_Iterations);

        //        seed = EditorGUILayout.FloatField("seed:", seed);
        //        scale = EditorGUILayout.FloatField("scale:", scale);
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