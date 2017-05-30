Shader "Hungry Dragon/Scenary/Scenary Standard"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_SecondTexture("Blend Texture", 2D) = "white" {}
		_NormalTex("Normal Texture", 2D) = "white" {}
		_NormalStrength("Normal strength", Range(0.001, 1.0)) = 1.0

		_CutOff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		[Toggle(FOG)] _EnableFog("Fog", int) = 1.0
		[Toggle(DARKEN)] _EnableDarken("Darken", int) = 1.0
		[Toggle(SPECULAR)] _EnableSpecular("Specular", int) = 1.0
		[Toggle(CUSTOM_VERTEXCOLOR)] _AutomaticBlend("Automatic blend", int) = 1.0


		_SpecularPower("Specular Power", float) = 3
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)

		_DarkenPosition("Darken position",  float) = 0.0
		_DarkenDistance("Darken distance",  float) = 20.0

		[KeywordEnum(OVERLAY, ADDITIVE, MODULATE)] VertexColor("Overlay mode", int) = 0
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
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma multi_compile __ FOG
			#pragma multi_compile __ DARKEN
			#pragma multi_compile __ SPECULAR
			#pragma multi_compile __ CUSTOM_VERTEXCOLOR
			#pragma multi_compile __ BLEND_TEXTURE
			#pragma multi_compile __ NORMALMAP
			#pragma multi_compile __ CUTOFF
			#pragma multi_compile __ VERTEXCOLOR_OVERLAY VERTEXCOLOR_ADDITIVE VERTEXCOLOR_MODULATE

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
