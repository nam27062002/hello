Shader "Hungry Dragon/NPC/NPC Diffuse Lit-Unlit Outline"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	
		[KeywordEnum(None, Tint, Gradient, ColorRamp, ColorRampMasked)] ColorMode("Color mode", Float) = 0.0
		[KeywordEnum(Unlit, Lit)] LitMode("Lit mode", Float) = 0.0

		_Tint1("Tint Color 1", Color) = (1,1,1,1)
		_Tint2("Tint Color 2", Color) = (1,1,1,1)
		_RampTex ("Ramp Texture", 2D) = "white" {}

		[KeywordEnum(X, Y, Z)] BlendAxis("Blend axis", Float) = 0.0
		_BlendUVScale("Blend uv scale", Range(0.1, 2.0)) = 1.0
		_BlendUVOffset("Blend uv offset", Range(-1.0, 1.0)) = 0.0
		_BlendAlpha("Blend alpha", Range(0.0, 1.0)) = 1.0

		[Toggle(REFLECTIONMAP)] _EnableReflectionMap("Enable Reflection map", Float) = 0.0
		_ReflectionMap("Texture", Cube) = "white" {}
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.5

		[Toggle(FRESNEL)] _EnableFresnel("Enable fresnel", Float) = 0.0
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)

		_OutlineColor("Outline Color", Color) = (0.8, 0.0, 0.0, 1.0)
		_OutlineWidth("Outline width", Range(0.0, 1.0)) = 0.08

		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" "LightMode" = "ForwardBase" }
		Pass
		{

			ZWrite off
			Cull back

			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
											//        BlendOp add, max // Traditional transparency

			CGPROGRAM

			#pragma vertex vertOutline
			#pragma fragment fragOutline


			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2fo {
				float4 vertex : POSITION;
			};

			float4 _OutlineColor;
			float _OutlineWidth;

			v2fo vertOutline(appdata_t v)
			{
				v2fo o;

//				o.vertex = UnityObjectToClipPos(v.vertex);
//				float3 normal = UnityObjectToWorldNormal(v.normal);

//				float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
//				float2 offset = TransformViewToProjection(norm.xy);

//				o.vertex.xy += offset * o.vertex.z * _OutlineWidth;

				float4 nvert = float4(v.vertex.xyz + v.normal * _OutlineWidth, 1.0);
				o.vertex = UnityObjectToClipPos(nvert);
				return o;
			}

			fixed4 fragOutline(v2fo i) : SV_Target
			{
				return _OutlineColor;
			}

			ENDCG
		}

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
			#pragma shader_feature COLORMODE_NONE COLORMODE_TINT COLORMODE_GRADIENT COLORMODE_COLORRAMP COLORMODE_COLORRAMPMASKED COLORMODE_BLENDTEX
			#pragma shader_feature LITMODE_UNLIT LITMODE_LIT
			#pragma shader_feature BLENDAXIS_X BLENDAXIS_Y BLENDAXIS_Z
			#pragma shader_feature __ REFLECTIONMAP
			#pragma shader_feature __ FRESNEL

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#define OPAQUEALPHA

			#include "entities.cginc"
			ENDCG
		}
	}

	CustomEditor "NPCDiffuseShaderGUI"
}
