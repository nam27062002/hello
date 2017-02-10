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

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					HG_FOG_COORDS(1)
					#ifdef DYNAMIC_SHADOWS
					LIGHTING_COORDS(2,3)
					#endif
					#if LIGHTMAP_ON
					float2 lmap : TEXCOORD4;
					#endif
					float4 color : COLOR; 
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _CutOff;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
					#ifdef DYNAMIC_SHADOWS
					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#endif
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif
					o.color = v.color;
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;	// Color

					clip(col.a - _CutOff);

					#ifdef DYNAMIC_SHADOWS
					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col *= attenuation;
					#endif
					 
					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col);	// Fog


//					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
					HG_DEPTH_ALPHA(i, col)

					// col = fixed4(1,1,1,1) * i.fogCoord;
					return col;
				}
			ENDCG
		}
	}

//	Fallback "Hungry Dragon/VertexLit"
}
