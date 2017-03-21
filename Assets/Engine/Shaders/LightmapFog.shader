// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Simple Lightmap & fog(Background)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}

	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass {  
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
//				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest							

				#define HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

				#define FOG
				#include "scenary.cginc"
			ENDCG
		}
	}
}
