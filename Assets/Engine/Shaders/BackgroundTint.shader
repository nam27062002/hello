Shader "Hidden/Background tint effect"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Tint("Tint", Color) = (1.0, 1.0, 1.0, 1.0)
		_Tint2("Tint2", Color) = (1.0, 1.0, 1.0, 1.0)
	}


	SubShader{
		Pass{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

			Stencil
			{
				Ref 5
				Comp NotEqual
				Pass keep
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _Tint;
			uniform float4 _Tint2;

			float4 frag(v2f_img i) : COLOR{
				float4 col = tex2D(_MainTex, i.uv) * lerp(_Tint, _Tint2, i.uv.y);
				return col;
			}
			ENDCG
		}

		Pass{
			Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }

			Stencil
			{
				Ref 5
				Comp Equal
				Pass keep
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _Tint;
			uniform float4 _Tint2;

			float4 frag(v2f_img i) : COLOR{
				float4 col = tex2D(_MainTex, i.uv);// *lerp(_Tint, _Tint2, i.uv.y);
				return col;
			}
			ENDCG
		}

	}
	Fallback off
}
