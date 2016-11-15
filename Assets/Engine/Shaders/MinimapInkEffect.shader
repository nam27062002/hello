Shader "Hidden/Minimap ink effect"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_outlineStrength("Outline Strength", Float) = 1.0
		_stepMargin("Step Margin", Float) = 0.3
	}

	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _offMultiply;
			uniform float _outlineStrength;
			uniform float _stepMargin;
			uniform float4 _MainTex_TexelSize;

			float gray(float4 col)
			{
//				return dot(col, float4(0.299, 0.587, 0.114, 0.0));
				return dot(col, float4(1.0, 1.0, 1.0, 0.0));
			}


			float4 frag(v2f_img i) : COLOR{

				float2 texelOffset = float2(_MainTex_TexelSize.x * _outlineStrength, 0.0);

				float4 col = tex2D(_MainTex, i.uv);
				float d1 = gray(col);
				float d2 = gray(tex2D(_MainTex, i.uv + texelOffset.xy));
				float d3 = gray(tex2D(_MainTex, i.uv + texelOffset.yx));

				float intensity = abs(d2 - d1) + abs(d3 - d1);
//				float intensity = (d2 - d1) * (d3 - d1);

//				float4 c = tex2D(_MainTex, i.uv + off * _bwBlend);

//				float lum = c.r*.3 + c.g*.59 + c.b*.11;
//				float4 bw = float4(lum, lum, lum, 1.0);

//				float4 result = c;
				float4 result = lerp(col, float4(0.0, 0.0, 0.0, 1.0), smoothstep(0.0, _stepMargin, intensity) );
				return result;
			}
			ENDCG
		}
	}
}
