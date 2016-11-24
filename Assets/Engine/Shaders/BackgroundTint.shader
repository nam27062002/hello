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

//			Cull off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Tint;
			uniform float4 _Tint2;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord.xy;

				return o;
			}

			float4 frag(v2f_img i) : COLOR{
				float4 col = tex2D(_MainTex, i.uv) * lerp(_Tint, _Tint2, i.uv.y);
				return col;
//				return float4(1.0, 1.0, 0.0, 1.0);
			}
			ENDCG
		}

		Pass{
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

			Stencil
			{
				Ref 5
				Comp Equal
				Pass keep
				ZFail keep
			}
//			Cull off

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _Tint;
			uniform float4 _Tint2;

			float4 frag(v2f_img i) : COLOR{
				float4 col = tex2D(_MainTex, i.uv);// *lerp(_Tint, _Tint2, i.uv.y);
//				return float4(1.0, 0.0, 0.0, 1.0);
				return col;
			}
			ENDCG
		}

	}
	Fallback off
}
