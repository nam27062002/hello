// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Transparent Additive AlphaBlend 2"
{
	Properties
	{
		_BasicColor("Basic Color", Color) = (0.5,0.5,0.5,0.5)
		_SaturatedColor("Saturated Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		_ColorRamp("Color Ramp", 2D) = "white" {}
		_EmissionSaturation("Emission saturation", Range(0.0, 8.0)) = 1.0
		_OpacitySaturation("Opacity saturation", Range(0.0, 8.0)) = 1.0
		_ColorMultiplier("Color multiplier", Range(0.0, 8.0)) = 1.0
		[Toggle(DISSOLVE)] _EnableDissolve("Enable alpha dissolve", Float) = 0
		[Toggle(COLOR_RAMP)] _EnableColorRamp("Enable color ramp", Float) = 0
		[Toggle(APPLY_RGB_COLOR_VERTEX)] _EnableColorVertex("Enable color vertex", Float) = 0

		_DissolveStep("DissolveStep.xy", Vector) = (0.0, 1.0, 0.0, 0.0)
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
//		[Enum(Additive, 1, AlphaBlend, 10)] _BlendMode("Blend mode", Float) = 10
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		//		Blend SrcAlpha One // Additive blending
//		Blend SrcAlpha OneMinusSrcAlpha // Alpha blend
//		Blend SrcAlpha[_BlendMode]
		Blend One OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[_ZTest]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ DISSOLVE
			#pragma multi_compile _ COLOR_RAMP
			#pragma multi_compile _ APPLY_RGB_COLOR_VERTEX
			//			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 particledata : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

#ifdef COLOR_RAMP
			sampler2D _ColorRamp;
			float4 _ColorRamp_ST;
#endif

			float4 _BasicColor;
			float4 _SaturatedColor;
			float _EmissionSaturation;
			float _OpacitySaturation;

#ifdef DISSOLVE
			float4 _DissolveStep;
#endif
			float _ColorMultiplier;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				o.particledata = v.texcoord.zw;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				float3 tex = tex2D(_MainTex, i.texcoord);
				float4 col;

				float ramp = -1.0 + (i.particledata.x * 2.0);

#ifdef APPLY_RGB_COLOR_VERTEX
				float4 vcolor = i.color;
#else
				float4 vcolor = float4(1.0, 1.0, 1.0, i.color.w);
#endif

#ifdef DISSOLVE
				col.a = clamp(tex.g * smoothstep(_DissolveStep.x, _DissolveStep.y, tex.b + ramp) * _OpacitySaturation * vcolor.w, 0.0, 1.0);
#else
				col.a = clamp(tex.g * _OpacitySaturation * vcolor.w, 0.0, 1.0);
#endif

				float lerpValue = clamp(tex.r * i.particledata.y * _ColorMultiplier, 0.0, 1.0);
#if COLOR_RAMP
				col.xyz = tex2D(_ColorRamp, float2((1.0 - lerpValue), 0.0)) * vcolor.xyz * col.a * _EmissionSaturation;
#else
				col.xyz = lerp(_BasicColor.xyz * vcolor.xyz, _SaturatedColor, lerpValue) * col.a * _EmissionSaturation;
#endif
				return col;
			}
			ENDCG
		}
	}
}
