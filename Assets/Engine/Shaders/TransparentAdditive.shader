// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/TransparentAdditive"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		[Toggle(EMISSIVEPOWER)] _EnableEmissivePower("Enable Emissive Power", int) = 0.0
		_EmissivePower("Emissive Power", Range(1.0, 4.0)) = 1.0
		[Toggle(AUTOMATICPANNING)] _EnableAutomaticPanning("Enable Automatic Panning", int) = 0.0
		_Panning("Automatic Panning", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0

	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha One
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[_ZTest]

		SubShader
		{
			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				#pragma shader_feature  __ CUSTOMPARTICLESYSTEM
				#pragma shader_feature  __ EMISSIVEPOWER
				#pragma shader_feature  __ AUTOMATICPANNING

				#include "UnityCG.cginc"
				#include "transparentparticles.cginc"
/*
				sampler2D _MainTex;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				float4 _MainTex_ST;

				float4 _TintColor;

#ifdef EMISSIVEPOWER
				float _EmissivePower;
#endif
				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
					half4 prev = i.color * tex2D(_MainTex, i.texcoord) * _TintColor;

#ifdef EMISSIVEPOWER
					return prev * _EmissivePower;
#else
					return prev;
#endif
				}
*/
				ENDCG
			}
		}
	}
}