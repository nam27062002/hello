// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Cocoon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SpecMask("Spec Mask", 2D) = "white" {}

		_SpecExponent("Specular Exponent", float) = 1.0
		[Rotation] _SecondLightDir("Second Light direction", Vector) = (0,0,-1,0)
		_AmbientColor("Ambient color", Color) = (0.0, 0.0, 0.0, 0.0)

		_VertexAnimation("Vertex Animation", Vector) = (1.0, 1.0, 1.0, 1.0)
		_TimePhase("Animation Phase", Float) = 1.0
		_Period("Period", Float) = 1.0

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }
		Pass
		{
//			Blend SrcAlpha OneMinusSrcAlpha
//			Cull Off
//			Lighting Off

			Cull back
			ZWrite on
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
			#pragma multi_compile __ FREEZE
			#pragma multi_compile __ TINT


//			#pragma multi_compile __ NORMALMAP
//			#pragma multi_compile __ SPECMASK

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#ifdef LOW_DETAIL_ON
//			#define SPECULAR
			#endif

			#ifdef MEDIUM_DETAIL_ON
			#define SPECMASK
			#endif

			#ifdef HI_DETAIL_ON
			#define SPECMASK
			#endif

//			#define SPECMASK
			#define VERTEX_ANIMATION
			#define AMBIENTCOLOR
//			#define TINT
			#define LITMODE_LIT


			#include "entities.cginc"

			ENDCG
		}
	}
}
