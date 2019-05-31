// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Diffuse"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	
		[KeywordEnum(None, Tint, Gradient, ColorRamp, ColorRampMasked)] ColorMode("Color mode", Float) = 0.0
		[KeywordEnum(Unlit, Lit)] LitMode("Lit mode", Float) = 1.0

		_Tint1("Tint Color 1", Color) = (1,1,1,1)
		_Tint2("Tint Color 2", Color) = (1,1,1,1)
		_RampTex ("Ramp Texture", 2D) = "white" {}

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		Pass
		{
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

			#pragma multi_compile __ FREEZE
			#pragma multi_compile __ TINT
			#pragma shader_feature COLORMODE_NONE COLORMODE_TINT COLORMODE_GRADIENT COLORMODE_COLORRAMP COLORMODE_COLORRAMPMASKED COLORMODE_BLENDTEX
			#pragma shader_feature LITMODE_UNLIT LITMODE_LIT

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#define OPAQUEALPHA

			#define LITMODE_LIT

			#include "entities.cginc"
			ENDCG
		}
	}

	CustomEditor "NPCDiffuseShaderGUI"
}
