// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon/Dragon standard" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_DetailTex("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular
		_BumpMap ("Normal Map (RGB)", 2D) = "white" {}
		_NormalStrenght("Normal Strenght", Range(0.1, 5.0)) = 1.0

		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_Tint ("Color Multiply", Color) = (1,1,1,1)
		_ColorAdd ("Color Add", Color) = (0,0,0,0)

		_InnerLightAdd ("Inner Light Add", float) = 0.0
		_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

		_InnerLightWavePhase("Inner Light Wave Phase", float) = 1.0
		_InnerLightWaveSpeed("Inner Light Wave Speed", float) = 1.0

//		_AmbientAdd("Ambient Add", Color) = (0,0,0,0)

		_Fresnel("Fresnel factor", Range(0, 10)) = 1.5
		_FresnelColor("Fresnel Color", Color) = (1,1,1,1)

		_SpecExponent("Specular Exponent", float) = 1.0
		[Rotation] _SecondLightDir("Second Light direction", Vector) = (0,0,-1,0)
		_SecondLightColor("Second Light color", Color) = (0.0, 0.0, 0.0, 0.0)

		_ReflectionMap("Reflection Map", Cube) = "white" {}
		_ReflectionAmount("Reflection amount", Range(0.0, 1.0)) = 0.0
		_ReflectionColor("Reflection color", Color) = (1.0, 1.0, 1.0, 1.0)

		_FireMap("Fire Map", 2D) = "white" {}
		_FireAmount("Fire amount", Range(0.0, 1.0)) = 0.0
		_FireSpeed("Fire speed", float) = 1.0

		_DissolveAmount("Dissolve amount", Range(0.0, 1.0)) = 0.0
		_DissolveUpperLimit("Dissolve upper", float) = 1.0
		_DissolveLowerLimit("Dissolve lower limit", float) = -1.0
		_DissolveMargin("Dissolve margin", float) = 0.1

		_ColorRampAmount("Color ramp amount", Range(0.0, 1.0)) = 0.0
		_ColorRampID0("Color ramp id 0", float) = 0.0
		_ColorRampID1("Color ramp id 1", float) = 0.0

		_VOAmplitude("Vertex offset amplitude", float) = 0.3
		_VOSpeed("Vertex offset speed", float) = 3.0

		// Blending state
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull mode", Float) = 0.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("SrcBlend", Float) = 1.0 //"One"
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("DestBlend", Float) = 0.0 //"Zero"
		[Enum(Opaque, 0, CutOff, 1, Transparent, 2)] _BlendMode("Blend mode", Float) = 0.0
//		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[Toggle] _ZWrite("__zw", Float) = 1.0

		_StencilMask("Stencil Mask", int) = 10

		/// Toggle Material Properties

		[Toggle(NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0
		[Toggle(SPECULAR)] _EnableSpecular("Enable Specular Light", Float) = 0
		[Toggle(FRESNEL)] _EnableFresnel("Enable fresnel", Float) = 1.0
		[Toggle(CUTOFF)] _EnableCutoff("Enable cutoff", Float) = 0
		[Toggle(DOUBLESIDED)] _EnableDoublesided("Enable doublesided", Float) = 0
		[Toggle(SILHOUETTE)] _EnableSilhouette("Enable silhouette", Float) = 0
		[Toggle(OPAQUEFRESNEL)] _EnableOpaqueFresnel("Enable opaque fresnel", Float) = 0
		[Toggle(OPAQUESPECULAR)] _EnableOpaqueSpecular("Enable opaque specular", Float) = 0
		[Toggle(BLENDFRESNEL)] _EnableBlendFresnel("Enable blend fresnel", Float) = 0.0
		[Toggle(DIFFUSE_AS_SPECULARMASK)] _EnableDiffuseAsSpecMask("Enable diffuse as specular mask", Float) = 0.0

		[Toggle(VERTEXOFFSET)]	_EnableVertexOffset("Enable vertex offset", Float) = 0.0
		[Toggle(VERTEXOFFSETX)] _EnableVertexOffsetX("Vertex offset X", Float) = 0.0
		[Toggle(VERTEXOFFSETY)] _EnableVertexOffsetY("Vertex offset Y", Float) = 0.0
		[Toggle(VERTEXOFFSETZ)] _EnableVertexOffsetZ("Vertex offset Z", Float) = 0.0

		/// Enum Material Properties
		[KeywordEnum(None, Reflection, Fire, Dissolve, Colorize)] FXLayer("Additional FX layer", Float) = 0
		[KeywordEnum(Normal, AutoInnerLight, BlinkLights, Emissive)] SelfIlluminate("Emission layer", Float) = 0

		[KeywordEnum(Normal, Color, ColorRamp)] ReflectionType("Reflection type", Float) = 0
	}

	SubShader {
		Tags { "Queue"="Geometry+10" "RenderType"="Opaque" "LightMode"="ForwardBase" }
	//	LOD 100
		ColorMask RGBA
	
		Pass {

			Stencil
			{
				Ref [_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			Cull [_Cull]
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
			ztest less

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#pragma shader_feature  __ SILHOUETTE
			#pragma shader_feature  __ NORMALMAP
			#pragma shader_feature  __ SPECULAR
			#pragma shader_feature  __ FRESNEL
			#pragma shader_feature  __ CUTOFF
			#pragma shader_feature  __ DOUBLESIDED
			#pragma shader_feature  __ OPAQUEALPHA
			#pragma shader_feature  __ OPAQUEFRESNEL
			#pragma shader_feature  __ BLENDFRESNEL
			#pragma shader_feature  __ OPAQUESPECULAR
			#pragma shader_feature	__ VERTEXOFFSET
			#pragma shader_feature	__ VERTEXOFFSETX
			#pragma shader_feature	__ VERTEXOFFSETY
			#pragma shader_feature	__ VERTEXOFFSETZ
			#pragma shader_feature	__ DIFFUSE_AS_SPECULARMASK

			#pragma shader_feature SELFILLUMINATE_NORMAL SELFILLUMINATE_AUTOINNERLIGHT SELFILLUMINATE_BLINKLIGHTS SELFILLUMINATE_EMISSIVE
			#pragma shader_feature FXLAYER_NONE FXLAYER_REFLECTION FXLAYER_FIRE FXLAYER_DISSOLVE FXLAYER_COLORIZE
			#pragma shader_feature REFLECTIONTYPE_NORMAL REFLECTIONTYPE_COLOR REFLECTIONTYPE_COLORRAMP


			#include "UnityCG.cginc" 
			#include "Lighting.cginc"
			#include "../HungryDragon.cginc"

//			#define FRESNEL

			#ifdef LOW_DETAIL_ON
			#undef NORMALMAP
			#undef SPECULAR
			#endif

			#ifdef MEDIUM_DETAIL_ON
			#undef SPECULAR
			#endif

			#ifdef HI_DETAIL_ON
			#endif

			#include "dragon.cginc"
			ENDCG
		}
	}
	CustomEditor "DragonShaderGUI"
}
