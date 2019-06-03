// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Ghost (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_Tint("Tint color (RGB)", Color) = (1, 1, 1, 0)

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent+10" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }
		Pass
		{
			ZWrite off
			Cull back
			Blend SrcAlpha OneMinusSrcAlpha
//			Blend One OneMinusSrcAlpha // Premultiplied transparency
//			Blend OneMinusDstColor One
//			Blend One One // Additive

			Stencil
			{
				Ref[_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ FREEZE

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

//			#define SPECULAR
			#define FRESNEL
			#define GHOST
			#define TINT
			#define LITMODE_LIT

			#include "entities.cginc"

			ENDCG
		}
	}
}
