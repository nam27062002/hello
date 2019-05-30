// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hungry Dragon/NPC/NPC Diffuse + Transparent (Spawners)"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		[KeywordEnum(None, Tint, Gradient, ColorRamp, ColorRampMasked, BlendTex)] ColorMode("Color mode", Float) = 0.0
		[KeywordEnum(Unlit, Lit)] LitMode("Lit mode", Float) = 1.0

		_Tint1("Tint Color 1", Color) = (1,1,1,1)
		_Tint2("Tint Color 2", Color) = (1,1,1,1)
		_RampTex("Ramp Texture", 2D) = "white" {}

		_StencilMask("Stencil Mask", int) = 10

		_Tint("Tint", color) = (1, 1, 1, 1)

		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull mode", Float) = 0.0
		[Toggle(OPAQUEALPHA)] _OpaqueAlpha("Opaque alpha", Float) = 1.0

	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "LightMode" = "ForwardBase"}

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull [_Cull]
			ColorMask RGBA

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ FREEZE
			#pragma shader_feature COLORMODE_NONE COLORMODE_TINT COLORMODE_GRADIENT COLORMODE_COLORRAMP COLORMODE_COLORRAMPMASKED COLORMODE_BLENDTEX
			#pragma shader_feature LITMODE_UNLIT LITMODE_LIT
			#pragma multi_compile __ OPAQUEALPHA

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#define TINT
			#define LITMODE_LIT

			#include "entities.cginc"

			ENDCG
		}
	}
			
	CustomEditor "NPCDiffuseShaderGUI"
}
