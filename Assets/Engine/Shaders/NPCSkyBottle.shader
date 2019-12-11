// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Sky bottle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Tint("Tint color (RGB)", Color) = (1, 1, 1, 0)

		_NormalTex("Normal", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3

		_SpecularPower("Specular power", float) = 1
		_SpecularColor("Specular color (RGB)", Color) = (0, 0, 0, 0)

		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)

		_ReflectionMap("Reflection Map", Cube) = "white" {}
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0


		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent+10" "RenderType" = "Transparent" }
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
            #pragma multi_compile __ NIGHT


			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#define NORMALMAP
			#define SPECULAR
			#define OPAQUESPECULAR
			#define REFLECTIONMAP
			#define NOAMBIENT
			#define FRESNEL
			#define LITMODE_LIT


//			#define GHOST
//			#define TINT

			#include "entities.cginc"

			ENDCG
		}
	}
}
