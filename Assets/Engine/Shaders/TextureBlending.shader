// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Texture Blending Overlay + Lightmap And Recieve Shadow" 
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
					half2 texcoord2 : TEXCOORD5;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _SecondTexture;
				float4 _SecondTexture_ST;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord2 = TRANSFORM_TEX(v.texcoord, _SecondTexture);
					o.color = v.color;
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif

					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					
					fixed4 col = tex2D(_MainTex, i.texcoord);	// Color
					fixed4 col2 = tex2D(_SecondTexture, i.texcoord2);	// Color
					float l = saturate( col.a + ( (i.color.a * 2) - 1 ) );
//					float l = clamp(col.a + (i.color.a * 2.0) - 1.0, 0.0, 1.0);
					col = lerp( col2, col, l);
					// Sof Light with vertex color 
					// http://www.deepskycolors.com/archive/2010/04/21/formulas-for-Photoshop-blending-modes.html
					// https://en.wikipedia.org/wiki/Relative_luminance
					float luminance = 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
					if ( luminance > 0.5 )
					{
						fixed4 one = fixed4(1,1,1,1);
						// col = one- (one-col) * (1-(i.color-fixed4(0.5,0.5,0.5,0.5)));	// Soft Light
						col = one - 2 * (one-i.color) * (one-col);	// Overlay
					}
					else
					{
						// col = col * (i.color + fixed4(0.5,0.5,0.5,0.5));	// Soft Light
						col = 2 * i.color * col;	// Overlay
					}

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
