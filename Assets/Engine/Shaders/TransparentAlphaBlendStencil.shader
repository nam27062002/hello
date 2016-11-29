﻿Shader "Hungry Dragon/TransparentAlphaBlend(Stencil)"
{
	Properties
	{
		_MainTex("Particle Texture", 2D) = "white" {}
	}

	Category
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }

		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		SubShader
		{
			Pass
			{
				Stencil
				{
					Ref 5
					Comp always
					Pass Replace
					ZFail keep
				}

				AlphaTest greater 0.01

				SetTexture[_MainTex]
				{
					combine texture * primary
				}
			}
		}
	}

	CustomEditor "GlowMaterialInspector"
}
