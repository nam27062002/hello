// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Automatic Texture Blending + Lightmap And Recieve Shadow" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture ("Second Texture (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass {  
			Tags { "LightMode" = "ForwardBase" }

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
				float4 getCustomVertexColor(inout appdata_t v)
				{
					return float4(v.color.xyz, 1.0 - dot(mul(float4(v.normal,0), unity_WorldToObject).xyz, float3(0,1,0)));
				}

				#include "scenary.cginc"

			ENDCG
		}
	}
}
