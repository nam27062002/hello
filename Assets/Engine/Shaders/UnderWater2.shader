// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/UnderWater2" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_PerspectivePower("Perspective power", float) = 0.5
		_ColorBack("Background color", color) = (1, 0, 0, 1)
		_ColorFront("Foreground color", color) = (1, 0, 0, 1)

		_FogFar("Fog far", float) = 1
		_FogNear("Fog near", float) = 0

		_CausticFar("Caustic far", float) = 1
		_CausticNear("Caustic near", float) = 0

		_WaveRadius("Wave radius ", Range(0.0, 1.0)) = 0.15

		_CausticSpeed("Caustic speed", float) = 2.0

		_StencilMask("Stencil Mask", int) = 10
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
				Ref [_StencilMask]
				Comp NotEqual
				Pass DecrWrap//keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma shader_feature _WATER
//				#pragma multi_compile_fog
//				#pragma multi_compile_fwdbase

//				#pragma multi_compile_particles
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"


				#define CAUSTIC_ANIM_SCALE  2.0f
				#define CAUSTIC_RADIUS 0.125f

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


				sampler2D _MainTex;
//				sampler2D _CameraDepthTexture;

//				float4 _CameraDepthTexture_TexelSize;

				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;
				float4 _ColorBack;
				float4 _ColorFront;
				float _WaveRadius;
				float _PerspectivePower;

				float _FogFar;
				float _FogNear;
				float _CausticFar;
				float _CausticNear;

				float _CausticSpeed;

				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * _WaveRadius * moveVertex * (1.0 - v.color.w);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);
//					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//					o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);
					o.uv = v.uv.xy;

					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
//					return i.uv.y;
//					return fixed4(0.0, 1.0, 0.0, 1.0);
					float depth =  pow(1.0 - abs(i.uv.y - 1.0), _PerspectivePower);
	//				float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
//					float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
					//float depth = clamp(1.0 - pow(abs((i.uv.y * _SurfaceScale) - _SurfaceOffset), 2.0), 0.0, 1.0);
//				return depth;
					fixed4 frag = lerp(fixed4(_ColorBack), fixed4(_ColorFront), 1.0 - depth);

					return frag;
					float2 uv = (i.uv.xy + float2((_Time.y * _CausticSpeed), 0.0)) * float2(1.0 + depth, 1.0);

					//float intensity = tex2D(_MainTex, frac(uv2 * float2(1.0 / persp, 1.0))).x;
					fixed4 col = tex2D(_MainTex, uv * float2(1.0 / depth, 1.0 / depth) * 0.001).x;


//					fixed4 col = tex2D(_MainTex, i.uv.xy * float2(1.0 + depth, 1.0 + depth) * 8.0);
					col = lerp(col, fixed4(0.0, 0.0, 0.0, 0.0), depth);
					frag += col;


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
				Ref [_StencilMask]
				Comp Equal
				Pass IncrWrap
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest

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


				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;
				float4 _ColorBack;
				float4 _ColorFront;
				float _PerspectivePower;

				float _CausticFar;
				float _CausticNear;
				float _WaveRadius;


				v2f vert(appdata_t v)
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * _WaveRadius * moveVertex * v.color.w;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);
//					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.uv = v.uv;

					o.color = v.color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
						float depth = pow(1.0 - abs(i.uv.y - 1.0), _PerspectivePower);
					//				float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
					//					float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1.0f;
					//float depth = clamp(1.0 - pow(abs((i.uv.y * _SurfaceScale) - _SurfaceOffset), 2.0), 0.0, 1.0);
					//				return depth;
					fixed4 frag = lerp(fixed4(_ColorBack), fixed4(_ColorFront), 1.0 - depth);

					return frag;

//					float lerpFog = 1.0 - clamp((depthR - _FogNear) / (_FogFar - _FogNear), 0.0, 1.0);
//					float lerpCaustic = 1.0 - clamp((depthR - _CausticNear) / (_CausticFar - _CausticNear), 0.0, 1.0);


				}
			ENDCG
		}

	}
}
