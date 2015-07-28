#ifndef LIGHTING_INCLUDED
#define LIGHTING_INCLUDED

#include "UnityLightingCommon.cginc"
#include "UnityGlobalIllumination.cginc"

struct SurfaceOutput {
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Emission;
	half Specular;
	fixed Gloss;
	fixed Alpha;
};

#ifndef USING_DIRECTIONAL_LIGHT
#if defined (DIRECTIONAL_COOKIE) || defined (DIRECTIONAL)
#define USING_DIRECTIONAL_LIGHT
#endif
#endif

#if defined(UNITY_SHOULD_SAMPLE_SH) || defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
	#define UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
#endif

inline fixed4 UnityLambertLight (SurfaceOutput s, UnityLight light)
{
	fixed diff = max (0, dot (s.Normal, light.dir));
	
	fixed4 c;
	c.rgb = s.Albedo * light.color * diff;
	c.a = s.Alpha;
	return c;
}

inline fixed4 LightingLambert (SurfaceOutput s, UnityGI gi)
{
	fixed4 c;
	c = UnityLambertLight (s, gi.light);

	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += UnityLambertLight (s, gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += UnityLambertLight (s, gi.light3);
		#endif
	#endif

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif

	return c;
}

inline half4 LightingLambert_Deferred (SurfaceOutput s, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
{
	outDiffuseOcclusion = half4(s.Albedo, 1);
	outSpecSmoothness = 0.0;
	outNormal = half4(s.Normal * 0.5 + 0.5, 1);
	half4 emission = half4(s.Emission, 1);

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		emission.rgb += s.Albedo * gi.indirect.diffuse;
	#endif

	return emission;
}

inline void LightingLambert_GI (
	SurfaceOutput s,
	UnityGIInput data,
	inout UnityGI gi)
{
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal, false);
}

inline fixed4 LightingLambert_PrePass (SurfaceOutput s, half4 light)
{
	fixed4 c;
	c.rgb = s.Albedo * light.rgb;
	c.a = s.Alpha;
	return c;
}

// NOTE: some intricacy in shader compiler on some GLES2.0 platforms (iOS) needs 'viewDir' & 'h'
// to be mediump instead of lowp, otherwise specular highlight becomes too bright.
inline fixed4 UnityBlinnPhongLight (SurfaceOutput s, half3 viewDir, UnityLight light)
{
	half3 h = normalize (light.dir + viewDir);
	
	fixed diff = max (0, dot (s.Normal, light.dir));
	
	float nh = max (0, dot (s.Normal, h));
	float spec = pow (nh, s.Specular*128.0) * s.Gloss;
	
	fixed4 c;
	c.rgb = s.Albedo * light.color * diff + light.color * _SpecColor.rgb * spec;
	c.a = s.Alpha;

	return c;
}

inline fixed4 LightingBlinnPhong (SurfaceOutput s, half3 viewDir, UnityGI gi)
{
	fixed4 c;
	c = UnityBlinnPhongLight (s, viewDir, gi.light);

	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += UnityBlinnPhongLight (s, viewDir, gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += UnityBlinnPhongLight (s, viewDir, gi.light3);
		#endif
	#endif

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif

	return c;
}

inline half4 LightingBlinnPhong_Deferred (SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outDiffuseOcclusion, out half4 outSpecSmoothness, out half4 outNormal)
{
	outDiffuseOcclusion = half4(s.Albedo, 1);
	outSpecSmoothness = half4(_SpecColor.rgb, s.Specular);
	outNormal = half4(s.Normal * 0.5 + 0.5, 1);
	half4 emission = half4(s.Emission, 1);

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		emission.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	
	return emission;
}

inline void LightingBlinnPhong_GI (
	SurfaceOutput s,
	UnityGIInput data,
	inout UnityGI gi)
{
	gi = UnityGlobalIllumination (data, 1.0, s.Gloss, s.Normal, false);
}

inline fixed4 LightingBlinnPhong_PrePass (SurfaceOutput s, half4 light)
{
	fixed spec = light.a * s.Gloss;
	
	fixed4 c;
	c.rgb = (s.Albedo * light.rgb + light.rgb * _SpecColor.rgb * spec);
	c.a = s.Alpha;
	return c;
}

#ifdef UNITY_CAN_COMPILE_TESSELLATION
struct UnityTessellationFactors {
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};
#endif // UNITY_CAN_COMPILE_TESSELLATION

// Deprecated, kept around for existing user shaders.
#define UNITY_DIRBASIS \
const half3x3 unity_DirBasis = half3x3( \
  half3( 0.81649658,  0.0,        0.57735027), \
  half3(-0.40824830,  0.70710678, 0.57735027), \
  half3(-0.40824830, -0.70710678, 0.57735027) \
);

// Deprecated, kept around for existing user shaders. Only sampling the flat lightmap now.
half3 DirLightmapDiffuse(in half3x3 dirBasis, fixed4 color, fixed4 scale, half3 normal, bool surfFuncWritesNormal, out half3 scalePerBasisVector)
{
	return DecodeLightmap (color);
}

#endif
