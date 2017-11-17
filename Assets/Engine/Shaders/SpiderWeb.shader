// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Spider web"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}

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
				fixed3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				fixed3 normal : NORMAL;
				fixed3 viewDir : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _TintColor;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;

				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.normal = UnityObjectToWorldNormal(v.normal);
				
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				half4 prev = i.color * tex2D(_MainTex, i.texcoord);
				float n = dot(i.normal, i.viewDir);
				float3 tn = normalize(i.viewDir - (i.normal * n));
//				float dd = abs(dot(d, normalize(i.viewDir.xy)));
//				float rq = (i.viewDir.x * i.viewDir.x) + (i.viewDir.y * i.viewDir.y);
				prev *= _TintColor * abs(dot(tn.xy, i.texcoord));// *rq;

				prev *= 2.0;
				return prev;
			}

			ENDCG
		}
	}
}
