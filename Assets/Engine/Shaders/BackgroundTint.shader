Shader "Hidden/Background tint effect"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Tint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_TexelOffset("Texel offset", Range(1, 10)) = 1.0
		_Focus("Focus", Range(0.0, 10.0)) = 0.5
		_LensOffset("Lens offset", Range(0.0, 1.0)) = 0.5
	}


	SubShader{
		// No culling or depth
//		Cull Off ZWrite Off ZTest Always

		Pass{

			Tags{ "RenderType" = "Opaque" }

			Stencil
			{
				Ref 5
				Comp NotEqual
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest


//			#pragma fragmentoption ARB_precision_hint_fastest 

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;

			uniform sampler2D _Depth;
			uniform float4 _Tint;
			uniform float4 _Tint2;
			sampler2D _LastCameraDepthTexture;
			float4 _LastCameraDepthTexture_TexelSize;
			float _TexelOffset;

			float _Focus;
			float _LensOffset;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
//				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex).xy;
				o.uv = v.texcoord.xy;
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
//				float4 depth = tex2D(_MainTex, i.uv);
//				float depth = Linear01Depth(tex2D(_LastCameraDepthTexture, i.uv).x);
				float2 offset = float2(_MainTex_TexelSize.x * _TexelOffset, 0.0);
//				float depthr = Linear01Depth(tex2D(_LastCameraDepthTexture, i.uv + offset.xy).x);
//				float deptht = Linear01Depth(tex2D(_LastCameraDepthTexture, i.uv + offset.yx).x);

				float dv = (_ProjectionParams.z - _ProjectionParams.y);
				float depth = tex2D(_MainTex, i.uv).a;
//				return depth;
				float depthr = tex2D(_MainTex, i.uv + offset.xy).a;
				float deptht = tex2D(_MainTex, i.uv + offset.yx).a;

				float absDepth = smoothstep(0.0, 0.0125, abs(depthr - depth) + abs(deptht - depth));

				float4 col = lerp(_Tint2, _Tint, absDepth);

				return col;
			}
			ENDCG
		}

		Pass{

			Stencil
			{
				Ref 5
				Comp Equal
				Pass keep
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
//			#pragma fragmentoption ARB_precision_hint_fastest 

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
//			uniform float4 _Tint;
//			uniform float4 _Tint2;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};


			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex).xy;
				o.uv = v.texcoord.xy;
				return o;
			}

			float4 frag(v2f i) : COLOR{
				float4 col = float4(1.0, 0.0, 0.0, 1.0); // tex2D(_MainTex, i.uv);// *lerp(_Tint, _Tint2, i.uv.y);
//				return float4(1.0, 0.0, 0.0, 1.0);
				return col;
			}
			ENDCG
		}
	}
}
