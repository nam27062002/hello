// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Texture Blending + Lightmap And Recieve Shadow" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture ("Second Texture (RGBA)", 2D) = "white" {}

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
					float4 color : COLOR;
					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					float4 color : COLOR;
					HG_FOG_COORDS(1)
					LIGHTING_COORDS(2,3)
					float2 lmap : TEXCOORD4; 
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _SecondTexture;

				float4 _FogColor;
				float _FogStart;
				float _FogEnd;
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.color = v.color;
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
					fixed4 col2 = tex2D(_SecondTexture, i.texcoord);	// Color
					float l = saturate( col.a + ( (i.color.r * 2) - 1 ) );
					col = lerp( col, col2, l);

					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col *= attenuation;

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col, _FogColor);	// Fog


					UNITY_OPAQUE_ALPHA(col.a);	// Opaque

					// col = fixed4(1,1,1,1) * i.fogCoord;
					return col;
				}
			ENDCG
		}
		// Pass to render object as a shadow caster
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }

			Fog {Mode Off}
			ZWrite On ZTest LEqual Cull Off
			Offset 1, 1

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_shadowcaster
				#pragma fragmentoption ARB_precision_hint_fastest

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				struct v2f { 
					V2F_SHADOW_CASTER;
				};

				v2f vert (appdata_base v)
				{
					v2f o;
					TRANSFER_SHADOW_CASTER(o)
					return o;
				}

				float4 frag (v2f i) : COLOR
				{
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		} //Pass
	}
}
