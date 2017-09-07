﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Ghost (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SpecularPower( "Specular power", float ) = 1
		_SpecularColor("Specular color (RGB)", Color) = (0, 0, 0, 0)
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_Tint("Tint color (RGB)", Color) = (1, 1, 1, 0)

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Pass
		{

//			Tags{ "Queue" = "Transparent+20" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
//			Blend SrcAlpha OneMinusSrcAlpha
//			Cull off Lighting Off ZWrite off Fog{ Color(0,0,0,0) }
//			ColorMask RGBA

//			Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
			Tags{ "Queue" = "Transparent+10" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			ZWrite on
			Cull back
//			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
//			ColorMask RGBA

			Stencil
			{
				Ref [_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#define HG_ENTITIES

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

//			#define SPECULAR
			#define FRESNEL
			#define GHOST
			#define TINT

			#include "entities.cginc"

			ENDCG
		}
	}
}
