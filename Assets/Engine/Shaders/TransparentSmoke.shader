// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/TransparentAlphaBlend smoke"
{
	Properties
	{
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_MaskTex("Mask Texture", 2D) = "white" {}
		_TintColor("Smoke Color 1", Color) = (0.5,0.5,0.5,0.5)
		_Speed("SpeedXY1.xy SpeedXY2.zw", Vector) = (0.0, 0.0, 0.0, 0.0)
		_SmoothVal("SmoothVal.xy Emission.w", Vector) = (0,0,0,0)
		_Offset("Offset", Range(0.0, 0.49)) = 0.3

		//		[Toggle(CUSTOMPARTICLESYSTEM)] _EnableCustomParticleSystem("Custom Particle System", int) = 0.0
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest", Float) = 2.0
		[Enum(Additive, 1, AlphaBlend, 10)] _BlendMode("Blend mode", Float) = 10
	}

	Category{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha [_BlendMode]
		Cull back
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest[_ZTest]

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _NoiseTex;
				float4 _NoiseTex_ST;
				sampler2D _MaskTex;
				float4 _MaskTex_ST;
				//fixed4 _TintColor;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
				};

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
				};

				float4 _TintColor;
				float4 _Speed;
				float4 _SmoothVal;
				float _Offset;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.uv = TRANSFORM_TEX(v.uv, _NoiseTex);
					o.uv2 = TRANSFORM_TEX(v.uv2, _MaskTex);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					float2 uv_Noise = i.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
					float4 t1 = tex2D(_NoiseTex, uv_Noise + (_Speed.xy * _Time.y));
					float4 t2 = tex2D(_NoiseTex, uv_Noise + (_Speed.zw * _Time.y));

					float ramp =(uv_Noise.y - _Offset) / (0.5 - _Offset);

					float temp_output_73_0 = smoothstep(_SmoothVal.x, _SmoothVal.y, (((t1.r * uv_Noise.y) + (t2.g * uv_Noise.y)) * t1.r * t2.g * ramp * uv_Noise.y));
//					float4 lerpResult65 = lerp(_ColorA_Instance, _ColorB_Instance, temp_output_73_0);

					return _TintColor * temp_output_73_0 * tex2D(_MaskTex, i.uv2).r * _SmoothVal.w;
				}

				ENDCG
			}
		}
	}

}
