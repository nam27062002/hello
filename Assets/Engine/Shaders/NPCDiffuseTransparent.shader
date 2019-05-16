// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hungry Dragon/NPC/NPC Diffuse + Transparent (Spawners)"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		[KeywordEnum(None, Tint, Gradient, ColorRamp, ColorRampMasked)] ColorMode("Color mode", Float) = 0.0

		_Tint1("Tint Color 1", Color) = (1,1,1,1)
		_Tint2("Tint Color 2", Color) = (1,1,1,1)
		_RampTex("Ramp Texture", 2D) = "white" {}

		_StencilMask("Stencil Mask", int) = 10

		_Tint("Tint", color) = (1, 1, 1, 1)

	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "LightMode" = "ForwardBase"}

		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull back
			ColorMask RGBA

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ FREEZE
			#pragma multi_compile COLORMODE_NONE COLORMODE_TINT COLORMODE_GRADIENT COLORMODE_COLORRAMP COLORMODE_COLORRAMPMASKED

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

//			#define FRESNEL
//			#define MATCAP
			#define TINT
//			#define DYNAMIC_LIGHT
			#define OPAQUEALPHA

			#include "entities.cginc"

			ENDCG
		}
	}
			
	CustomEditor "NPCDiffuseShaderGUI"
}
