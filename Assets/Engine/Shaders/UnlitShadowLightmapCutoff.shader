// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Lightmap And Recieve Shadow Cutoff (On Line Decorations)" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_CutOff  ("alpha Cutoff", Range(0.0, 1.0)) = 0.5
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "AlphaTest"}
		LOD 100
//		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
//			Tags{ "LightMode" = "ForwardBase" }

		Pass {  


//			AlphaTest Greater [_CutOff]

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest

				#define HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

				#define FOG
				#define CUTOFF
				#include "scenary.cginc"
			ENDCG
		}
	}
}
