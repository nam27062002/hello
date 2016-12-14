// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Lightmap And Recieve Shadow with Normal Map and overlay (On Line Decorations)"
{
	Properties 
	{
		_MainTex ("Base (RGBA)", 2D) = "white" {}
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_Specular("Specular Factor", float) = 3
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase" }
		LOD 100
		
		Pass {		

			Stencil
			{
				Ref 4
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"
				#include "HungryDragon.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
					#if LIGHTMAP_ON
					float4 texcoord1 : TEXCOORD1;
					#endif

					float4 color : COLOR;
					float3 normal : NORMAL;
					float4 tangent : TANGENT;
				}; 

				struct v2f {
					float4 pos : SV_POSITION;
					half2 texcoord : TEXCOORD0;
					HG_FOG_COORDS(1)
					#if LIGHTMAP_ON
					float2 lmap : TEXCOORD4;
					#endif
					float4 color : COLOR; 
					float3 tangentWorld : TANGENT;
					float3 normalWorld : TEXCOORD5;
					float3 binormalWorld : TEXCOORD6;
					float3 halfDir : TEXCOORD2;
				};

				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float4 _MainTex_TexelSize;
				uniform sampler2D _NormalTex;
				uniform float4 _NormalTex_ST;
				uniform float _NormalStrength;
				uniform float _Specular;

				HG_FOG_VARIABLES
				
				v2f vert (appdata_t v) 
				{
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
					#if LIGHTMAP_ON
					o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
					#endif
					o.color = v.color;

					// To calculate tangent world
					float4x4 modelMatrix = unity_ObjectToWorld;
					float4x4 modelMatrixInverse = unity_WorldToObject;
					o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
					o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
					o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity

					fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					// Half View - See: Blinn-Phong
					float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
					float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					o.halfDir = normalize(lightDirection + viewDirection);

					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;	// Color
					fixed4 one = fixed4(1, 1, 1, 1);
					float specMask = col.w;
					col.w = 1.0;
					// col = one- (one-col) * (1-(i.color-fixed4(0.5,0.5,0.5,0.5)));	// Soft Light
					col = one - 2 * (one - i.color) * (one - col);	// Overlay

					float4 encodedNormal = tex2D(_NormalTex, _NormalTex_ST.xy * i.texcoord + _NormalTex_ST.zw);
					float3 localCoords = float3(2.0 * encodedNormal.xz - float2(1.0, 1.0), 1.0 / _NormalStrength);
					float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
					float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
					fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _Specular);

					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
					col *= attenuation;
					 
					#if LIGHTMAP_ON
					fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
					col.rgb *= lm;
					#endif

					HG_APPLY_FOG(i, col);	// Fog

					UNITY_OPAQUE_ALPHA(col.a);	// Opaque

					// col = fixed4(1,1,1,1) * i.fogCoord;
					return col + (specular * specMask * _LightColor0);
				}
			ENDCG
		}
	}

	Fallback "Mobile/VertexLit"
}
