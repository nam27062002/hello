// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Jelly fish"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("Normal", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_SpecularPower( "Specular power", float ) = 1
		_SpecularColor("Specular color (RGB)", Color) = (0, 0, 0, 0)
		_AmbientColor("Ambient color", Color) = (0.0, 0.0, 0.0, 0.0)

//		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
//		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_TimePhase("Head Time Phase", Float) = 1.0
		_Period("Head Period", Float) = 1.0
		_VertexAnimation("Head Vertex Animation", Vector) = (1.0, 1.0, 1.0, 1.0)

		_TimePhase2("Legs Time Phase 2", Float) = 1.0
		_Period2("Legs Period", Float) = 50.0
		_VertexAnimation2("Legs Vertex Animation2", Vector) = (1.0, 1.0, 1.0, 1.0)
		_VertexAnimation3("Legs Vertex Animation3", Vector) = (1.0, 1.0, 1.0, 1.0)

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
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#pragma multi_compile __ FREEZE
			#pragma multi_compile __ TINT
            #pragma multi_compile __ NIGHT


			//#pragma multi_compile __ NORMALMAP
			//#pragma multi_compile __ SPECULAR

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#ifdef LOW_DETAIL_ON
			#endif

			#ifdef MEDIUM_DETAIL_ON
			#define NORMALMAP
			#define SPECULAR
			#endif

			#ifdef HI_DETAIL_ON
			#define NORMALMAP
			#define SPECULAR
			#endif

			#define VERTEX_ANIMATION
			#define JELLY
//			#define DYNAMIC_LIGHT
			#define AMBIENTCOLOR

			#define LITMODE_LIT

			#include "entities.cginc"

			ENDCG
		}
	}
}
