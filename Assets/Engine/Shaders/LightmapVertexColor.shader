// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Lightmap And VertexColor (Background)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}

		// FOG
		_FogColor ("Fog Color", Color) = (0,0,0,0)
		_FogStart( "Fog Start", float ) = 0
		_FogEnd( "Fog End", float ) = 100

	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass {  
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase
							
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
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
					float2 lmap : TEXCOORD4; 
					float4 color : COLOR;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;

				float4 _FogColor;
				float _FogStart;
				float _FogEnd;
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex), _FogStart, _FogEnd);	// Fog
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif
					o.color = v.color;
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;	// Color

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col, _FogColor);	// Fog
					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
					return col;
				}
			ENDCG
		}
		// Pass to render object as a shadow caster
//		Pass {
//			Name "ShadowCaster"
//			Tags { "LightMode" = "ShadowCaster" }
//
//			Fog {Mode Off}
//			ZWrite On ZTest LEqual Cull Off
//			Offset 1, 1
//
//			CGPROGRAM
//				#pragma vertex vert
//				#pragma fragment frag
//				#pragma multi_compile_shadowcaster
//				#pragma fragmentoption ARB_precision_hint_fastest
//
//				#include "UnityCG.cginc"
//				#include "AutoLight.cginc"
//
//				struct v2f { 
//					V2F_SHADOW_CASTER;
//				};
//
//				v2f vert (appdata_base v)
//				{
//					v2f o;
//					TRANSFER_SHADOW_CASTER(o)
//					return o;
//				}
//
//				float4 frag (v2f i) : COLOR
//				{
//					SHADOW_CASTER_FRAGMENT(i)
//				}
//			ENDCG
//		} //Pass
	}
}
