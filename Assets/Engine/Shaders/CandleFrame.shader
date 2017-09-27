// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Candle frame"
{
	Properties
	{
//		_MainTex ("Texture", 2D) = "white" {}
		_Tint ("Color", Color) = (1,1,1,1)
		_Radius ("Radius", Range(0.0, 1.0)) = 0.1
		_FallOff("FallOff", Range(0.0, 1.0)) = 0.1
		_Offset("Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{

//		Tags{ "ForceSupported" = "True" "RenderType" = "Overlay" }
		Tags{ "Queue" = "Transparent+50" "RenderType" = "Transparent"  "LightMode" = "ForwardBase" }


		Pass
		{
			Lighting Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			ZTest Always

			Stencil
			{
				Ref[_StencilMask]
				Comp NotEqual
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
//			#pragma multi_compile_fwdbase


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			
//			sampler2D	_MainTex;
			float4		_Tint;
			float		_Radius;
			float		_FallOff;
			float2		_Offset;

			fixed4 frag (v2f i) : SV_Target
			{
//				i.color.a = 1.0f;
//				float w = WIDTH * (1.0 - i.color.a);
//				float t = THRESHOLD * (1.0 - i.color.a);
//				float s = spiral(i.uv, w, t);
				float2 d = i.uv - _Offset;
				float dq = length(d);
//				float dq = dot(d, d);
				fixed4 col = _Tint;
				col.a *= smoothstep(_Radius, _Radius + _FallOff, dq);
				return col;
			}
			ENDCG
		}
	}
}
