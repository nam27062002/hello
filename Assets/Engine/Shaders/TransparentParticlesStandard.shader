// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Particles/Transparent particles standard"
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
		_ABOffset("Alpha blend offset", Range(0.0, 8.0)) = 0.0

//		[Toggle(DISSOLVE)] _EnableDissolve("Enable alpha dissolve", Float) = 0
		[Toggle(COLOR_RAMP)] _EnableColorRamp("Enable color ramp", Float) = 0
		[Toggle(APPLY_RGB_COLOR_VERTEX)] _EnableColorVertex("Enable color vertex", Float) = 0

		_DissolveStep("DissolveStep.xy", Vector) = (0.0, 1.0, 0.0, 0.0)

		[Toggle(AUTOMATICPANNING)] _EnableAutomaticPanning("Enable Automatic Panning", int) = 0.0
		_Panning("Automatic Panning", Vector) = (0.0, 0.0, 0.0, 0.0)

		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)

		[Toggle(EMISSIVEPOWER)] _EnableEmissivePower("Enable Emissive Power", int) = 0.0
		_EmissivePower("Emissive Power", Range(1.0, 4.0)) = 1.0

		[Toggle(EXTENDED_PARTICLES)] _EnableExtendedParticles("Enable Extended Particles", int) = 0.0

		[Enum(Additive, 0, SoftAdditive, 1, AdditiveDouble, 2, AlphaBlend, 3, AdditiveAlphaBlend, 4, Premultiply, 5)] BlendMode("Blend mode", Float) = 0.0
		[Enum(None, 0, Enabled, 1, Extended, 2)] Dissolve("Dissolve", Float) = 0.0

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 5.0 //"SrcAlpha"
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 1.0 //"One"
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
	}

	Category{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend [_SrcBlend] [_DstBlend]
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

				#pragma shader_feature _ EMISSIVEPOWER
				#pragma shader_feature _ DISSOLVE
				#pragma shader_feature _ COLOR_RAMP
				#pragma shader_feature _ APPLY_RGB_COLOR_VERTEX
				#pragma shader_feature _ AUTOMATICPANNING
				#pragma shader_feature BLENDMODE_ADDITIVE BLENDMODE_SOFTADDITIVE BLENDMODE_ADDITIVEDOUBLE BLENDMODE_ALPHABLEND BLENDMODE_ADDITIVEALPHABLEND BLENDMODE_PREMULTIPLY
				#pragma shader_feature DISSOLVE_NONE DISSOLVE_ENABLED DISSOLVE_EXTENDED
				#pragma shader_feature _ EXTENDED_PARTICLES

				#include "UnityCG.cginc"
				#include "transparentparticles.cginc"
				ENDCG
			}
		}
	}
	CustomEditor "TransparentParticlesShaderGUI"
}