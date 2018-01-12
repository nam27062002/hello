// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Candle frame"
{
	Properties
	{
		_Tint ("Color", Color) = (1,1,1,1)
		_Tint2 ("Color2", Color) = (1,1,1,1)
		_Radius ("Radius", Range(0.0, 1.0)) = 0.1
		_FallOff("FallOff", Range(0.0, 1.0)) = 0.1
		[HideInInspector]_Offset("Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{

//		Tags{ "ForceSupported" = "True" "RenderType" = "Overlay" }
		Tags{ "Queue" = "Transparent+50" "RenderType" = "Transparent" }

		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			Stencil
			{
				Ref[_StencilMask]
				Comp NotEqual
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float4		_Tint;
			float		_Radius;
			float		_FallOff;
			float2		_Offset;

			fixed4 frag (v2f i) : SV_Target
			{
				float2 d = i.uv - _Offset;
				float dq = length(d);
				fixed4 col = _Tint;
				col.a *= smoothstep(_Radius, _Radius + _FallOff, dq);
				return col;
			}
			ENDCG
		}

		Pass
		{

			Stencil
			{
				Ref[_StencilMask]
				Comp Equal
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4		_Tint2;
			float		_Radius;
			float		_FallOff;
			float2		_Offset;

			fixed4 frag(v2f i) : SV_Target
			{
				float2 d = i.uv - _Offset;
				float dq = length(d);
				fixed4 col = _Tint2;
				col.a *= smoothstep(_Radius, _Radius + _FallOff, dq);
				return col;
			}
			ENDCG
		}
	}
}
