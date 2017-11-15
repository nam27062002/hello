// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon/Dragon standard" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_DetailTex("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular
		_BumpMap ("Normal Map (RGB)", 2D) = "white" {}
		_NormalStrenght("Normal Strenght", Range(0.1, 5.0)) = 1.0

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Tint ("Color Multiply", Color) = (1,1,1,1)
		_ColorAdd ("Color Add", Color) = (0,0,0,0)

		_InnerLightAdd ("Inner Light Add", float) = 0.0
		_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

		_InnerLightWavePhase("Inner Light Wave Phase", float) = 1.0
		_InnerLightWaveSpeed("Inner Light Wave Speed", float) = 1.0

		_AmbientAdd("Ambient Add", Color) = (0,0,0,0)

		_Fresnel("Fresnel factor", Range(0, 10)) = 1.5
		_FresnelColor("Fresnel Color", Color) = (1,1,1,1)

		_SpecExponent("Specular Exponent", float) = 1.0
		_SecondLightDir("Second Light direction", Vector) = (0,0,-1,0)
		_SecondLightColor("Second Light color", Color) = (0.0, 0.0, 0.0, 0.0)

		_ReflectionMap("Reflection Map", Cube) = "white" {}
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0

		_FireMap("Fire Map", 2D) = "white" {}
		_FireAmount("Fire amount", Range(0.0, 1.0)) = 0.0

		// Blending state
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull mode", Float) = 0.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1.0 //"One"
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 0.0 //"Zero"
		[Enum(Opaque, 0, Transparent, 1)] _BlendMode("Blend mode", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0

		_StencilMask("Stencil Mask", int) = 10

		/// Toggle Material Properties

		[Toggle(NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0
		[Toggle(SPECULAR)] _EnableSpecular("Enable Specular Light", Float) = 0
		[Toggle(FRESNEL)] _EnableFresnel("Enable fresnel", Float) = 1.0
		[Toggle(CUTOFF)] _EnableCutoff("Enable cutoff", Float) = 0
		[Toggle(DOUBLESIDED)] _EnableDoublesided("Enable doublesided", Float) = 0
		[Toggle(SILHOUETTE)] _EnableSilhouette("Enable silhouette", Float) = 0

		/// Enum Material Properties
		[KeywordEnum(None, Reflection, Fire)] FXLayer("Additional FX layer", Float) = 0
		[KeywordEnum(Normal, AutoInnerLight, BlinkLights)] SelfIlluminate("Additional FX layer", Float) = 0
			
	}

	SubShader {
		Tags { "Queue"="Geometry+10" "RenderType"="Opaque" "LightMode"="ForwardBase" }
	//	LOD 100
		ColorMask RGBA
	
		Pass {

			Stencil
			{
				Ref [_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			Cull[_Cull]
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			ztest less

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#pragma shader_feature  __ SILHOUETTE
			#pragma shader_feature  __ NORMALMAP
			#pragma shader_feature  __ SPECULAR
			#pragma shader_feature  __ FRESNEL
			#pragma shader_feature  __ CUTOFF
			#pragma shader_feature  __ DOUBLESIDED

			#pragma shader_feature SELFILLUMINATE_NORMAL SELFILLUMINATE_AUTOINNERLIGHT SELFILLUMINATE_BLINKLIGHTS
			#pragma shader_feature FXLAYER_NORMAL FXLAYER_REFLECTION FXLAYER_FIRE

			#include "UnityCG.cginc" 
			#include "Lighting.cginc"
			#include "../HungryDragon.cginc"

//			#define FRESNEL

			#if LOW_DETAIL_ON
			#undef NORMALMAP
			#undef SPECULAR
			#endif

			#if MEDIUM_DETAIL_ON
			#undef SPECULAR
			#endif

			#if HI_DETAIL_ON
			#endif

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			#include "dragon.cginc"
			ENDCG
		}
	}
	CustomEditor "DragonShaderGUI"
}
