﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/TransparentAdditive"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		[HideInInspector] _VColor("Custom vertex color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex("Particle Texture", 2D) = "white" {}
//		[Toggle(CUSTOMPARTICLESYSTEM)] _EnableCustomParticleSystem("Custom Particle System", int) = 0.0
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha One
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest[_ZTest]

		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				#pragma shader_feature  __ CUSTOMPARTICLESYSTEM

				#include "UnityCG.cginc"

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

#ifdef CUSTOMPARTICLESYSTEM
				float4 _VColor;
#endif
				float4 _TintColor;

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
#ifdef CUSTOMPARTICLESYSTEM
					half4 prev = i.color * tex2D(_MainTex, i.texcoord) * _VColor;
#else
					half4 prev = i.color * tex2D(_MainTex, i.texcoord) * _TintColor;
#endif
					return prev;
				}
				ENDCG
			}
		}
	}
}