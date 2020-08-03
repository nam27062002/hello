﻿// Copyright (C) 2014 - 2016 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace TMPro.SpriteAssetUtilities
{
    public enum SpriteAssetImportFormats { None = 0, TexturePacker = 0x1 };

    public class TexturePacker
    {
        [System.Serializable]
        public struct SpriteFrame
        {
            public float x;
            public float y;
            public float w;
            public float h;

            public override string ToString()
            {
                string s = "x: " + x.ToString("f2") + " y: " + y.ToString("f2") + " h: " + h.ToString("f2") + " w: " + w.ToString("f2");
                return s;
            }
        }

        [System.Serializable]
        public struct SpriteSize
        {
            public float w;
            public float h;

            public override string ToString()
            {
                string s = "w: " + w.ToString("f2") + " h: " + h.ToString("f2");
                return s;
            }
        }

        [System.Serializable]
        public struct SpriteData
        {
            public string filename;
            public SpriteFrame frame;
            public bool rotated;
            public bool trimmed;
            public SpriteFrame spriteSourceSize;
            public SpriteSize sourceSize;
            public Vector2 pivot;

        }

        [System.Serializable]
        public class SpriteDataObject
        {
            public List<SpriteData> frames;
        }


    }
}