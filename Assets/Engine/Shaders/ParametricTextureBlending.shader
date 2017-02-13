// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Parametric Texture Blending + Lightmap And Recieve Shadow" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_SecondTexture ("Second Texture (RGB)", 2D) = "white" {}

		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 0.2
		_Specular("Specular Factor", float) = 3

		_BlendDirection("Blend direction", Vector) = (0.0, 3.0, 0.0)
		_SpecularDir("Specular Dir", Vector) = (0, 0, -1, 0)
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
				#include "Lighting.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					float3 normal : NORMAL;
					float4 tangent : TANGENT;

					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif
				}; 

				struct v2f {
					float4 vertex : SV_POSITION; 
					half2 texcoord : TEXCOORD0;
					HG_FOG_COORDS(1)
					#ifdef DYNAMIC_SHADOWS
					LIGHTING_COORDS(2,3)
					#endif
					float2 lmap : TEXCOORD4; 
					float blendValue : COLOR;

					float3 tangentWorld : TANGENT;
					float3 normalWorld : NORMAL;
					float3 binormalWorld : TEXCOORD5;
					float3 halfDir : TEXCOORD6;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _SecondTexture;

				uniform sampler2D _NormalTex;
				uniform float4 _NormalTex_ST;

				uniform float _NormalStrength;
				uniform float _Specular;

				float3 _BlendDirection;
				uniform fixed4 _SpecularDir;


				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);


					o.blendValue = 1 - dot(mul ( float4(v.normal,0), unity_WorldToObject ).xyz, _BlendDirection);
					o.blendValue = (o.blendValue * 2) - 1;

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					HG_TRANSFER_FOG(o, worldPos);	// Fog

					#ifdef DYNAMIC_SHADOWS
					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#endif
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif

					float4x4 modelMatrix = unity_ObjectToWorld;
					float4x4 modelMatrixInverse = unity_WorldToObject;
					o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
					o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity

//					fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					// Half View - See: Blinn-Phong
					float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
					//					float3 viewDirection = normalize(worldPos.xyz - _WorldSpaceCameraPos);
					// float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz); 
					float3 lightDirection = normalize(_SpecularDir.rgb);
					o.halfDir = normalize(lightDirection + viewDirection);

					return o;
				}
				 
				fixed4 frag (v2f i) : SV_Target
				{  
					fixed4 col = tex2D(_MainTex, i.texcoord);	// Color
					fixed specMask = col.w;
					fixed4 col2 = tex2D(_SecondTexture, i.texcoord);	// Color

					float l = saturate( col.a + i.blendValue );
					col = lerp( col2, col, l);

					#ifdef DYNAMIC_SHADOWS
					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col *= attenuation;
					#endif

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col);	// Fog

					float4 encodedNormal = tex2D(_NormalTex, i.texcoord);
					float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
					float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
					float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
					fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _Specular);
					 
//					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
					col = col + specular * specMask * _LightColor0.xyzz * i.blendValue
					HG_DEPTH_ALPHA(i, col)
					return col;
				}
			ENDCG
		}
	}
//	Fallback "Hungry Dragon/VertexLit"
}
