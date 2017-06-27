// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon/Body Radioactive" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}

	_BumpMap ("Normal Map (RGB)", 2D) = "white" {}
	_NormalStrenght("Normal Strenght", float) = 1.0

	_DetailTex ("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular

//	_RadMap("Radiation Map", 2D) = "white" {}
//	_RadAmount("Radiation amount", Range(0.0, 1.0)) = 0.0

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

	_OutlineColor("Outline Color", Color) = (1.0, 1.0, 0.0, 1.0)
	_OutlineWidth("Outline Width", float) = 0.2
	_OutlineGradient("Outline Gradient", float) = 2.0

	_StencilMask("Stencil Mask", int) = 10
}

SubShader {
	Tags{ "Queue" = "Transparent+50" "RenderType" = "Transparent" "LightMode" = "ForwardBase" }

	ColorMask RGBA
	LOD 100


	Pass{
		ZWrite off
//		Ztest always
		Cull back

		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
//		BlendOp add, max // Traditional transparency

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

		#include "UnityCG.cginc" 
		#include "Lighting.cginc"
		#include "../HungryDragon.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
			float3 normal : NORMAL;
			float4 tangent : TANGENT;
		};

		struct v2fo {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float3 viewDir : TEXCOORD0;
		};

		float4 _OutlineColor;
		float _OutlineWidth;
		float _OutlineGradient;

		v2fo vert(appdata_t v)
		{
			v2fo o;
			float4 nvert = float4(v.vertex.xyz + v.normal * _OutlineWidth, 1.0);
			o.vertex = mul(UNITY_MATRIX_MVP, nvert);
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, nvert).xyz);

			return o;
		}

		fixed4 frag(v2fo i) : SV_Target
		{
			float intensity = clamp(pow(max(dot(i.viewDir, i.normal), 0.0), _OutlineGradient), 0.0, 1.0);
			return fixed4(_OutlineColor.rgb, intensity);
		}

		ENDCG

	}



	Pass {
		Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }

		Cull Back
		//	LOD 100

		Stencil
		{
			Ref [_StencilMask]
			Comp always
			Pass Replace
			ZFail keep
		}

		cull back
		ztest less
		ZWrite On


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
