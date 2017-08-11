﻿Shader "Hungry Dragon/Scenary/Scenary Standard"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_LightmapIntensity("Lightmap intensity", Range(0.001, 4.0)) = 1.0
		_SecondTexture("Blend Texture", 2D) = "white" {}
		_NormalTex("Normal Texture", 2D) = "white" {}
		_NormalStrength("Normal strength", Range(0.001, 1.0)) = 1.0

		_CutOff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
/*
		[Toggle(FOG)] _EnableFog("Fog", int) = 0.0
		[Toggle(DARKEN)] _EnableDarken("Darken", int) = 0.0
		[Toggle(SPECULAR)] _EnableSpecular("Specular", int) = 0.0
		[Toggle(CUSTOM_VERTEXCOLOR)] _AutomaticBlend("Automatic blend", int) = 0.0
		[Toggle(BLEND_TEXTURE)] _BlendTexture("BlendTexture", int) = 0.0
		[Toggle(NORMAL_MAP)] _NormalMap("Normal Map", int) = 0.0
		[Toggle(CUTOFF)] _CutOff("CutOff", int) = 0.0
*/
		_EmissivePower("Emissive Power", Range(0.0, 5.0)) = 1.0
		_BlinkTimeMultiplier("Emissive blink multiplier", Range(0.0, 5.0)) = 2.0

		_SpecularPower("Specular Power", float) = 3
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)

		_DarkenPosition("Darken position",  float) = 0.0
		_DarkenDistance("Darken distance",  float) = 20.0

		[KeywordEnum(NONE, OVERLAY, ADDITIVE, MODULATE)] VertexColor("Overlay mode", int) = 0
//		[Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0

		// Blending state
		[HideInInspector] _Mode("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0

		_StencilMask("Stencil Mask", int) = 5
	}

	SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" "LightMode" = "ForwardBase" }
		LOD 100

		Pass{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
//			#pragma glsl_no_auto_normalization
//			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma shader_feature  __ FOG
			#pragma shader_feature  __ DARKEN
			#pragma shader_feature  __ SPECULAR
			#pragma shader_feature  __ CUSTOM_VERTEXCOLOR
			#pragma shader_feature  __ BLEND_TEXTURE
			#pragma shader_feature  __ NORMALMAP
			#pragma shader_feature  __ CUTOFF
			#pragma shader_feature  __ VERTEXCOLOR_OVERLAY VERTEXCOLOR_ADDITIVE VERTEXCOLOR_MODULATE
			#pragma shader_feature	__ EMISSIVEBLINK
			#pragma shader_feature  __ LIGHTMAPCONTRAST

			#define HG_SCENARY

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "HungryDragon.cginc"
			#include "Lighting.cginc"

//			#define FOG
	
			#define OPAQUEALPHA
			#include "scenary.cginc"
			ENDCG
		}
	}
	CustomEditor "ScenaryShaderGUI"
}
