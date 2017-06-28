// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Ghost (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("Normal", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_AlphaTex("Alpha", 2D) = "white" {}
		_SpecularPower( "Specular power", float ) = 1
		_SpecularColor("Specular color (RGB)", Color) = (0, 0, 0, 0)
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)

		_Tint("Tint color (RGB)", Color) = (1, 1, 1, 0)
		_WaveRadius("Wave Radius", float) = 1.5
		_WavePhase("Wave phase", float) = 1.0
		_AlphaMSKScale("Alpha mask scale", Range(0.5, 8.0)) = 3.0
		_AlphaMSKOffset("Alpha mask offset", Range(-0.3, 0.3)) = 0.0
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
			#define FRESNEL
			#endif

			#if HI_DETAIL_ON
			#define NORMALMAP
			#define SPECULAR
			#define FRESNEL
			#endif

			#define EMISSIVE
			#define CUSTOM_ALPHA

			#define CUSTOM_VERTEXPOSITION

			uniform float _WaveRadius;
			uniform float _WavePhase;

			float4 getCustomVertexPosition(inout appdata_t v)
			{
				float3 normal = v.vertex;
				normal.y = 0.0f;
				normal = normalize(normal);
//				float wvc = (1.0 - v.color.x) * v.color.w;	//vc.a = Wave intensity ; 
				float wvc = (1.0 - v.color.x);	//vc.a = Wave intensity ; 
				float incWave = (0.5 + sin((_Time.y  * _WavePhase) + (v.vertex.y * _WavePhase)) * 0.5) * _WaveRadius * wvc;
//				float4 tvertex = v.vertex + float4(normal.xyz, 0.0) * ((incWave.x + incWave.y + incWave.z) * 0.33333);
				float4 tvertex = v.vertex + float4(normal, 0.0) * incWave;
				return mul(UNITY_MATRIX_MVP, tvertex);
			}

			#define CUSTOM_TINT
			float4 getCustomTint(float4 col, float4 tint, float4 vcolor)
			{
				float4 col2 = col * tint;
//				col2.w = 0.0f;
				return lerp(col, col2, vcolor.w);
			}

			#include "entities.cginc"

			ENDCG
		}
	}
}
