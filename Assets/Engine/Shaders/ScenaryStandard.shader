// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/Scenary/Scenary Standard" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SecondTexture("Second Texture (RGB)", 2D) = "white" {}
		_Panning("Panning", Vector) = (0,0,0,0)

		_Tint("Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", Range(0.1, 5.0)) = 1.0
		_SpecularPower("Specular Power", float) = 3
		[Rotation] _SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
	
		_CutOff("Alpha cutoff threshold", Range(0.0, 1.0)) = 0.5

		_DarkenPosition("Darken position",  float) = 0.0
		_DarkenDistance("Darken distance",  float) = 20.0

		_EmissivePower("Emissive Power", Float) = 1.0
		_BlinkTimeMultiplier("Blink time multiplier", Float) = 0.0
		_WaveEmission("Emission phase", Range(0.0, 0.1)) = 0.1
		_EmissiveColor("Emissive color", Color) = (1.0, 1.0, 1.0, 1.0)

//		_ReflectionColor("Reflection color", Color) = (1.0, 1.0, 0.0, 1.0)
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 1.0
		_ReflectionMap("Reflection map", 2D) = "white" {}

		_LightmapContrastIntensity("Lightmap contrast intensity", Range(0.0, 5.0)) = 1.3
		_LightmapContrastMargin("Lightmap contrast margin", Range(0.0, 6.0)) = 0.5
		_LightmapContrastPhase("Lightmap contrast phase", Float) = 2.0

//		_StencilMask("Stencil Mask", int) = 10

		[Toggle(BLEND_TEXTURE)] _EnableBlendTexture("Enable Blend Texture", Float) = 0
		[Toggle(ADDITIVE_BLEND)] _EnableAdditiveBlend("Enable Additive Blend", Float) = 0
		[Toggle(CUSTOM_VERTEXCOLOR)] _EnableAutomaticBlend("Automatic Y blend", Float) = 0
		[Toggle(SPECULAR)] _EnableSpecular("Enable Specular Light", Float) = 0
		[Toggle(NORMALWASSPECULAR)] _EnableNormalwAsSpecular("Enable Normal.w as specular mask", Float) = 0
		[Toggle(NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0
		[Toggle(OPAQUEALPHA)] _EnableOpaqueAlpha("Enable opaque alpha", Float) = 0
		[Toggle(CUTOFF)] _EnableCutoff("Enable cut off", Float) = 0
		[Toggle(FOG)] _EnableFog("Enable fog", Float) = 1
		[Toggle(WAVE_EMISSION)] _EnableWaveEmission("Enable wave emission", Float) = 0
		[Toggle(TINT)] _EnableTint("Enable tint", Float) = 0

//		[Toggle(EMISSIVEBLINK)] _EnableEmissiveBlink("Enable emissive blink", Float) = 0
//		[Toggle(REFLECTIVE)] _EnableReflective("Enable reflective", Float) = 0
//		[Toggle(LIGHTMAPCONTRAST)] _EnableLightmapContrast("Enable lightmap contrast", Float) = 0
		[KeywordEnum(None, Overlay, Additive, Modulate)] VertexColor("Vertex color mode", Float) = 0
//		[KeywordEnum(None, Blink, Reflective, LightmapContrast)] Emissive("Emission type", Float) = 0
		[KeywordEnum(None, Blink, Reflective, Custom, Color)] Emissive("Emission type", Float) = 0
		[KeywordEnum(Texture, Color)] MainColor("Main color", Float) = 0.0
/*
		0.	Zero				Blend factor is(0, 0, 0, 0).
		1.	One					Blend factor is(1, 1, 1, 1).
		2.	DstColor			Blend factor is(Rd, Gd, Bd, Ad).
		3.	SrcColor			Blend factor is(Rs, Gs, Bs, As).
		4.	OneMinusDstColor	Blend factor is(1 - Rd, 1 - Gd, 1 - Bd, 1 - Ad).
		5.	SrcAlpha			Blend factor is(As, As, As, As).
		6.	OneMinusSrcColor	Blend factor is(1 - Rs, 1 - Gs, 1 - Bs, 1 - As).
		7.	DstAlpha			Blend factor is(Ad, Ad, Ad, Ad).
		8.	OneMinusDstAlpha	Blend factor is(1 - Ad, 1 - Ad, 1 - Ad, 1 - Ad).
		9.	SrcAlphaSaturate	Blend factor is(f, f, f, 1); where f = min(As, 1 - Ad).
		10.	OneMinusSrcAlpha	Blend factor is(1 - As, 1 - As, 1 - As, 1 - As).
*/
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull mode", Float) = 2.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1.0 //"One"
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 0.0 //"Zero"
		[Enum(Opaque, 0, Transparent, 1, CutOff, 2)] _BlendMode("Blend mode", Float) = 0.0
		[Enum(SingleSided, 0, DoubleSided, 1)] _DoubleSided("Double sided", Float) = 0.0
		[Toggle] _ZWrite("__zw", Float) = 1.0


	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry"}
		LOD 100
		
		Pass {		
//			Tags{ "LightMode" = "ForwardBase" }

			Cull [_Cull]
			Blend [_SrcBlend] [_DstBlend]
			ZWrite[_ZWrite]

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
//				#pragma multi_compile_fwdbase
				#pragma multi_compile __ BLEND_TEXTURE
				#pragma shader_feature __ ADDITIVE_BLEND
				#pragma shader_feature __ CUSTOM_VERTEXCOLOR
				#pragma multi_compile __ SPECULAR
				#pragma shader_feature __ NORMALWASSPECULAR
				#pragma shader_feature __ NORMALMAP
				#pragma shader_feature __ FOG
				#pragma shader_feature __ CUTOFF
				#pragma shader_feature __ OPAQUEALPHA
//				#pragma shader_feature __ REFLECTIVE
				#pragma shader_feature __ WAVE_EMISSION
				#pragma shader_feature _ TINT
                #pragma shader_feature  __ _ZWRITE_ON

				#pragma shader_feature VERTEXCOLOR_NONE VERTEXCOLOR_OVERLAY VERTEXCOLOR_ADDITIVE VERTEXCOLOR_MODULATE
//				#pragma shader_feature EMISSIVE_NONE EMISSIVE_BLINK EMISSIVE_REFLECTIVE EMISSIVE_LIGHTMAPCONTRAST
				#pragma shader_feature EMISSIVE_NONE EMISSIVE_BLINK EMISSIVE_REFLECTIVE EMISSIVE_CUSTOM EMISSIVE_COLOR
				#pragma shader_feature MAINCOLOR_TEXTURE MAINCOLOR_COLOR

				#pragma multi_compile __ LIGHTMAP_ON
				#pragma multi_compile __ FORCE_LIGHTMAP
				#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

				#include "HungryDragon.cginc"

				#ifdef LOW_DETAIL_ON
				#undef NORMALMAP
				#undef SPECULAR
				#endif

				#ifdef MEDIUM_DETAIL_ON
				#undef SPECULAR
				#endif

				#ifdef HI_DETAIL_ON
				#endif

				//#define TINT

				#include "scenary.cginc"
			ENDCG
		}
	}

	CustomEditor "ScenaryShaderGUI"
}
