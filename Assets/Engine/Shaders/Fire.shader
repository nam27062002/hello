Shader "Hungry Dragon/Fire Transparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_Flamespeed("Flame speed", Range(0.0, 5.0)) = 0.5
		_Flamethrower("Flame thrower", Range(0.0, 5.0)) = 0.8
		_Flamedistance("Flame distance", Range(0.0, 5.0)) = 1.5
	}

	SubShader
	{
		Tags {"Queue"="Transparent+5" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		Tags {"Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha 
		// Blend One One
		Cull Off
		ZWrite Off


		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

//			AlphaTest Less[_CutOut]

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 noiseUV : TEXCOORD1;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			}; 

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;

//			float _CutOut;
			float _Flamespeed;
			float _Flamethrower;
			float _Flamedistance;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.noiseUV = TRANSFORM_TEX(  v.uv + float2( 0, -_Time.y * _Flamespeed) , _NoiseTex);
				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 noise = tex2D( _NoiseTex, i.noiseUV ) * _Flamethrower;
				noise.g = (noise.g * i.uv.y * _Flamedistance);

//				fixed4 noise = tex2D(_NoiseTex, i.noiseUV) * _FlameThrower * 0.75;/				noise.g = noise.g * i.uv.y * _FlameThrower;

				noise.r = 0.0f;
				fixed4 col = tex2D(_MainTex, i.uv - noise.rg);
				col.a *= smoothstep(0.025, 0.15, noise.g);

//				float wAtenuation = 0.75 - abs(i.uv.x - 0.5);
//				col.a *= wAtenuation;// *wAtenuation;
//				col.a *= noise.g;
//				clip(col.a - _CutOut);
				return col * i.color;
			}
			ENDCG
		}
	}
Fallback "Mobile/VertexLit"
}
