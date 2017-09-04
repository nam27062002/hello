// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Scenary/Diffuse + Lightmap + Animated Vertex (Water plants)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpeedWave ("Speed Wave", float) = 1.0
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "DisableBatching"="true" }
//		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100
		
		Pass {		
//			cull front
			cull off
			ZWrite On

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile __ FOG
				#pragma multi_compile __ DARKEN
				#pragma multi_compile __ CUTOFF
				#pragma multi_compile __ OPAQUEALFA
				#pragma multi_compile __ LIGHTMAP_ON

				#define HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

				#define FOG
				#define CUSTOM_VERTEXPOSITION
				#define OPAQUEALPHA

				float _SpeedWave;
				float4 getCustomVertexPosition(inout appdata_t v)
				{
					float hMult = v.vertex.y;
					//float4 tvertex = v.vertex + float4(sin((_Time.y * hMult * _SpeedWave ) * 0.525) * hMult * 0.08, 0.0, 0.0, 0.0f);
					float4 tvertex = v.vertex + float4(sin((_Time.y * hMult * _SpeedWave) * 0.525) * hMult * 0.08, 0.0, 0.0, 0.0f);
					//					tvertex.w = -0.5f;
					return UnityObjectToClipPos(tvertex);
				}	

				#include "scenary.cginc"
			ENDCG
		}
	}
}
