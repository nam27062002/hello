// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Particles/Transparent Additive"
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
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
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
//				#pragma multi_compile_particles
				#pragma shader_feature  __ EMISSIVEPOWER
				#pragma shader_feature  __ AUTOMATICPANNING

				#include "UnityCG.cginc"
				#include "transparentparticles.cginc"
				ENDCG
			}
		}
	}
}