// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Particles/Transparent Signal"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		_IDSignal("ID signal", float) = 0.0

		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[_ZTest]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
//			#pragma shader_feature  __ EMISSIVEPOWER
//			#pragma shader_feature  __ AUTOMATICPANNING

			#include "UnityCG.cginc"

			#define	ALPHABLEND

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _TintColor;

			float _IDSignal;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;

				float c = fmod(_IDSignal, 3.0);
				float f = floor(_IDSignal / 3.0);

				float2 off = float2(c * 0.33333, -f * 0.125);
				o.texcoord = (TRANSFORM_TEX((v.texcoord), _MainTex)) + off;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 prev = i.color * tex2D(_MainTex, i.texcoord);

				prev *= _TintColor;

//#if defined(SOFTADDITIVE)
//				prev.rgb *= prev.a;
//#elif defined(ALPHABLEND)
//				prev *= 2.0;
//#endif

				return prev;
			}
			ENDCG
		}
	}
}
