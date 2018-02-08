// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Cocoon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SpecMask("Spec Mask", 2D) = "white" {}
		_SpecularPower( "Specular power", float ) = 1
		_SpecularColor("Specular color (RGB)", Color) = (0, 0, 0, 0)
		_Tint("Tint", Color) = (1,1,1,1)

//		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
//		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_VertexAnimation("Vertex Animation", Vector) = (1.0, 1.0, 1.0, 1.0)
		_TimePhase("Animation Phase", Float) = 1.0
		_Period("Period", Float) = 1.0

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Pass
		{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }

//			Blend SrcAlpha OneMinusSrcAlpha
//			Cull Off
//			Lighting Off

			Cull back
//			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha

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
//			#pragma multi_compile __ NORMALMAP
//			#pragma multi_compile __ SPECULAR

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#if LOW_DETAIL_ON
			#endif

			#if MEDIUM_DETAIL_ON
			#define SPECULAR
			#endif

			#if HI_DETAIL_ON
			#define SPECULAR
			#endif

			#define SPECMASK
			#define VERTEX_ANIMATION
			#define TINT

			#include "entities.cginc"

			ENDCG
		}
	}
}
