Shader "Hungry Dragon/BeamRush"
{
	Properties
	{
//		_MainTex ("Texture", 2D) = "white" {}
		_RayWidth ("Ray width", Range(0.001, 2.0)) = 0.5
		_RayPhase ("Ray phase", Range(0.001, 10.0)) = 0.5
		_RaySpeed("Ray speed", Range(0.001, 10.0)) = 0.5
		_RayColor("Ray color", Color) = (0.0, 1.0, 1.0, 1.0)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
		Blend SrcAlpha One

		//		AlphaTest Greater .01
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest LEqual

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			float _RayWidth;
			float _RayPhase;
			float _RaySpeed;
			float _RayOffset;
			float4 _RayColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			#define INITOFFSET 0.5
			fixed4 frag (v2f i) : SV_Target
			{
//				float s = ((sin(_RayOffset + (i.uv.x * _RayPhase) + (_Time.y * _RaySpeed)) * (1.0 - _RayWidth)) + 1.0) * 0.5;
				float t = abs(INITOFFSET + (i.uv.x * _RayPhase) - (_Time.y * _RaySpeed));
				float m = fmod(floor(t), 2.0);
				float f = frac(t);
				float s = lerp(f, 1.0 - f, m);

				s = 1.0 - smoothstep(0, _RayWidth, abs(s - i.uv.y));
				return s * _RayColor;
			}
			ENDCG
		}
	}
}
