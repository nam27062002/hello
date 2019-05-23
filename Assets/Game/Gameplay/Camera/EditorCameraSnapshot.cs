using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class EditorCameraSnapshot : MonoBehaviour {

#if UNITY_EDITOR
    public enum ResolutionEnumerator
    {
        R_720p_1280X720,
        R_1080p_1920X1080,
        R_UHD4K_3840X2160,
        R_8K_7680X4320
    };

    public ResolutionEnumerator m_resolution = ResolutionEnumerator.R_UHD4K_3840X2160;
    private ResolutionEnumerator m_currentResolution = (ResolutionEnumerator)(-1);

    private readonly int m_4kWidth = 3840;
    private readonly int m_4kHeigth = 2160;
    private Camera m_renderCamera;
    private Camera m_originalCamera;

    private RenderTexture m_RenderTexture = null;
    private Texture2D m_Texture = null;

    private Rect m_rectArea;

    private bool m_doSnapshot = false;


    private int m_layermaskNPC;
    private int m_layermaskScenary;
    private int m_layermaskFire;
    private int m_layermaskDefault;
    private int m_layermaskBackground;
    private int m_layermaskUI;
    private int m_layermaskOriginal;

    private int m_screenShotcount = 0;

    private string m_screenshotPath;
    private string m_videoPath;


    public bool m_Video = false;
    public bool m_CaptureOnStart = false;

    private float m_maximumDeltaTimeBackUp;



    private Resolution getResolution(ResolutionEnumerator res)
    {
        Resolution resolution = new Resolution();
        switch (res)
        {
            case ResolutionEnumerator.R_720p_1280X720:
                resolution.width = 1280;
                resolution.height = 720;
                break;

            case ResolutionEnumerator.R_1080p_1920X1080:
                resolution.width = 1920;
                resolution.height = 1080;
                break;

            case ResolutionEnumerator.R_UHD4K_3840X2160:
                resolution.width = 3840;
                resolution.height = 2160;
                break;

            case ResolutionEnumerator.R_8K_7680X4320:
                resolution.width = 7680;
                resolution.height = 4320;
                break;
        }

        return resolution;
    }


    private void setResolutionTextures(ResolutionEnumerator resenum)
    {
        if (resenum != m_currentResolution)
        {
            if (m_RenderTexture != null)
            {
                DestroyImmediate(m_RenderTexture);
                m_RenderTexture = null;
            }
            if (m_Texture != null)
            {
                DestroyImmediate(m_Texture);
                m_Texture = null;
            }

            Resolution res = getResolution(resenum);

            m_RenderTexture = new RenderTexture(res.width, res.height, 24, RenderTextureFormat.ARGB32);
            m_Texture = new Texture2D(res.width, res.height, TextureFormat.ARGB32, false);

            m_rectArea = new Rect(0, 0, res.width, res.height);

            m_currentResolution = resenum;
        }
    }


    // Use this for initialization
    void Start () {

        m_originalCamera = gameObject.GetComponent<Camera>();
        m_renderCamera = new GameObject("Background tint camera", typeof(Camera)).GetComponent<Camera>();
        m_renderCamera.gameObject.SetActive(false);
		m_renderCamera.useOcclusionCulling = false;

        setResolutionTextures(m_resolution);
/*        m_RenderTexture = new RenderTexture(m_4kWidth, m_4kHeigth, 24, RenderTextureFormat.ARGB32);
        m_Texture = new Texture2D(m_4kWidth, m_4kHeigth, TextureFormat.ARGB32, false);

        m_rectArea = new Rect(0, 0, m_4kWidth, m_4kHeigth);*/

//        m_collidersMask = LayerMask.GetMask("Ground", "GroundVisible", "Player", "AirPreys", "WaterPreys", "MachinePreys", "GroundPreys", "Mines");
        m_layermaskNPC = LayerMask.GetMask("AirPreys", "WaterPreys", "MachinePreys", "GroundPreys", "Mines");
        m_layermaskDefault = LayerMask.GetMask("Default", "Obstacle");
        m_layermaskBackground = LayerMask.GetMask("Ignore Raycast");
        m_layermaskUI = LayerMask.GetMask("UI", "3dOverUI");
        m_layermaskOriginal = m_originalCamera.cullingMask | m_layermaskUI;

        m_screenshotPath = Directory.GetCurrentDirectory() + "/" + "HD_SS_";
        m_videoPath = Directory.GetCurrentDirectory() + "/" + "HD_VID_";

        m_screenShotcount = checkScreenshotCount(m_screenshotPath);

        m_maximumDeltaTimeBackUp = Time.maximumDeltaTime;

        m_doSnapshot = m_CaptureOnStart;

        Time.captureFramerate = Application.targetFrameRate;
    }

    private int checkScreenshotCount(string path)
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

    private int checkVideoCount(string path)
    {
        int count = -1;
        string spath;
        do
        {
            count++;
            spath = path + count.ToString() + ".png";

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

        if (m_RenderTexture != null)
        {
            DestroyImmediate(m_RenderTexture);
            m_RenderTexture = null;
        }

        if (m_Texture != null)
        {
            DestroyImmediate(m_Texture);
            m_Texture = null;
        }

    }


    // Update is called once per frame
    void Update () {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            setResolutionTextures(m_resolution);
            if (m_Video)
            {
                m_screenShotcount = checkVideoCount(m_videoPath);
                m_doSnapshot = !m_doSnapshot;
                Time.maximumDeltaTime = m_doSnapshot ? (1.0f / 30.0f) : m_maximumDeltaTimeBackUp;
            }
            else
            {
                m_screenShotcount = checkScreenshotCount(m_screenshotPath);
                m_doSnapshot = true;
            }
        }
#endif
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
        m_renderCamera.targetTexture = m_RenderTexture;
        m_renderCamera.cullingMask = layerMask;
        m_renderCamera.Render();


        RenderTexture.active = m_RenderTexture;

        m_Texture.ReadPixels(m_rectArea, 0, 0);

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
        byte[] bytes = m_Texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

    }


    void OnPreRender()
    {

        if (m_doSnapshot)
        {
            if (m_Video)
            {
                string filePath = m_videoPath + m_screenShotcount.ToString();
                doSnapshot(m_layermaskOriginal);
                saveSnapshot(filePath);

            }
            else
            {
                string filePath = m_screenshotPath + m_screenShotcount.ToString();

                doSnapshot(m_layermaskBackground);
                saveSnapshot(filePath + "1");

                doSnapshot(m_layermaskDefault);
                saveSnapshot(filePath + "2");

                doSnapshot(m_layermaskNPC);
                saveSnapshot(filePath + "3");

                doSnapshot(GameConstants.Layers.PLAYER);
                saveSnapshot(filePath + "4");

                //            doSnapshot(m_layermaskUI);
                //            saveSnapshot(filePath + "5");

                doSnapshot(m_layermaskOriginal);
                saveSnapshot(filePath + "5");

                m_doSnapshot = false;
            }
            m_screenShotcount++;
        }

    }
#endif


}
