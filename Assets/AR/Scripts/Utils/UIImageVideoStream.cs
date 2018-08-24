using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

public class UIImageVideoStream : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////////

    public class ChangeResourceImageListener
    { 
        public virtual void onChangeResourceImage (UnityEngine.Object kImageResource) {}

        public virtual void onPlaybackFinished () {}
    }

    private ChangeResourceImageListener m_kChangeResourceImageListener = null;

    //////////////////////////////////////////////////////////////////////////

    public enum EFramesType
    {
        E_TEXTURES,
        E_SPRITES,

        E_UNKNOWN
    }

    private class AnimatedFrame
    {
        public int m_iLoadingImageIdx;

        public int m_iImageIdx;

        public Texture2D m_kTexture;

        public Sprite m_kSprite;

        public bool m_bLoading;

        public bool m_bReady;

        public bool m_bShowing;

        public bool m_bShown;

        public AnimatedFrame ()
        {
            m_iImageIdx = -1;

            m_kTexture = null;

            m_kSprite = null;

            m_bLoading = false;

            m_bReady = false;

            m_bShowing = false;

            m_bShown = false;
        }
    }

    private List <AnimatedFrame> m_kAnimationFrames = new List<AnimatedFrame> ();

    //////////////////////////////////////////////////////////////////////////

    private class AsyncLoadingRequest
    {
        public ResourceRequest m_kRequest;

        public AnimatedFrame m_kRelatedFrame;
    }

    private List <AsyncLoadingRequest> m_kAsyncLoadingRequests = new List<AsyncLoadingRequest> ();

    //////////////////////////////////////////////////////////////////////////

    private AnimatedFrame m_kCurrentLoadingFrame = null;

    private AnimatedFrame m_kCurrentShowingFrame = null;

    //////////////////////////////////////////////////////////////////////////

    private static readonly int EXTRA_BUFFER_IMAGES = 2;

    private EFramesType m_eFramesType = EFramesType.E_UNKNOWN;

    private string m_strPath;

    private int m_iInitFrame;

    private int m_iFinalFrame;

    private int m_iFrameRate;

    private int m_iFrameRateDelay;

    private float m_fPlaybackSpeed = 1.0f;

    private bool m_bRewindMode = false;

    //////////////////////////////////////////////////////////////////////////

    private DateTime m_kShowingFrameStartTime;

    private int m_iNextLoadingFrame;

    private float m_fNextLoadingFrame;

    private int m_iCurrentLoadingFrameSlot;

    private int m_iCurrentShowingFrameSlot;

    private bool m_bPlaying = false;

    private bool m_bStopped = false;

    private int m_iReplayIdx = -1;

    //////////////////////////////////////////////////////////////////////////



    // INTERNAL METHODS //////////////////////////////////////////////////////

    private void FinishAsyncLoadingOp (AsyncLoadingRequest kRequest)
    {
        if (kRequest.m_kRelatedFrame != null)
        {
            bool bFoundInstances = false;

            for (int i = 0; i < m_kAnimationFrames.Count; ++i)
            {
                if (m_kAnimationFrames [i] != kRequest.m_kRelatedFrame)
                {
                    if (m_eFramesType == EFramesType.E_TEXTURES)
                    {
                        if (m_kAnimationFrames [i].m_kTexture == kRequest.m_kRelatedFrame.m_kTexture)
                        {
                            bFoundInstances = true;
                        }
                    }
                    else if (m_eFramesType == EFramesType.E_SPRITES)
                    {
                        if (m_kAnimationFrames [i].m_kSprite == kRequest.m_kRelatedFrame.m_kSprite)
                        {
                            bFoundInstances = true;
                        }
                    }
                }
            }

            if (!bFoundInstances)
            {
                if (m_eFramesType == EFramesType.E_TEXTURES)
                {
                    Resources.UnloadAsset (kRequest.m_kRelatedFrame.m_kTexture);

                    kRequest.m_kRelatedFrame.m_kTexture = null;
                }
                else if (m_eFramesType == EFramesType.E_SPRITES)
                {
                    Resources.UnloadAsset (kRequest.m_kRelatedFrame.m_kSprite);

                    kRequest.m_kRelatedFrame.m_kSprite = null;
                }
            }
        }

        if (m_eFramesType == EFramesType.E_TEXTURES)
        {
            kRequest.m_kRelatedFrame.m_kTexture = kRequest.m_kRequest.asset as Texture2D;
        }
        else if (m_eFramesType == EFramesType.E_SPRITES)
        {
            kRequest.m_kRelatedFrame.m_kSprite = kRequest.m_kRequest.asset as Sprite;
        }

        kRequest.m_kRelatedFrame.m_bReady = true;
    }

    private void LoadImage (int iImageIdx, bool bForceWait = false)
    {
        if (m_kCurrentLoadingFrame != null)
        {
            string strImagePath = m_strPath;

            if (iImageIdx < 10)
                strImagePath += "0";

            strImagePath += iImageIdx;

            m_kCurrentLoadingFrame.m_iLoadingImageIdx = iImageIdx;

            ResourceRequest kRequest = Resources.LoadAsync (strImagePath);

            if (kRequest != null)
            {
                AsyncLoadingRequest kNewAsyncRequest = new AsyncLoadingRequest ();
                kNewAsyncRequest.m_kRequest = kRequest;
                kNewAsyncRequest.m_kRelatedFrame = m_kCurrentLoadingFrame;

                if (bForceWait)
                {
                    while (kRequest.isDone)
                    {
                        Thread.Sleep (1);
                    }

                    FinishAsyncLoadingOp (kNewAsyncRequest);
                }
                else
                {
                    m_kAsyncLoadingRequests.Add (kNewAsyncRequest);
                }
            }
        }
    }

    IEnumerator LoadInitImages ()
    {
        AnimatedFrame kInitFrame = new AnimatedFrame ();
        m_kCurrentLoadingFrame = kInitFrame;

        DateTime kInitTime = DateTime.UtcNow;

        CalculateNextLoadingFrame ();

        LoadImage (m_iInitFrame, true);

        TimeSpan kCurrentTime = DateTime.UtcNow - kInitTime;
        long iMillisSinceEpoch = (long) kCurrentTime.TotalMilliseconds;

        // As minimum we set m_iFrameRateDelay for loading
        if (iMillisSinceEpoch < m_iFrameRateDelay)
        {
            iMillisSinceEpoch = m_iFrameRateDelay;
        }

        Debug.TaggedLog ("Calety", "LoadInitImages: " + iMillisSinceEpoch);

        int iBufferSize = (int) Math.Ceiling((double) iMillisSinceEpoch / (double)m_iFrameRateDelay);

        // extra images just in case
        iBufferSize += EXTRA_BUFFER_IMAGES;

        Debug.TaggedLog ("Calety", "iBufferSize: " + iBufferSize);

        m_kAnimationFrames.Add (kInitFrame);

        m_kCurrentLoadingFrame.m_bReady = true;
        SetCurrentShowingFrame (kInitFrame);

        for (int i = 1; i < iBufferSize; ++i)
        {
            AnimatedFrame kNewFrame = new AnimatedFrame ();
            m_kCurrentLoadingFrame = kNewFrame;

            CalculateNextLoadingFrame ();
              
            LoadImage (m_iNextLoadingFrame, true);
            kNewFrame.m_bReady = true;

            m_kAnimationFrames.Add (kNewFrame);
        }

        CalculateNextLoadingFrame ();

        m_iCurrentLoadingFrameSlot = 0;
        m_iCurrentShowingFrameSlot = 0;

        yield return null;
    }

    private bool SetCurrentShowingFrame (AnimatedFrame kFrame)
    {
        if (kFrame.m_bReady)
        {
            if (m_kCurrentShowingFrame != null)
            {
                if (m_iReplayIdx != -1)
                {
                    if (m_kCurrentShowingFrame.m_iImageIdx == m_iReplayIdx)
                    {
                        m_iReplayIdx = -1;
                    }
                }

                m_kCurrentShowingFrame.m_bShown = true;

                m_kCurrentShowingFrame.m_bShowing = false;
            }

            m_kCurrentShowingFrame = kFrame;

            m_kCurrentShowingFrame.m_iImageIdx = m_kCurrentShowingFrame.m_iLoadingImageIdx;

            m_kCurrentShowingFrame.m_bShown = false;

            m_kCurrentShowingFrame.m_bShowing = true;

            m_kCurrentShowingFrame.m_bLoading = false;

            return true;
        }

        return false;
    }

    private void ReturnCurrentShowingFrame ()
    {
        if (m_kChangeResourceImageListener != null)
        {
            UnityEngine.Object kResource = GetCurrentFrameResource ();

            if (kResource != null)
            {
                m_kChangeResourceImageListener.onChangeResourceImage (kResource);
            }
        }
    }

    private void CalculateNextLoadingFrame ()
    {
        if (!m_bRewindMode)
        {
            m_fNextLoadingFrame += m_fPlaybackSpeed;

            if (m_fNextLoadingFrame > m_iFinalFrame)
            {
                m_fNextLoadingFrame = m_iFinalFrame;
            }
        }
        else
        {
            m_fNextLoadingFrame -= m_fPlaybackSpeed;

            if (m_fNextLoadingFrame < m_iInitFrame)
            {
                m_fNextLoadingFrame = m_iInitFrame;
            }
        }

        m_iNextLoadingFrame = (int) m_fNextLoadingFrame;
    }

    //////////////////////////////////////////////////////////////////////////



    // METHODS ///////////////////////////////////////////////////////////////

    public void Configure (EFramesType eFramesType, string strPath, int iInitFrame, int iFinalFrame, int iFPS)
    {
        m_eFramesType = eFramesType;

        m_strPath = strPath;

        m_iInitFrame = iInitFrame;

        m_iFinalFrame = iFinalFrame;

        m_iFrameRate = iFPS;

        m_iFrameRateDelay = 1000 / m_iFrameRate;

        m_iNextLoadingFrame = m_iInitFrame;
        m_fNextLoadingFrame = m_iInitFrame;

        StartCoroutine (LoadInitImages ());
    }

    public void Play ()
    {
        if (!m_bPlaying)
        {
            if (m_iReplayIdx == -1)
            {
                m_iReplayIdx = m_iInitFrame;
            }

            m_bStopped = false;

            m_kShowingFrameStartTime = DateTime.UtcNow;

            ReturnCurrentShowingFrame ();

            m_bPlaying = true;
        }
    }

    public void Stop ()
    {
        m_bStopped = true;

        m_bPlaying = false;

        m_iNextLoadingFrame = m_iInitFrame;
        m_fNextLoadingFrame = m_iInitFrame;

        if (m_kChangeResourceImageListener != null)
        {
            m_kChangeResourceImageListener.onPlaybackFinished ();   
        }
    }

    //////////////////////////////////////////////////////////////////////////



    // SETTERS //////////////////////////////////////////////////////////////

    public void SetListener (ChangeResourceImageListener kChangeResourceImageListener)
    {
        m_kChangeResourceImageListener = kChangeResourceImageListener;
    }

    public void SetPlaybackSpeed (float fSpeed)
    {
        m_fPlaybackSpeed = fSpeed;
    }

    public void SetIsInRewindMode (bool bRewind)
    {
        m_bRewindMode = bRewind;
    }

    //////////////////////////////////////////////////////////////////////////



    // GETTERS //////////////////////////////////////////////////////////////

    public int GetCurrentFrame ()
    {
        if (m_kCurrentShowingFrame != null)
        {
            return m_kCurrentShowingFrame.m_iImageIdx;
        }

        return m_iInitFrame;
    }

    public int GetTotalFrames ()
    {
        return m_iFinalFrame - m_iInitFrame;
    }

    public UnityEngine.Object GetCurrentFrameResource ()
    {
        if (m_kCurrentShowingFrame != null && (m_iReplayIdx == -1 || (m_iReplayIdx != -1 && m_kCurrentShowingFrame.m_iImageIdx == m_iReplayIdx)))
        {
            if (m_eFramesType == EFramesType.E_TEXTURES)
            {
                return m_kCurrentShowingFrame.m_kTexture;
            }
            else if (m_eFramesType == EFramesType.E_SPRITES)
            {
                return m_kCurrentShowingFrame.m_kSprite;
            }
        }

        return null;
    }

    public bool GetPlaybackFinished ()
    {
        if (m_bPlaying)
        {
            if (m_kCurrentShowingFrame != null && m_iReplayIdx == -1)
            {
                if (m_bRewindMode)
                {
                    if (m_kCurrentShowingFrame.m_iImageIdx == m_iInitFrame)
                    {
                        return true;
                    }
                }
                else
                {
                    if (m_kCurrentShowingFrame.m_iImageIdx == m_iFinalFrame)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        return true;
    }

    //////////////////////////////////////////////////////////////////////////



    // UNITY METHODS ////////////////////////////////////////////////////////

    void Update ()
    {
        if (m_bPlaying)
        {
            TimeSpan kCurrentTime = DateTime.UtcNow - m_kShowingFrameStartTime;
            long iMillisSinceEpoch = (long) kCurrentTime.TotalMilliseconds;

            if (iMillisSinceEpoch >= m_iFrameRateDelay)
            {
                if (GetPlaybackFinished ())
                {
                    Stop ();
                }
                else
                {
                    int iNextShowingFrameSlot = (m_iCurrentShowingFrameSlot + 1) % m_kAnimationFrames.Count;

                    if (SetCurrentShowingFrame (m_kAnimationFrames [iNextShowingFrameSlot]))
                    {
                        DateTime kBaseTime = m_kShowingFrameStartTime;
                        kBaseTime.AddMilliseconds (m_iFrameRateDelay);

                        TimeSpan kExtraTime = DateTime.UtcNow - kBaseTime;

                        m_kShowingFrameStartTime = DateTime.UtcNow;
                        m_kShowingFrameStartTime.Subtract (kExtraTime);

                        m_iCurrentShowingFrameSlot = iNextShowingFrameSlot;

                        ReturnCurrentShowingFrame ();
                    }
                }
            }
        }



        for (int i = m_kAsyncLoadingRequests.Count - 1; i >= 0; i--)
        {
            if (m_kAsyncLoadingRequests [i].m_kRequest.isDone && m_kAsyncLoadingRequests [i].m_kRelatedFrame.m_bShown)
            {
                FinishAsyncLoadingOp (m_kAsyncLoadingRequests [i]);

                m_kAsyncLoadingRequests.RemoveAt (i);
            }
        }

        if (m_bPlaying)
        {
            if (!m_kAnimationFrames [m_iCurrentLoadingFrameSlot].m_bLoading && m_kAnimationFrames [m_iCurrentLoadingFrameSlot].m_bShowing)
            {
                m_kCurrentLoadingFrame = m_kAnimationFrames [m_iCurrentLoadingFrameSlot];

                LoadImage (m_iNextLoadingFrame, false);
                m_kAnimationFrames [m_iCurrentLoadingFrameSlot].m_bReady = false;
                m_kAnimationFrames [m_iCurrentLoadingFrameSlot].m_bLoading = true;

                CalculateNextLoadingFrame ();

                m_iCurrentLoadingFrameSlot = (m_iCurrentLoadingFrameSlot + 1) % m_kAnimationFrames.Count;
            }
        }
        else
        {
            if (m_bStopped)
            {
                for (int i = 0; i < m_kAnimationFrames.Count; ++i)
                {
                    if (m_eFramesType == EFramesType.E_TEXTURES)
                    {
                        if (m_kAnimationFrames [i].m_kTexture != null)
                        {
                            Resources.UnloadAsset (m_kAnimationFrames [i].m_kTexture);

                            m_kAnimationFrames [i].m_kTexture = null;
                        }
                    }
                    else if (m_eFramesType == EFramesType.E_SPRITES)
                    {
                        if (m_kAnimationFrames [i].m_kSprite != null)
                        {
                            Resources.UnloadAsset (m_kAnimationFrames [i].m_kSprite);

                            m_kAnimationFrames [i].m_kSprite = null;
                        }
                    }
                }
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////

    
}
