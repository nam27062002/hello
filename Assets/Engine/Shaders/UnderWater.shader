// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/UnderWater" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ColorBack("Background color", color) = (1, 0, 0, 1)
		_ColorFront("Foreground color", color) = (1, 0, 0, 1)

		_FogFar("Fog far", float) = 1
		_FogNear("Fog near", float) = 0

		_CausticFar("Caustic far", float) = 1
		_CausticNear("Caustic near", float) = 0

		_WaveRadius("Wave radius ", Range(0.0, 1.0)) = 0.15

	}

	SubShader {
		Tags{ "Queue" = "Transparent+10" "RenderType" = "Transparent" }
		LOD 100

		Pass {  
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Fog{ Color(0, 0, 0, 0) }

			Stencil
			{
				Ref 5
				Comp NotEqual
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase

//				#pragma multi_compile_particles
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"


				#define CAUSTIC_ANIM_SCALE  2.0f
				#define CAUSTIC_RADIUS  0.125f

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 scrPos:TEXCOORD1;
					float4 color : COLOR;
				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;

//				float4 _CameraDepthTexture_TexelSize;

				float4 _MainTex_ST;
				float4 _ColorBack;
				float _WaveRadius;

				float _FogFar;
				float _FogNear;
				float _CausticFar;
				float _CausticNear;

				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * _WaveRadius * moveVertex * v.color.w;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);
//					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.uv = TRANSFORM_TEX(v.vertex.xy, _MainTex);

					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
					float depthR = (depth - i.scrPos.z);

					float lerpFog = 1.0 - clamp((depthR - _FogNear) / (_FogFar - _FogNear), 0.0, 1.0);
					float lerpCaustic = 1.0 - clamp((depthR - _CausticNear) / (_CausticFar - _CausticNear), 0.0, 1.0);

//					float2 anim = float2(sin(i.uv.x * CAUSTIC_ANIM_SCALE + _Time.y * 0.02f) * CAUSTIC_RADIUS,
//										 (sin(i.uv.y * CAUSTIC_ANIM_SCALE + _Time.y * 0.04f) * CAUSTIC_RADIUS));

					float z = depthR;// i.uv.y;
//					fixed4 col = tex2D(_MainTex, 0.7f * (i.uv.xy/* + anim*/) * (z * 14.0f) * _ProjectionParams.w) * 0.1f;
					fixed4 col = tex2D(_MainTex, (fmod(i.uv.xy + float2(3.0 * _Time.y, 0.0), 1024.0)/* + anim*/ * 0.025) * (1.0 + (z * _ProjectionParams.w * 25.0))) * 0.51f;
					col.w = 0.0f;

					fixed4 frag = lerp(fixed4(_ColorBack), fixed4(0.0, 0.0, 0.0, 0.0), lerpFog);
					frag += lerp(fixed4(0.0, 0.0, 0.0, 0.0), col, lerpCaustic);
//					float w = clamp(1.0 - ((depthR + _CausticIntensity) * 0.0125f), 0.0f, 1.0f);
//					col = lerp(fixed4(_Color) + col * w * 20.0, col, w * w);
//					col = lerp(fixed4(_ColorBack) + col * w * 0.0, col, w * w);
					return frag;
				}
			ENDCG
		}

		Pass{
			Blend SrcAlpha One
//			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Fog{ Color(0, 0, 0, 0) }

			Stencil
			{
				Ref 5
				Comp Equal
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#pragma multi_compile_particles
				#include "UnityCG.cginc"

				#define CAUSTIC_ANIM_SCALE  2.0f
				#define CAUSTIC_RADIUS  0.125f

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 uv : TEXCOORD0;
					float4 scrPos:TEXCOORD1;
					float4 color : COLOR;
				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _ColorFront;
				float _CausticFar;
				float _CausticNear;


				v2f vert(appdata_t v)
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * 0.15 * moveVertex * v.color.w;

					o.vertex = UnityObjectToClipPos(v.vertex);


					o.scrPos = ComputeScreenPos(o.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					o.color = v.color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
					float depthR = (depth - i.scrPos.z);

//					float lerpFog = 1.0 - clamp((depthR - _FogNear) / (_FogFar - _FogNear), 0.0, 1.0);
					float lerpCaustic = 1.0 - clamp((depthR - _CausticNear) / (_CausticFar - _CausticNear), 0.0, 1.0);


//					float2 anim = float2(sin(i.uv.x * CAUSTIC_ANIM_SCALE + _Time.y * 0.02f) * CAUSTIC_RADIUS,
//										(sin(i.uv.y * CAUSTIC_ANIM_SCALE + _Time.y * 0.04f) * CAUSTIC_RADIUS));

//					float z = depthR;// i.uv.y;
//					fixed4 col = (tex2D(_MainTex, 7.0f * (i.uv.xy + anim) * (z * 10.0f) * _ProjectionParams.w) * 0.7f) + _ColorFront * 0.5;
					float z = depthR + 10.0;// i.uv.y;
//					fixed4 col = tex2D(_MainTex, 0.7f * (i.uv.xy/* + anim*/) * (z * 14.0f) * _ProjectionParams.w) * 0.1f;
					fixed4 col = (tex2D(_MainTex, (fmod(i.uv.xy + float2(0.0 * _Time.y, 0.0), 1024.0)/* + anim*/ * 0.1) * (1.0 + (z * _ProjectionParams.w * 5.0))) * 0.7f) + _ColorFront * 0.5;

//					col.w = 1.0f;//dot(col.xyz, float3(0.299, 0.587, 0.114));
					return col;
				}
			ENDCG
		}

	}
}
