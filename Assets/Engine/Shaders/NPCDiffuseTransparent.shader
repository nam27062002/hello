// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hungry Dragon/NPC/NPC Diffuse + Fresnel + Transparent (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_Tint( "Tint", color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull back
		ColorMask RGBA

		Pass
		{
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

			#define FRESNEL
			#define TINT

			#include "entities.cginc"

			ENDCG
		}
	}
}
