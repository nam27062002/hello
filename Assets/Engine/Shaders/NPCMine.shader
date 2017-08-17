﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Mine (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("Normal", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_SpecularPower( "Specular power", float ) = 1
		_SpecularColor("Specular color (RGB)", Color) = (0, 0, 0, 0)
//		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
//		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Pass
		{

			Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
			Cull Back

			ZWrite on

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
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#define HG_ENTITIES

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#if LOW_DETAIL_ON
			#endif

			#if MEDIUM_DETAIL_ON
			#define NORMALMAP
			#define SPECULAR
			#endif

			#if HI_DETAIL_ON
			#define NORMALMAP
			#define SPECULAR
			#endif

//			#define NORMALMAP
//			#define SPECULAR

//			#define FRESNEL
//			#define MATCAP

			#include "entities.cginc"

			ENDCG
		}
	}
}
