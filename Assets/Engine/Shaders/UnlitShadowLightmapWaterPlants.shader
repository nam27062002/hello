// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Lightmap And Recieve Shadow Animated Vertex(On Line Decorations)" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpeedWave ("Speed Wave", float) = 1.0
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase" }
//		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100
		
		Pass {		

			Stencil
			{
				Ref 4
				Comp always
				Pass Replace
				ZFail keep
			}

			cull front

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase

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
//					LIGHTING_COORDS(2,3)
					#if LIGHTMAP_ON
					float2 lmap : TEXCOORD4;
					#endif
					float4 color : COLOR; 
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _SpeedWave;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					float hMult = v.vertex.y;
					v.vertex += float4(sin((_Time.y * 10.0 * hMult * _SpeedWave ) * 0.525) * hMult * 0.08, 0.0, 0.0, 0.0f);
//					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.vertex = UnityObjectToClipPos(v.vertex);

					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
//					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif
					o.color = v.color;
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;	// Color

//					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
//					col *= attenuation;
					 
					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm; 
					#endif

					HG_APPLY_FOG(i, col);	// Fog


					UNITY_OPAQUE_ALPHA(col.a);	// Opaque

					// col = fixed4(1,1,1,1) * i.fogCoord;
					return col;
				}
			ENDCG
		}
	}

//	Fallback "Mobile/VertexLit"
}
