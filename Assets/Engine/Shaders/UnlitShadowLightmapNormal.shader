// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Scenary/Diffuse + Lightmap + Normal Map" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_Specular("Specular Factor", float) = 3
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
	}

	SubShader {
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" "LightMode" = "ForwardBase" }
		LOD 100
		
		Pass {  

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
//				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

				#define HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"
				#include "Lighting.cginc"

				#if LOW_DETAIL_ON
				#endif

				#if MEDIUM_DETAIL_ON
				#define NORMALMAP
				#endif

				#if HI_DETAIL_ON
				#define NORMALMAP
				#define SPECULAR
				#endif

				#define FOG
				#define OPAQUEALPHA

				#include "scenary.cginc"

			ENDCG
		}
	}
}
