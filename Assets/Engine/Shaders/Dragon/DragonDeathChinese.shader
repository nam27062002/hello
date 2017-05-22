// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon/Death Chinese" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normal Map (RGB)", 2D) = "white" {}
	_DetailTex ("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular

//	_ReflectionMap("Reflection Map", Cube) = "white" {}
//	_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0

	_Tint ("Color Multiply", Color) = (1,1,1,1)
	_ColorAdd ("Color Add", Color) = (0,0,0,0)

	_InnerLightAdd ("Inner Light Add", float) = 0
	_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

	_SpecExponent ("Specular Exponent", float) = 1
	_Cutoff("Cutoff Level", Range(0, 1)) = 0.5
	_Fresnel("Fresnel factor", Range(0, 10)) = 1.5
	_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
	_AmbientAdd("Ambient Add", Color) = (0,0,0,0)

	_SecondLightDir("Second Light dir", Vector) = (0,0,-1,0)
	_SecondLightColor("Second Light Color", Color) = (0.0, 0.0, 0.0, 0.0)

}

SubShader {
	Tags {"Queue"="Transparent+20" "IgnoreProjector"="True" "RenderType"="Transparent" "LightMode" = "ForwardBase" }
	ZWrite on
	Blend SrcAlpha OneMinusSrcAlpha 
	Cull Off
//	Cull Front
	ColorMask RGBA
	
	Pass {
/*
		Stencil
		{
			Ref 5
			Comp always
			Pass Replace
			ZFail keep
		}
*/
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#include "UnityCG.cginc" 
			#include "Lighting.cginc"
			#include "../HungryDragon.cginc"

			#if LOW_DETAIL_ON
			#endif

			#if MEDIUM_DETAIL_ON
			#define FRESNEL
			#define NORMALMAP
			#endif

			#if HI_DETAIL_ON
			#define FRESNEL
			#define NORMALMAP
			#define SPEC

			#endif

			#define CUTOUT
			#define DOUBLESIDED

			#define REFL

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			#include "dragon.cginc"
		ENDCG
	}	
}
}
