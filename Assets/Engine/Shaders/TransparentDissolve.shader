// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Transparent Dissolve"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_DissolveTex("Dissolve Texture", 2D) = "white" {}
		_EmissionSaturation("Emission Saturation", float) = 1.0
		_OpacitySaturation("Opacity Saturation", float) = 1.0

		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest", Float) = 2.0
		[Enum(Additive, 1, AlphaBlend, 10)] _BlendMode("Blend mode", Float) = 10
	}

	Category{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha [_BlendMode]
		Cull off
		Lighting Off
		ZWrite Off
		ZTest[_ZTest]

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DissolveTex;
				float4 _DissolveTex_ST;

				float _EmissionSaturation;
				float _OpacitySaturation;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 uv : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
					float dissolve : TEXCOORD1;
				};

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.uv = TRANSFORM_TEX(v.uv.xy, _MainTex);
					o.dissolve = v.uv.z;
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					float4 t1 = tex2D(_MainTex, i.uv);
					float4 t2 = tex2D(_DissolveTex, i.uv);

					float ramp = -1.0 + (i.dissolve * 2.0);
					float4 col = float4(t1.xyz * i.color.xyz * _EmissionSaturation, t1.w * (t2.x + ramp) * _OpacitySaturation * i.color.w);

					return col;
				}

				ENDCG
			}
		}
	}

}
