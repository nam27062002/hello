// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Texture Blending Vertex Color + Lightmap And Recieve Shadow" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture ("Second Texture (RGB)", 2D) = "white" {}

	}

	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		LOD 100
		
		Pass {  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color : COLOR;
//					float3 normal : NORMAL;
					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif
				}; 

				struct v2f {
					float4 vertex : SV_POSITION; 
					half2 texcoord : TEXCOORD0;
					HG_FOG_COORDS(1)
					LIGHTING_COORDS(2,3)
					float2 lmap : TEXCOORD4; 
					float4 color : COLOR;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _SecondTexture;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

					o.color = v.color;

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					HG_TRANSFER_FOG(o, worldPos);	// Fog

					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif

					return o;
				}
				 
				fixed4 frag (v2f i) : SV_Target
				{  
					fixed4 col = tex2D(_MainTex, i.texcoord);	// Color
					fixed4 col2 = tex2D(_SecondTexture, i.texcoord);	// Color

					float l = i.color.a; // clamp(i.texcoord.x, 0.0, 1.0);//i.color.a;
					col = lerp( col2, col, l);

					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col.rgb *= attenuation;
					col.rgb += i.color.rgb;

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col);	// Fog

					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
					return col;
				}
			ENDCG

		}
	}
//	Fallback "Mobile/VertexLit"
}
