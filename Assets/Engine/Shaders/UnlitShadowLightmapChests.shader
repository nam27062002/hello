// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Scenary/Diffuse + Lightmap (Chests)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_StencilMask("Stencil Mask", int) = 10
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase" }
		LOD 100
		
		Pass {		

			Stencil
			{
				Ref [_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
//				#pragma glsl_no_auto_normalization
//				#pragma fragmentoption ARB_precision_hint_fastest

				#define HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

//				#define FOG
				#define OPAQUEALPHA

				#include "scenary.cginc"

			ENDCG
		}
	}
}
