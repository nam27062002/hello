﻿Shader "Hungry Dragon/TransparentSoftAdditive"
{
	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		[HideInInspector] _VColor("Custom vertex color", Color) = (1.0, 1.0, 1.0, 1.0)
		[Toggle(CUSTOMPARTICLESYSTEM)] _EnableCustomParticleSystem("Custom Particle System", int) = 0.0
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend One OneMinusSrcColor
		ColorMask RGB
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest[_ZTest]
/*
		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
*/
		// ---- Fragment program cards
		SubShader
		{
			Pass
			{

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles

				#pragma shader_feature  __ CUSTOMPARTICLESYSTEM

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _TintColor;

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

				v2f vert(appdata_t v)
				{
					v2f o;

					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				fixed4 frag(v2f i) : COLOR
				{
#ifdef CUSTOMPARTICLESYSTEM
					half4 prev = i.color * tex2D(_MainTex, i.texcoord) * _VColor;
#else
					half4 prev = i.color * tex2D(_MainTex, i.texcoord);
#endif
					prev.rgb *= prev.a;
					return prev;
				}
				ENDCG
			}
		}
	}
}
