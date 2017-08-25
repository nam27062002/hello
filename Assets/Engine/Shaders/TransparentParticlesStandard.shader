// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Transparent particle standard"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		[HideInInspector] _VColor("Custom vertex color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex("Particle Texture", 2D) = "white" {}
//		[Toggle(CUSTOMPARTICLESYSTEM)] _EnableCustomParticleSystem("Custom Particle System", int) = 0.0
		[Toggle(EMISSIVEPOWER)] _EnableEmissivePower("Enable Emissive Power", int) = 0.0
		_EmissivePower("Emissive Power", Range(1.0, 4.0)) = 1.0
		[Toggle(AUTOMATICPANNING)] _EnableAutomaticPanning("Enable Automatic Panning", int) = 0.0
		_Panning("Automatic Panning", Vector) = (0.0, 0.0, 0.0, 0.0)

		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
		[Enum(Additive, 1, AlphaBlend, 10)] _BlendMode("Blend mode", Float) = 10
	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha[_BlendMode]
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
				ENDCG
			}
		}
	}
}