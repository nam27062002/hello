Shader "Hungry Dragon/PostEffect/LockEffectDilate"
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_DilateDecay("Dilate decay", Float) = 1.0
	}

	SubShader{

		pass { // Dilate mask texture
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode Off }
			Blend Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half _DilateDecay;

			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half2 uv2[4] : TEXCOORD1;
			};

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				o.uv2[0] = v.texcoord + _MainTex_TexelSize.xy * half2(1.0, 1.0) * 1.0;
				o.uv2[1] = v.texcoord + _MainTex_TexelSize.xy * half2(-1.0, -1.0) * 1.0;
				o.uv2[2] = v.texcoord + _MainTex_TexelSize.xy * half2(1.0, -1.0) * 1.0;
				o.uv2[3] = v.texcoord + _MainTex_TexelSize.xy * half2(-1.0, 1.0) * 1.0;
/*
				o.uv2[4] = v.texcoord + _MainTex_TexelSize.xy * half2(0.0, 1.0) * 2.0;
				o.uv2[5] = v.texcoord + _MainTex_TexelSize.xy * half2(1.0, 0.0) * 2.0;
				o.uv2[6] = v.texcoord + _MainTex_TexelSize.xy * half2(0.0, -1.0) * 2.0;
				o.uv2[7] = v.texcoord + _MainTex_TexelSize.xy * half2(-1.0, 0.0) * 2.0;
*/
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 current = tex2D(_MainTex, i.uv);
				fixed4 neighbors = current;

				neighbors = max(tex2D(_MainTex, i.uv2[0]), neighbors);
				neighbors = max(tex2D(_MainTex, i.uv2[1]), neighbors);
				neighbors = max(tex2D(_MainTex, i.uv2[2]), neighbors);
				neighbors = max(tex2D(_MainTex, i.uv2[3]), neighbors);
/*
				neighbors = max(tex2D(_MainTex, i.uv2[4]), neighbors);
				neighbors = max(tex2D(_MainTex, i.uv2[5]), neighbors);
				neighbors = max(tex2D(_MainTex, i.uv2[6]), neighbors);
				neighbors = max(tex2D(_MainTex, i.uv2[7]), neighbors);
*/
/*
				neighbors += tex2D(_MainTex, i.uv2[0]);
				neighbors += tex2D(_MainTex, i.uv2[1]);
				neighbors += tex2D(_MainTex, i.uv2[2]);
				neighbors += tex2D(_MainTex, i.uv2[3]);
				neighbors /= 4.0;
*/

				fixed d = neighbors.x - current.x;
				current.x += d * _DilateDecay;

				return current;
			}

			ENDCG
		}
	}
}