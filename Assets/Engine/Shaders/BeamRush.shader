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
			// make fog work
			#pragma multi_compile_fog
			
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

//			sampler2D _MainTex;
//			float4 _MainTex_ST;
			float _RayWidth;
			float _RayPhase;
			float _RaySpeed;
			float4 _RayColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv; // TRANSFORM_TEX(v.uv, _MainTex);
//				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				// sample the texture
//				fixed4 col = tex2D(_MainTex, i.uv);
				float s = (sin(i.uv.x * _RayPhase + _Time.y * _RaySpeed) + 1.0) * 0.5;
				s = 1.0 - smoothstep(0, _RayWidth, abs(s - i.uv.y));				
				return s * _RayColor;
			}
			ENDCG
		}
	}
}
