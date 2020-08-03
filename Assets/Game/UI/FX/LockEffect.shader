Shader "Hungry Dragon/PostEffect/LockEffect"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (0,1,0,1)
		_SilhouetteColor("Silhouette Color", Color) = (0,1,0,1)
	}

	SubShader{
		Pass{
//			name "Glow"
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode Off }
			Blend Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
//			uniform half4 _MainTex_TexelSize;
			uniform sampler2D _Lock;
			uniform half4 _Lock_TexelSize;

			uniform float4 _OutlineColor;
			uniform float4 _SilhouetteColor;

			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv1 : TEXCOORD1;
			};


			#define ITERATIONS 16.0

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv1 = v.texcoord;

				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
//				half4 tex = half4(_Lock_TexelSize.x, _Lock_TexelSize.y, -_Lock_TexelSize.y, 0.0) * _OutlineWidth;

				fixed4 lockTex = tex2D(_Lock, i.uv);

				fixed4 col = lerp(_OutlineColor, _SilhouetteColor, lockTex.y);

				fixed4 mainTex = tex2D(_MainTex, i.uv);
				mainTex = lerp(mainTex, col, lockTex.x);

				return mainTex;
			}
			ENDCG
		}
	}
}
