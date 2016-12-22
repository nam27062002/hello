// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Lightmap And Recieve Shadow + Normal Map" 
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
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
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase
							
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"
				#include "Lighting.cginc"


				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
//					float4 color : COLOR;
					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
//					float4 color : COLOR;
					HG_FOG_COORDS(1)
					#if LIGHTMAP_ON
					float2 lmap : TEXCOORD2;
					#endif
					half2 texcoord2 : TEXCOORD3;
					float3 tangentWorld : TANGENT;
					float3 normalWorld : NORMAL;
					float3 binormalWorld : TEXCOORD4;
					float3 halfDir : TEXCOORD5;
				};

				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform sampler2D _NormalTex;
				uniform float4 _NormalTex_ST;
				uniform float _NormalStrength;
				uniform float _Specular;
				uniform fixed4 _SpecularDir;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.texcoord2 = TRANSFORM_TEX(v.texcoord, _NormalTex);
//					o.color = v.color;
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif
																							// To calculate tangent world
					float4x4 modelMatrix = unity_ObjectToWorld;
					float4x4 modelMatrixInverse = unity_WorldToObject;
					o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
					o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity

					fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
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
					float specMask = col.a;

					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col);	// Fog

					float4 encodedNormal = tex2D(_NormalTex, _NormalTex_ST.xy * i.texcoord2 + _NormalTex_ST.zw);
					float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
					float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
					float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
					fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _Specular);

					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
					return col + (specular * specMask * _LightColor0);
				}
			ENDCG
		}
	}
	Fallback "Mobile/VertexLit"
}
