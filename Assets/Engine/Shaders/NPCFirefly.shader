// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NPC/NPC Firefly"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EmissiveIntensity("Emissive intensity", float) = 1.0
		_EmissiveBlink("Emissive blink", float) = 1.0
		_EmissiveOffset("Emissive offset", float) = 0.0

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		Pass
		{
			Cull Back
			ZWrite on

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

			#pragma multi_compile __ FREEZE
			#pragma multi_compile __ TINT
            #pragma multi_compile __ NIGHT


			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#define EMISSIVE
//			#define DYNAMIC_LIGHT
			#define OPAQUEALPHA

			#define LITMODE_LIT

			#include "entities.cginc"
			ENDCG
		}
	}
}
