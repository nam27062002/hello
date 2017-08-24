// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon/Body Chinese" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normal Map (RGB)", 2D) = "white" {}
	_NormalStrenght ("Normal Strenght", float) = 1.0
	_DetailTex ("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular

	_ReflectionMap("Reflection Map", Cube) = "white" {}
	_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0

	_Tint ("Color Multiply", Color) = (1,1,1,1)
	_ColorAdd ("Color Add", Color) = (0,0,0,0)

	_InnerLightAdd ("Inner Light Add", float) = 0.0
	_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

	_SpecExponent ("Specular Exponent", float) = 1.0
	_Fresnel("Fresnel factor", Range(0, 10)) = 1.5
	_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
	_AmbientAdd("Ambient Add", Color) = (0,0,0,0)
	_SecondLightDir("Second Light dir", Vector) = (0,0,-1,0)
	_SecondLightColor("Second Light Color", Color) = (0.0, 0.0, 0.0, 0.0)

	_StencilMask("Stencil Mask", int) = 10
}


SubShader {
	Tags { "Queue"="Geometry+10" "IgnoreProjector"="True" "RenderType"="Opaque" "LightMode"="ForwardBase" }
	Cull Back
//	LOD 100
	ColorMask RGBA
	
	Pass {

		Stencil
		{
			Ref [_StencilMask]
			Comp always
			Pass Replace
			ZFail keep
		}

		ztest less
		ZWrite On

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON
			#pragma multi_compile __ SILHOUETTE


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
