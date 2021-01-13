// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Hungry Dragon/StrokeSilhouette"
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0.8, 0.0, 0.0, 1.0)
		_OutlineWidth("Outline width", Range(0.0, 5.0)) = 0.08
		_SilhouetteColor("Silhouette Color", Color) = (0.0, 0.0, 0.0, 1.0)
	}


	CGINCLUDE

	#include "UnityCG.cginc" 
	#include "Lighting.cginc"
//	#include "../HungryDragon.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2fo {
		float4 vertex : POSITION;
	};

	float4 _OutlineColor;
	float _OutlineWidth;
	float4 _SilhouetteColor;

	v2fo vert(appdata_t v)
	{
		v2fo o;

		o.vertex = UnityObjectToClipPos(v.vertex);

		float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);

		o.vertex.xy += offset * o.vertex.z * _OutlineWidth;

/*		float4 nvert = float4(v.vertex.xyz + v.normal * _OutlineWidth, 1.0);
		o.vertex = UnityObjectToClipPos(nvert);*/
		return o;
	}

	v2fo vert2(appdata_t v)
	{
		v2fo o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		return o;
	}


	fixed4 frag(v2fo i) : SV_Target
	{
		return _OutlineColor;
	}

	fixed4 frag2(v2fo i) : SV_Target
	{
		return _SilhouetteColor;
	}


	ENDCG


	SubShader
	{
		Tags{ "Queue" = "Transparent+50" "RenderType" = "Transparent" }

		ColorMask RGBA
		LOD 100


		Pass{
			ZWrite off
			Cull back

			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
											//        BlendOp add, max // Traditional transparency

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG

		}

		Pass
		{
			Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

			cull back
			ztest less
			ZWrite On

			CGPROGRAM
			#pragma vertex vert2
			#pragma fragment frag2
			ENDCG
		}
	}
}


/*
//
//	inner stroke using only fresnel

Shader "Hungry Dragon/StrokeSilhouette"
{
	Properties	
	{
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}

		_OutlineColor("Outline Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_SilhouetteColor("Silhouette Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_CutOff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27

	}

	CGINCLUDE

	#include "UnityCG.cginc" 
	#include "Lighting.cginc"
	#include "HungryDragon.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2fo {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float3 viewDir : VECTOR;
	};

	uniform float4 _OutlineColor;
	uniform float4 _SilhouetteColor;

	uniform float _FresnelPower;

	uniform float _CutOff;

	v2fo vert(appdata_t v)
	{
		v2fo o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.normal = normalize(UnityObjectToWorldNormal(v.normal));
//		float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
//		float4 worldPos = UNITY_MATRIX_V[2];// mul(unity_ObjectToWorld, float4(0.0, 0.0, -1.0, 0.0));
//		o.viewDir = (worldPos);

		float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
		o.viewDir = viewDirection;

		return o;
	}

	fixed4 frag(v2fo i) : SV_Target
	{
		fixed4 frag;
		fixed fresnel = clamp(pow(max(dot(i.viewDir, i.normal), 0.0), _FresnelPower), 0.0, 1.0);

		frag = lerp(_OutlineColor, _SilhouetteColor, step(_CutOff, fresnel));// step(_CutOff, fresnel));
//		frag = lerp(_OutlineColor, _SilhouetteColor, fresnel);// step(_CutOff, fresnel));

		return frag;
	}


	ENDCG

	SubShader
	{
		Tags{ "Queue" = "Geometry+10" "IgnoreProjector" = "True" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		Cull Back
		//	LOD 100
		ColorMask RGBA
		ztest less
		ZWrite On

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}
*/