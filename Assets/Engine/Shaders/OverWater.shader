// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/OverWater" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Tint (RGB)", color) = (1, 0, 0, 1)
	}

	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass {  
//			Tags { "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			// Blend One One
			//Blend OneMinusDstColor One
			//Cull Off
//			Lightning Off
			ZWrite Off
			Fog{ Color(0, 0, 0, 0) }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
							// make fog work
				#pragma multi_compile_fog

				#pragma multi_compile_particles
				#include "UnityCG.cginc"
//				#include "AutoLight.cginc"
//				#include "HungryDragon.cginc"

				#define CAUSTIC_ANIM_SCALE  3.0f
				#define CAUSTIC_RADIUS  0.125f

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal : NORMAL;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					float3 normal : NORMAL;
					float3 viewDir: TEXCOORD2;
					half2 uv : TEXCOORD0;
					float4 scrPos:TEXCOORD1;
					float4 color : COLOR;
				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;


				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.x * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.y * 35.0f) + _Time.y) + sin((v.vertex.y * 15.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					v.vertex.z += (sinX + sinY) * 0.002 * step(0.0, v.vertex.z) * v.color.g;

//					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);

					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

//					o.uv = TRANSFORM_TEX(o.scrPos, _MainTex);

//					o.normal = mul(UNITY_MATRIX_MVP, normalize(float3(sinX, sinY, 0.5f)));
//					o.normal = mul(unity_WorldToObject, normalize());
//					o.normal = normalize(mul(float3(sinX , sinY, 1.0f), unity_WorldToObject).xyz);
					o.normal = normalize(mul(float3(sinX, sinY, 1.0f), unity_ObjectToWorld).xyz);
//					o.normal = normalize(mul(v.normal, unity_WorldToObject));

					o.viewDir = o.vertex - _WorldSpaceCameraPos;


					o.color = v.color;
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
//					return fixed4(1.0f, 0.0f, 0.0f, 0.5f);
					float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
					float depthR = (depth - (i.scrPos.z * 1.0f));
					//				float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).x);
					//				float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).x;

					float2 anim = float2(sin(i.uv.x * CAUSTIC_ANIM_SCALE + _Time.y * 0.02f) * CAUSTIC_RADIUS,
						sin(i.uv.y * CAUSTIC_ANIM_SCALE + _Time.y * 0.04f) * CAUSTIC_RADIUS);

					//					fixed4 col = tex2D(_MainTex, 3.0f * (i.uv.xy) * (depthR * 1.02f) * _ProjectionParams.w ) * 2.0f;
					float z = depth;// i.uv.y;
									//					i.uv.y = depthR;
									//					float4 prj = float4(i.uv, z, 0.0f);
					fixed4 col = tex2D(_MainTex, 1.0f * (i.uv.xy) * (z * 2.02f) * _ProjectionParams.w) * 2.0f;
					//				fixed4 col = tex2Dproj(_MainTex, (i.scrPos) * depth * 0.1f);
					col.w = 0.0f;
					col = lerp(_Color, col, clamp(1.0 - (depth * 0.02f), 0.0f, 1.0f));

//					float3 reflectedDir = normalize(reflect(i.viewDir, i.normal));
					float3 ref = normalize(-unity_LightPosition[0].xyz + i.viewDir);
					float reflectLight = pow(clamp(dot(i.normal, ref), 0.0f, 1.0f), 10.5f) ;
//					fixed4 reflect = fixed4(0.0f, 0.0f, 0.0f, 0.0f);

					return col + fixed4(reflectLight, reflectLight, reflectLight, 0.0f);
					//				depth = (depth - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y);
					//				return fixed4(_Color.xyz, depth * _Color.w * 8.0f) + col;
					//				return tex2D(_CameraDepthTexture, i.scrPos).r;
					//				return col;


//					fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;	// Color
//					half4 depth = half4(Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);

//					float4 d = Linear01Depth(tex2D(_CameraDepthTexture, i.texcoord));
//					float depthValue = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
//					float4 d = tex2D(_CameraDepthTexture, i.texcoord);
//					fixed4 col = fixed4(d, d, d, 0.5) * i.color; // fixed4(d, d, d, 1.0);
//					fixed4 col = fixed4(i.color.ggg, 0.5); // fixed4(d, d, d, 1.0);

//					HG_APPLY_FOG(i, col);	// Fog
//					col.rgb = lerp(col.rgb, _FogColor.rgb, i.fogCoord);
//					UNITY_OPAQUE_ALPHA(col.a);	// Opaque
//					return col;
				}
			ENDCG
		}
		// Pass to render object as a shadow caster
//		Pass {
//			Name "ShadowCaster"
//			Tags { "LightMode" = "ShadowCaster" }
//
//			Fog {Mode Off}
//			ZWrite On ZTest LEqual Cull Off
//			Offset 1, 1
//
//			CGPROGRAM
//				#pragma vertex vert
//				#pragma fragment frag
//				#pragma multi_compile_shadowcaster
//				#pragma fragmentoption ARB_precision_hint_fastest
//
//				#include "UnityCG.cginc"
//				#include "AutoLight.cginc"
//
//				struct v2f { 
//					V2F_SHADOW_CASTER;
//				};
//
//				v2f vert (appdata_base v)
//				{
//					v2f o;
//					TRANSFER_SHADOW_CASTER(o)
//					return o;
//				}
//
//				float4 frag (v2f i) : COLOR
//				{
//					SHADOW_CASTER_FRAGMENT(i)
//				}
//			ENDCG
//		} //Pass
	}
}
