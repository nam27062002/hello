Shader "Hidden/FogBlend"
{
	Properties{
		_MainTex("Main (RGB)", 2D) = "white" {}
		_OriginalTex("Base (RGB)", 2D) = "white" {}
		_LerpValue("Lerp", Float) = 0.0
	}

	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _OriginalTex;
			uniform float _LerpValue;


			float4 frag(v2f_img i) : COLOR{
				return lerp(tex2D(_OriginalTex,i.uv),tex2D(_MainTex,i.uv), _LerpValue);
			}
			ENDCG
		}
	}
}
