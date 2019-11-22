using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameConstants {
    namespace Materials {
        public static class Property {
            public static readonly int BLEND_MODE = Shader.PropertyToID("_BlendMode");


            public static readonly int TINT = Shader.PropertyToID("_Tint");
            public static readonly int TINT_COLOR = Shader.PropertyToID("_TintColor");
            public static readonly int FRESNEL_COLOR = Shader.PropertyToID("_FresnelColor"); 
            public static readonly int FRESNEL_COLOR_2 = Shader.PropertyToID("_FresnelColor2");
            public static readonly int FRESNEL_POWER = Shader.PropertyToID("_FresnelPower");
            public static readonly int INNER_LIGHT_COLOR = Shader.PropertyToID("_InnerLightColor");
            public static readonly int INNER_LIGHT_ADD = Shader.PropertyToID("_InnerLightAdd");
            public static readonly int COLOR_ADD = Shader.PropertyToID("_ColorAdd");
            public static readonly int OPACITY_SATURATION = Shader.PropertyToID("_OpacitySaturation");
            public static readonly int ORIGINAL_TEX = Shader.PropertyToID("_OriginalTex");
            public static readonly int COLOR = Shader.PropertyToID("_Color");
            public static readonly int INTENSITY = Shader.PropertyToID("_Intensity");
            public static readonly int ASH_LEVEL = Shader.PropertyToID("_AshLevel");
            public static readonly int POWER = Shader.PropertyToID("_Power");
            public static readonly int EMISSIVE_POWER = Shader.PropertyToID("_EmissivePower");
            public static readonly int GOLD_COLOR = Shader.PropertyToID("_GoldColor");

            public static readonly int FOG_START = Shader.PropertyToID("_FogStart");
            public static readonly int FOG_END = Shader.PropertyToID("_FogEnd");
            public static readonly int FOG_TEXTURE = Shader.PropertyToID("_FogTexture");


            public static readonly int LERP_VALUE = Shader.PropertyToID("_LerpValue");
            public static readonly int START_TIME = Shader.PropertyToID("_StartTime");
            public static readonly int START_POSITION = Shader.PropertyToID("_StartPosition");


            public static readonly int DISSOLVE_AMOUNT = Shader.PropertyToID("_DissolveAmount");


            public static readonly int COLOR_RAMP_ID_0 = Shader.PropertyToID("_ColorRampID0");
            public static readonly int COLOR_RAMP_ID_1 = Shader.PropertyToID("_ColorRampID1");
            public static readonly int COLOR_RAMP_AMOUNT = Shader.PropertyToID("_ColorRampAmount");
        }

        public static class Keyword {
            public static readonly string TINT = "TINT";
            public static readonly string CUSTOM_PARTICLE_SYSTEM = "CUSTOMPARTICLESYSTEM";
            public static readonly string CUTOFF = "CUTOFF";
            public static readonly string DOUBLESIDED = "DOUBLESIDED";
            public static readonly string FORCE_LIGHTMAP = "FORCE_LIGHTMAP";
            public static readonly string FREEZE = "FREEZE";
            public static readonly string FRESNEL = "FRESNEL";
            public static readonly string MATCAP = "MATCAP";
        }
    }
}