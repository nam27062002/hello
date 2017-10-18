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
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", Range(0.1, 5.0)) = 1.0
		_SpecularPower("Specular Power", float) = 3
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
	
		_CutOff("Alpha cutoff threshold", Range(0.0, 1.0)) = 0.5

		_DarkenPosition("Darken position",  float) = 0.0
		_DarkenDistance("Darken distance",  float) = 20.0

		_EmissivePower("Emissive Power", Float) = 1.0
		_BlinkTimeMultiplier("Blink time multiplier", Float) = 2.0

		_ReflectionColor("Reflection color", Color) = (1.0, 1.0, 0.0, 1.0)
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 1.0
		_ReflectionMap("Reflection map", 2D) = "white" {}

		_LightmapContrastIntensity("Lightmap contrast intensity", Range(0.0, 5.0)) = 1.3
		_LightmapContrastMargin("Lightmap contrast margin", Range(0.0, 6.0)) = 0.5

//		_StencilMask("Stencil Mask", int) = 10

		[Toggle(BLEND_TEXTURE)] _EnableBlendTexture("Enable Blend Texture", Float) = 0
		[Toggle(CUSTOM_VERTEXCOLOR)] _EnableAutomaticBlend("Automatic Y blend", Float) = 0
		[Toggle(SPECULAR)] _EnableSpecular("Enable Specular Light", Float) = 0
		[Toggle(NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0
		[Toggle(OPAQUEALPHA)] _EnableOpaqueAlpha("Enable opaque alpha", Float) = 1
		[Toggle(CUTOFF)] _EnableCutoff("Enable cut off", Float) = 0
		[Toggle(FOG)] _EnableFog("Enable fog", Float) = 1
//		[Toggle(EMISSIVEBLINK)] _EnableEmissiveBlink("Enable emissive blink", Float) = 0
//		[Toggle(REFLECTIVE)] _EnableReflective("Enable reflective", Float) = 0
//		[Toggle(LIGHTMAPCONTRAST)] _EnableLightmapContrast("Enable lightmap contrast", Float) = 0
		[KeywordEnum(None, Overlay, Additive, Modulate)] VertexColor("Vertex color mode", Float) = 0
		[KeywordEnum(None, Blink, Reflective, LightmapContrast)] Emissive("Emission type", Float) = 0
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
		[HideInInspector] _ZWrite("__zw", Float) = 1.0

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
				#pragma multi_compile __ CUSTOM_VERTEXCOLOR
				#pragma multi_compile __ SPECULAR
				#pragma multi_compile __ NORMALMAP
				#pragma multi_compile __ FOG
				#pragma multi_compile __ CUTOFF
				#pragma multi_compile __ OPAQUEALFA
				#pragma multi_compile __ REFLECTIVE
				#pragma multi_compile __ LIGHTMAP_ON

				
				#pragma multi_compile VERTEXCOLOR_NONE VERTEXCOLOR_OVERLAY VERTEXCOLOR_ADDITIVE VERTEXCOLOR_MODULATE

				#pragma multi_compile EMISSIVE_NONE EMISSIVE_BLINK EMISSIVE_REFLECTIVE EMISSIVE_LIGHTMAPCONTRAST

				#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON


				#define HG_SCENARY

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

				#include "HungryDragon.cginc"

//				#define FOG
//				#define OPAQUEALPHA
//				#define BLEND_TEXTURE
//				#define CUSTOM_VERTEXCOLOR

				#if LOW_DETAIL_ON
				#undef NORMALMAP
				#undef SPECULAR
				#endif

				#if MEDIUM_DETAIL_ON
				#undef SPECULAR
				#endif

				#if HI_DETAIL_ON
				#endif

//				#define FOG
//				#define OPAQUEALPHA


				#include "scenary.cginc"
			ENDCG
		}
	}

	CustomEditor "ScenaryShaderGUI"
}
