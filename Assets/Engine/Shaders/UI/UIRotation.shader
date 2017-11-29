// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/RotationDrawer"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		_TargetPos("Target position", Vector) = (0.0, 0.0, 0.0, 0.0)
		_SpecPow("Specular Power", float) = 2.0

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
			#pragma shader_feature  __ EMISSIVEPOWER
			#pragma shader_feature  __ AUTOMATICPANNING

			#include "UnityCG.cginc"

			#define	ALPHABLEND
//			#pragma exclude_renderers d3d11

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 tex : TEXCOORD0;
				fixed4 col : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _TintColor;
			float4 _TargetPos;
			float _SpecPow;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.col = v.color;
				o.tex = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				i.tex -= 0.5;
				
				float d = 1.0 - step(0.25, dot(i.tex, i.tex));

				fixed2 tp = _TargetPos * fixed2(1.0, -1.0);
				float dtp = length(tp);
				float dtx = length(i.tex);

				fixed3 n1 = normalize(fixed3(tp, sin(acos(dtp))));
				fixed3 n2 = normalize(fixed3(i.tex, sin(acos(dtx))));

				fixed4 prev;
				prev.xyz = _TintColor.xyz * pow(dot(n1, n2), _SpecPow * 0.25);
				prev.w = d;

				fixed2 dt = i.tex - (tp * clamp(0.5 / dtp, 0.0, 1.0));
				d = dot(dt, dt);
				d = 1.0 - step(0.01, d);

				prev = lerp(prev, fixed4(0.0, 0.0, 0.0, 1.0), d);
				return prev;
			}

			ENDCG
		}
	}
}
