// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Lightmap And Recieve Shadow Transparent (On Line Decorations)" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}

		// FOG
		_FogColor ("Fog Color", Color) = (0,0,0,0)
		_FogStart( "Fog Start", float ) = 0
		_FogEnd( "Fog End", float ) = 100

	}

	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
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
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					HG_FOG_COORDS(1)
					LIGHTING_COORDS(2,3)
					float2 lmap : TEXCOORD4; 
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
					HG_TRANSFER_FOG(o, mul(_Object2World, v.vertex), _FogStart, _FogEnd);	// Fog
					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord);	// Color

					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col *= attenuation;

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col, _FogColor);	// Fog

					return col;
				}
			ENDCG
		}
	}
}
