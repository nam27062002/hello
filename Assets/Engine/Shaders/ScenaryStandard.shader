Shader "Hungry Dragon/Scenary/Scenary Standard"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_SecondTexture("Blend Texture", 2D) = "white" {}
		_NormalTex("Normal Texture", 2D) = "white" {}
		_NormalStrength("Normal strength", Range(0.0, 1.0)) = 1.0

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		[Toggle(FOG)] _EnableFog("Fog", int) = 1.0
		[Toggle(DARKEN)] _EnableDarken("Darken", int) = 1.0

		[Toggle(SPECULAR)] _EnableSpecular("Specular", int) = 1.0
		_SpecularPower("Specular Power", float) = 3
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)

		[Enum(UV0,0,UV1,1)] _UVSec("UV Set for secondary textures", Float) = 0

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
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma shader_feature FOG DARKEN SPECULAR BLEND_TEXTURE NORMALMAP
//			#pragma multi_compile  FOG, DARKEN, BLEND_TEXTURE, NORMALMAP

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
