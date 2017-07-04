using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class EditorCameraSnapshot : MonoBehaviour {

#if UNITY_EDITOR

    private readonly int m_4kWidth = 3840;
    private readonly int m_4kHeigth = 2160;
    private Camera m_renderCamera;
    private Camera m_originalCamera;
    private RenderTexture m_4kRenderTexture;
    private Texture2D m_4kTexture;
    private Rect m_rectArea;

    private bool m_doSnapshot = false;


    private int m_layermaskPlayer;
    private int m_layermaskNPC;
    private int m_layermaskScenary;
    private int m_layermaskFire;
    private int m_layermaskDefault;
    private int m_layermaskBackground;
    private int m_layermaskUI;
    private int m_layermaskOriginal;

    private int m_screenShotcount = 0;

    private string m_screenshotPath;


	// Use this for initialization
	void Start () {

        m_originalCamera = gameObject.GetComponent<Camera>();
        m_renderCamera = new GameObject("Background tint camera", typeof(Camera)).GetComponent<Camera>();
        m_renderCamera.gameObject.SetActive(false);
		m_renderCamera.useOcclusionCulling = false;


        m_4kRenderTexture = new RenderTexture(m_4kWidth, m_4kHeigth, 24, RenderTextureFormat.ARGB32);
        m_4kTexture = new Texture2D(m_4kWidth, m_4kHeigth, TextureFormat.ARGB32, false);

        m_rectArea = new Rect(0, 0, m_4kWidth, m_4kHeigth);

//        m_collidersMask = LayerMask.GetMask("Ground", "GroundVisible", "Player", "AirPreys", "WaterPreys", "MachinePreys", "GroundPreys", "Mines");
        m_layermaskPlayer = LayerMask.GetMask("Player");
        m_layermaskNPC = LayerMask.GetMask("AirPreys", "WaterPreys", "MachinePreys", "GroundPreys", "Mines");
        m_layermaskDefault = LayerMask.GetMask("Default", "Obstacle");
        m_layermaskBackground = LayerMask.GetMask("Ignore Raycast");
        m_layermaskUI = LayerMask.GetMask("UI", "3dOverUI");
        m_layermaskOriginal = m_originalCamera.cullingMask | m_layermaskUI;

        m_screenshotPath = Directory.GetCurrentDirectory() + "/" + "HD_SS_";

        m_screenShotcount = checkScreenshotCount(m_screenshotPath);
    }

    int checkScreenshotCount(string path)
    {
        int count = -1;
        string spath;
        do
        {
            count++;
            spath = path + count.ToString() + "1.png";

        } while (File.Exists(spath));
        return count;
    }


    public void OnDestroy()
    {
        if (m_renderCamera != null)
        {
            m_renderCamera.targetTexture = null;
            DestroyObject(m_renderCamera);
        }

        if (m_4kRenderTexture != null)
        {
            DestroyImmediate(m_4kRenderTexture);
            m_4kRenderTexture = null;
        }

        if (m_4kTexture != null)
        {
            DestroyImmediate(m_4kTexture);
            m_4kTexture = null;
        }

    }


    // Update is called once per frame
    void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
        {
            m_doSnapshot = true;
        }
	}


    void doSnapshot(int layerMask)
    {
        m_renderCamera.CopyFrom(m_originalCamera);
        //          m_renderCamera.transform.CopyFrom(m_originalCamera.transform, true);
        m_renderCamera.backgroundColor = Color.clear;
        m_renderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_renderCamera.renderingPath = RenderingPath.Forward;
        m_renderCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        m_renderCamera.depthTextureMode = DepthTextureMode.Depth;
        m_renderCamera.targetTexture = m_4kRenderTexture;
        m_renderCamera.cullingMask = layerMask;
        m_renderCamera.Render();


        RenderTexture.active = m_4kRenderTexture;

        m_4kTexture.ReadPixels(m_rectArea, 0, 0);

    }

    void saveSnapshot(string path)
    {
        path += ".png";
        // Compose screenshot path
        Debug.Log("Saving screenshot at " + path);

        // Overwrite any existing picture with the same name
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // Save picture!
        byte[] bytes = m_4kTexture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

    }


    void OnPreRender()
    {

        if (m_doSnapshot)
        {
            string filePath = m_screenshotPath + m_screenShotcount.ToString();

            doSnapshot(m_layermaskBackground);
            saveSnapshot(filePath + "1");

            doSnapshot(m_layermaskDefault);
            saveSnapshot(filePath + "2");

            doSnapshot(m_layermaskNPC);
            saveSnapshot(filePath + "3");

            doSnapshot(m_layermaskPlayer);
            saveSnapshot(filePath + "4");

//            doSnapshot(m_layermaskUI);
//            saveSnapshot(filePath + "5");

            doSnapshot(m_layermaskOriginal);
            saveSnapshot(filePath + "5");

            m_screenShotcount++;
            m_doSnapshot = false;
        }

    }
#endif


}
