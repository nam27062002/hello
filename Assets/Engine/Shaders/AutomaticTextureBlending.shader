// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Automatic Texture Blending + Lightmap And Recieve Shadow" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture ("Second Texture (RGB)", 2D) = "white" {}

	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 100
		
		Pass {  
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
							
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float3 normal : NORMAL;
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
					float blendValue : TEXCOORD5;
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


					o.blendValue = 1 - dot(mul ( float4(v.normal,0), unity_WorldToObject ).xyz, float3(0,1,0));
					o.blendValue = (o.blendValue * 2) - 1;

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

					float l = saturate( col.a + i.blendValue );
					col = lerp( col2, col, l);

					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col *= attenuation;

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
}
