// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Scenary/Automatic Texture Blending + Lightmap + Darken" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture("Second Texture (RGB)", 2D) = "white" {}
		_DarkenPosition("Darken position",  float) = 0.0
		_DarkenDistance("Darken distance",  float) = 20.0
	}

	SubShader {
		Tags { "Queue" = "Geometry" "RenderType"="Opaque" }
		LOD 100
		
		Pass {  
			Tags { "LightMode" = "ForwardBase"}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest
		
				#define	HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"


				#define FOG
				#define BLEND_TEXTURE

				#define CUSTOM_VERTEXCOLOR		

				#define DARKEN
/*
				#define CUSTOM_VERTEXPOSITION

				uniform float _WaveRadius;
				uniform float _WavePhase;

				float4 getCustomVertexPosition(inout appdata_t v)
				{
					float3 incWave = (0.5 + sin((_Time.y  * _WavePhase) + (v.vertex.xyz * _WavePhase)) * 0.5) * _WaveRadius;
					float4 tvertex = v.vertex + float4(v.normal, 0.0) * (incWave.x + incWave.y + incWave.z) * 0.33333;
					return mul(UNITY_MATRIX_MVP, tvertex);
				}
*/
				#include "scenary.cginc"

			ENDCG
		}
	}
}
