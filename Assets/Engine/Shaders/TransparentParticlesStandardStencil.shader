 // Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hungry Dragon/Particles/Transparent particles standard (Stencil)"
{
	Properties
	{
		_BasicColor("Basic Color", Color) = (0.5,0.5,0.5,0.5)
		_SaturatedColor("Saturated Color", Color) = (0.5,0.5,0.5,0.5)

		_MainTex("Particle Texture", 2D) = "white" {}
		_ColorRamp("Color Ramp", 2D) = "white" {}
		_NoiseTex("Noise Texture", 2D) = "white" {}

		_EmissionSaturation("Emission saturation", Range(0.0, 8.0)) = 1.0
		_OpacitySaturation("Opacity saturation", Range(0.0, 8.0)) = 1.0
		_ColorMultiplier("Color multiplier", Range(0.0, 8.0)) = 1.0
		_ABOffset("Alpha blend offset", Range(0.0, 8.0)) = 0.0

		[Toggle(COLOR_RAMP)] _EnableColorRamp("Enable color ramp", Float) = 0
		[Toggle(COLOR_TINT)] _EnableColorTint("Enable color tint", Float) = 0
		[Toggle(APPLY_RGB_COLOR_VERTEX)] _EnableColorVertex("Enable color vertex", Float) = 0
		[Toggle(DISSOLVE_ENABLED)] _EnableAlphaDissolve("Dissolve", Float) = 0.0
			
		_DissolveStep("DissolveStep.xy", Vector) = (0.0, 1.0, 0.0, 0.0)

		[Toggle(AUTOMATICPANNING)] _EnableAutomaticPanning("Enable Automatic Panning", int) = 0.0
		_Panning("Automatic Panning", Vector) = (0.0, 0.0, 0.0, 0.0)

		_TintColor("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_GlobalAlpha("Global alpha", float) = 1.0

		[Toggle(EMISSIVEPOWER)] _EnableEmissivePower("Enable Emissive Power", int) = 0.0
		_EmissivePower("Emissive Power", Range(1.0, 4.0)) = 1.0

		[Toggle(EXTENDED_PARTICLES)] _EnableExtendedParticles("Enable Extended Particles", int) = 0.0
		[Toggle(NOISE_TEXTURE)] _EnableNoiseTexture("Enable noise texture", int) = 0.0
		_NoisePanning("Noise Panning", Vector) = (0.0, 0.0, 0.0, 0.0)
		[Toggle(NOISE_TEXTURE_EMISSION)] _EnableNoiseTextureEmission("Enable noise texture emission", int) = 0.0
		[Toggle(NOISE_TEXTURE_ALPHA)] _EnableNoiseTextureAlpha("Enable noise texture alpha", int) = 0.0
		[Toggle(NOISE_TEXTURE_DISSOLVE)] _EnableNoiseTextureDissolve("Enable noise texture dissolve", int) = 0.0
		[Toggle(NOISEUV)] _EnableNoiseUV("Enable noise uv channel", int) = 0.0

		[Toggle(FLOWMMAP)] _EnableFlowMap("Enable flow map", int) = 0.0

		[Enum(Additive, 0, SoftAdditive, 1, AdditiveDouble, 2, AlphaBlend, 3, AdditiveAlphaBlend, 4, Premultiply, 5, Multiply, 6)] BlendMode("Blend mode", Float) = 0.0

		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 5.0 //"SrcAlpha"
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 1.0 //"One"
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull mode", Float) = 0.0

		_StencilMask("Stencil Mask", int) = 20
		[Enum(UnityEngine.Rendering.CompareFunction)] _Comp("Compare function", Float) = 3.0

	}

	Category{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend [_SrcBlend] [_DstBlend]
		Cull [_Cull]
		Lighting Off
		ZWrite Off
		ZTest[_ZTest]

		Stencil
		{
			Ref[_StencilMask]
			Comp [_Comp]
			//				Pass IncrWrap
		}


		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma shader_feature _ EMISSIVEPOWER
				#pragma shader_feature _ AUTOMATICPANNING
				#pragma shader_feature _ EXTENDED_PARTICLES
				#pragma shader_feature _ DISSOLVE_ENABLED
				#pragma shader_feature _ COLOR_RAMP
				#pragma shader_feature _ COLOR_TINT
				#pragma shader_feature _ APPLY_RGB_COLOR_VERTEX

				#pragma shader_feature _ NOISE_TEXTURE
				#pragma shader_feature _ NOISE_TEXTURE_EMISSION
				#pragma shader_feature _ NOISE_TEXTURE_ALPHA
				#pragma shader_feature _ NOISE_TEXTURE_DISSOLVE
				#pragma shader_feature _ NOISEUV

				#pragma shader_feature _ FLOWMAP

				#pragma shader_feature BLENDMODE_ADDITIVE BLENDMODE_SOFTADDITIVE BLENDMODE_ADDITIVEDOUBLE BLENDMODE_ALPHABLEND BLENDMODE_ADDITIVEALPHABLEND BLENDMODE_PREMULTIPLY BLENDMODE_MULTIPLY
//				#pragma shader_feature DISSOLVE_NONE DISSOLVE_ENABLED DISSOLVE_EXTENDED

				#include "UnityCG.cginc"
				#include "transparentparticles.cginc"
				ENDCG
			}
		}
	}
	CustomEditor "TransparentParticlesShaderGUI"
}