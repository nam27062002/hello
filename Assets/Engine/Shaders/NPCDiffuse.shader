﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Diffuse"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	
//		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
//		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
//		_FresnelColor2("Fresnel color 2 (RGB)", Color) = (0, 0, 0, 0)
//		_GoldColor("Gold color (RGB)", Color) = (0, 0, 0, 0)
		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Pass
		{
			Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase"}
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

//			#pragma multi_compile __ OPAQUEALPHA
			#pragma multi_compile __ FREEZE
			#pragma multi_compile __ TINT


			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

//			#define DYNAMIC_LIGHT
			#define OPAQUEALPHA

			#include "entities.cginc"
			ENDCG
		}
	}
}
