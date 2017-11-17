// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Spider web"
{
	Properties
	{
		_DarkColor("Dark Color", Color) = (0.5,0.5,0.5,0.5)
		_BrightColor("Bright Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		[Toggle(ONLYTEXTURE)] _OnlyTexture("Only texture & vertex color", Float) = 0
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha One
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[_ZTest]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature ONLYTEXTURE

			#include "UnityCG.cginc"

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

			float4 _DarkColor;
			float4 _BrightColor;

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
#ifdef ONLYTEXTURE
				return tex2D(_MainTex, i.texcoord) * i.color;
#else
				float intensity = tex2D(_MainTex, i.texcoord);
				float n = dot(i.normal, i.viewDir);
				float3 tn = normalize(i.viewDir - (i.normal * n));
//				float refl = clamp(pow(dot(tn.xy, i.texcoord - 2.0), 8.0), 0.0, 1.0);
				float refl = 1.0 - clamp(pow(abs(dot(tn.xy, i.texcoord - _MainTex_ST.x * 0.5)), 1.0), 0.0, 1.0);
//				float dd = abs(dot(d, normalize(i.viewDir.xy)));
//				float rq = (i.viewDir.x * i.viewDir.x) + (i.viewDir.y * i.viewDir.y);
				float4 prev = lerp(_DarkColor * intensity, _BrightColor, refl);// *rq;
				prev.a *= i.color.r;
				return prev;
#endif
			}

			ENDCG
		}
	}
}
