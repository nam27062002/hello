Shader "Hungry Dragon/Fire Transparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		_Flamespeed("Flame speed", Range(0.0, 2.0)) = 0.5
		_Flamethrower("Flame thrower", Range(0.0, 5.0)) = 0.8
		_Flamedistance("Flame distance", Range(0.0, 5.0)) = 1.5
			
		_StencilMask("Stencil Mask", int) = 10
	}

	SubShader
	{
//		Tags{ "Queue" = "Transparent" "RenderType" = "ExcludeAdditive" }
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off
		ZWrite Off

		Stencil
		{
			Ref [_StencilMask]
			Comp always
			Pass Replace
			ZFail keep
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest

			
			#include "UnityCG.cginc"

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

			float _Flamespeed;
			float _Flamethrower;
			float _Flamedistance;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.noiseUV = TRANSFORM_TEX( v.uv + float2( 0, -_Time.y * _Flamespeed) , _NoiseTex );
				return o;
			}
			

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 noise = tex2D( _NoiseTex, i.noiseUV ) * _Flamethrower;
				noise += tex2D(_NoiseTex, i.noiseUV - float2(0.0, _Time.y * _Flamespeed * 2.0)) * _Flamethrower;

				noise *= 0.5;
//				clip(noise.r - 0.25);
				noise.g = (noise.g * i.uv.y * _Flamedistance);

				noise.r = 0.0f;
				fixed4 col = tex2D(_MainTex, i.uv - noise.rg);
				col.a *= smoothstep(0.025, 0.15, noise.g);
				clip(col.a - 0.1);

				return col * i.color;
			}
			ENDCG
		}
	}
}
