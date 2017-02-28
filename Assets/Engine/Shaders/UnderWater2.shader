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
		_PerspectivePower("Perspective power", Range(1.0, 10.0)) = 6.0
		_ColorBack("Background color", color) = (1, 0, 0, 1)
		_ColorFront("Foreground color", color) = (1, 0, 0, 1)

		_WaveRadius("Wave radius ", Range(0.0, 1.0)) = 0.15

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
//				Pass DecrWrap//keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest

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
				float4 _MainTex_ST;
				float4 _MainTex_TexelSize;
				float4 _ColorBack;
				float4 _ColorFront;
				float _WaveRadius;
				float _PerspectivePower;


				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * _WaveRadius * moveVertex * (1.0 - v.color.w);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);
					o.uv = v.uv.xy;

					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					float depth =  pow(1.0 - abs(i.uv.y - 1.0), _PerspectivePower);
					fixed4 frag = lerp(fixed4(_ColorBack), fixed4(_ColorFront), 1.0 - depth);

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
//				Pass IncrWrap
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

				float _WaveRadius;

				v2f vert(appdata_t v)
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * _WaveRadius * moveVertex * (1.0 - v.color.w);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);
					o.uv = v.uv;

					o.color = v.color;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					float depth = pow(1.0 - abs(i.uv.y - 1.0), _PerspectivePower);
					fixed4 frag = lerp(fixed4(_ColorBack), fixed4(_ColorFront), 1.0 - depth);

					return frag;
				}
			ENDCG
		}

	}
}
