using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameConstants
{
	public class Material{
		public static readonly int TINT = Shader.PropertyToID("_Tint");
		public static readonly int FRESNEL_COLOR = Shader.PropertyToID("_FresnelColor");
		public static readonly int INNER_LIGHT_COLOR = Shader.PropertyToID("_InnerLightColor");
		public static readonly int INNER_LIGHT_ADD = Shader.PropertyToID("_InnerLightAdd");
		public static readonly int COLOR_ADD = Shader.PropertyToID("_ColorAdd");
		public static readonly int OPACITY_SATURATION = Shader.PropertyToID("_OpacitySaturation");
		public static readonly int ORIGINAL_TEX = Shader.PropertyToID("_OriginalTex");

		public static readonly int FOG_START = Shader.PropertyToID("_FogStart");
		public static readonly int FOG_END = Shader.PropertyToID("_FogEnd");
		public static readonly int FOG_TEXTURE = Shader.PropertyToID("_FogTexture");

		public static readonly int LERP_VALUE = Shader.PropertyToID("_LerpValue");
		public static readonly int START_TIME = Shader.PropertyToID("_StartTime");
		public static readonly int START_POSITION = Shader.PropertyToID("_StartPosition");

	}
}
