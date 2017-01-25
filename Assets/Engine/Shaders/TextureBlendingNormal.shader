// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Texture Blending + Vertex Color Overlay + Lightmap And Recieve Shadow + Normal Map" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture ("Second Texture (RGB)", 2D) = "white" {}
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_Specular("Specular Factor", float) = 3
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
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
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"
				#include "Lighting.cginc"

				#if LOW_DETAIL_ON
				#endif

				#if MEDIUM_DETAIL_ON
				#define RIM
				#define BUMP
				#endif

				#if HI_DETAIL_ON
				#define RIM
				#define BUMP
				#define SPEC
				#endif

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color : COLOR;
					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					float4 color : COLOR;
					HG_FOG_COORDS(1)
					#if LIGHTMAP_ON
					float2 lmap : TEXCOORD2;
					#endif
					float2 texcoord2 : TEXCOORD3;

					float3 normalWorld : NORMAL;
					#ifdef BUMP
					float3 tangentWorld : TANGENT;
					float3 binormalWorld : TEXCOORD4;
					#endif
					float3 halfDir : TEXCOORD5;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _SecondTexture;
				float4 _SecondTexture_ST;
				#ifdef BUMP
				uniform sampler2D _NormalTex;
				uniform float4 _NormalTex_ST;
				uniform float _NormalStrength;
				#endif
				uniform float _Specular;
				uniform fixed4 _SpecularDir;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord2 = TRANSFORM_TEX(v.texcoord, _SecondTexture);
					o.color = v.color;
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
//					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif

					#ifdef BUMP
																							// To calculate tangent world
					float4x4 modelMatrix = unity_ObjectToWorld;
					float4x4 modelMatrixInverse = unity_WorldToObject;
					o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
					o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
					#else
					o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
					#endif

					fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					// Half View - See: Blinn-Phong
					float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
					float3 lightDirection = normalize(_SpecularDir.rgb);
					o.halfDir = normalize(lightDirection + viewDirection);

					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					
					float4 col = tex2D(_MainTex, i.texcoord);	// Color
					float specMask = col.w;
					float4 col2 = tex2D(_SecondTexture, i.texcoord2);	// Color
					float l = saturate( col.a + ( (i.color.a * 2) - 1 ) );
//					float l = clamp(col.a + (i.color.a * 2.0) - 1.0, 0.0, 1.0);
//					col = lerp( col2, col, l);

					col.a = 1.0;
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

//					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
//					col *= attenuation;

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif


					HG_APPLY_FOG(i, col);	// Fog
//					col = 0.5;

//					float4 encodedNormal = tex2D(_NormalTex, _NormalTex_ST.xy * i.texcoord + _NormalTex_ST.zw);
					#ifdef BUMP
					float4 encodedNormal = tex2D(_NormalTex, i.texcoord);
					float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
					float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
					float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
					#else
					float3 normalDirection = i.normalWorld;
					#endif

					fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _Specular);

					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
					return col + (specular * specMask * i.color * _LightColor0);
//					return col + (specular  * _LightColor0);
//					return col;
				}
			ENDCG
		}
	}
	Fallback "Mobile/VertexLit"
}
