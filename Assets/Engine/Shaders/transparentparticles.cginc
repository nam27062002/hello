// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members _LightmapIntensity)
//#pragma exclude_renderers d3d11

struct appdata_t {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float4 texcoord : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
#if defined(EXTENDED_PARTICLES)
	float2 particledata : TEXCOORD1;
#endif	//EXTENDED_PARTICLES

#if defined(NOISE_TEXTURE) || defined(FLOWMAP)
	float2 noiseuv : TEXCOORD2;
#endif	//NOISE_TEXTURE || FLOWMAP
};

sampler2D _MainTex;
float4 _MainTex_ST;

#if defined(EXTENDED_PARTICLES)
float _EmissionSaturation;
float _OpacitySaturation;
float _ColorMultiplier;

#if defined(COLOR_RAMP)
sampler2D _ColorRamp;
float4 _ColorRamp_ST;

#elif defined(COLOR_TINT)
float4 _BasicColor;

#else
float4 _BasicColor;
float4 _SaturatedColor;
#endif	//COLOR_RAMP

#if defined(DISSOLVE_ENABLED)
float4 _DissolveStep;
#endif //DISSOLVE_ENABLED

#if defined(NOISE_TEXTURE) || defined(FLOWMAP)
sampler2D _NoiseTex;
float4 _NoiseTex_ST;
float4 _NoisePanning;
#endif	//NOISE_TEXTURE || FLOWMAP

#else	//EXTENDED_PARTICLES
float4 _TintColor;

#if defined(EMISSIVEPOWER)
float _EmissivePower;
#endif	//EMISSIVEPOWER

#endif	//EXTENDED_PARTICLES

float2 _Panning;

#if defined(BLENDMODE_ADDITIVEALPHABLEND)
float _ABOffset;
#endif	//BLENDMODE_ADDITIVEALPHABLEND

float _GlobalAlpha;



v2f vert(appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.color = v.color;

//	o.texcoord = TRANSFORM_TEX(v.texcoord + _Panning.xy * _Time.yy, _MainTex);
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) + (_Panning.xy * _Time.yy);

#if defined(NOISE_TEXTURE) || defined(FLOWMAP)

#if defined(NOISE_TEXTURE)

#if defined(NOISEUV)
	o.noiseuv = TRANSFORM_TEX(v.texcoord, _NoiseTex) + (_NoisePanning.xy * _Time.yy);
#else
	o.noiseuv = TRANSFORM_TEX(v.texcoord, _MainTex) + (_NoisePanning.xy * _Time.yy);
#endif

#else	//FLOWMAP

#if defined(NOISEUV)
	o.noiseuv = TRANSFORM_TEX(v.texcoord, _NoiseTex);
#else
	o.noiseuv = TRANSFORM_TEX(v.texcoord, _MainTex);
#endif

#endif	//NOISE_TEXTURE

#endif 	//NOISE_TEXTURE || FLOWMAP

#ifdef EXTENDED_PARTICLES
	o.particledata = v.texcoord.zw;
#endif	//EXTENDED_PARTICLES
	return o;
}


fixed4 frag(v2f i) : COLOR
{
	fixed4 tex;
	//fixed4 tex = fixed4(1.0, 1.0, 1.0, 1.0);// tex2D(_MainTex, i.texcoord);
	fixed4 col;

#ifdef EXTENDED_PARTICLES

#if defined(APPLY_RGB_COLOR_VERTEX)
	float4 vcolor = i.color;
#else	//APPLY_RGB_COLOR_VERTEX
	float4 vcolor = float4(1.0, 1.0, 1.0, i.color.w);
#endif	//APPLY_RGB_COLOR_VERTEX

	float nEmission = 1.0;
	float nAlpha = 1.0;
	float nDissolve = 1.0;


#if defined(NOISE_TEXTURE)
	tex = tex2D(_MainTex, i.texcoord);
	float3 noise = tex2D(_NoiseTex, i.noiseuv);

#if defined(NOISE_TEXTURE_EMISSION)
	nEmission = noise.x;
#endif	//NOISE_TEXTURE_EMISSION

#if defined(NOISE_TEXTURE_ALPHA)
	nAlpha = noise.y;
#endif	//NOISE_TEXTURE_ALPHA

#if defined(NOISE_TEXTURE_DISSOLVE)
	nDissolve = noise.z;
#endif	//NOISE_TEXTURE_DISSOLVE

//	tex = fixed4(0.0, 1.0, 0.0, 1.0);
	
//#endif	//NOISE_TEXTURE
#elif defined(FLOWMAP)
	float2 off = (tex2D(_NoiseTex, i.noiseuv).xy * 2.0f - 1.0f) * _NoisePanning.y;
//	float2 off = float2(1.0, 0.0);
	float time = _Time.y * _NoisePanning.x;
	float phase0 = frac(time * 0.5f + 0.5f);
	float phase1 = frac(time * 0.5f + 1.0f);

	tex = tex2D(_MainTex, i.texcoord + off * phase0);
	fixed4 tex2 = tex2D(_MainTex, i.texcoord + off * phase1);

	float flowLerp = abs((0.5f - phase0) / 0.5f);

	tex = lerp(tex, tex2, flowLerp);
//	tex = fixed4(1.0, 0.0, 0.0, 1.0);

#else
	tex = tex2D(_MainTex, i.texcoord);

#endif

#if defined(DISSOLVE_ENABLED)

#if defined(DISSOLVE_VERTEXCOLORALFA)
	float ramp = -1.0 + (i.color.w * 2.0);
	col.a = clamp(tex.g * smoothstep(_DissolveStep.x, _DissolveStep.y, (tex.b + ramp) * nDissolve) * _OpacitySaturation * nAlpha, 0.0, 1.0);
#else
	float ramp = -1.0 + (i.particledata.x * 2.0);
	col.a = clamp(tex.g * smoothstep(_DissolveStep.x, _DissolveStep.y, (tex.b + ramp) * nDissolve) * _OpacitySaturation * vcolor.w * nAlpha, 0.0, 1.0);
#endif

#else	//DISSOLVE_ENABLED
	col.a = clamp(tex.g * _OpacitySaturation * vcolor.w, 0.0, 1.0) * nAlpha;
#endif	//DISSOLVE_ENABLED

#if !defined(COLOR_TINT)
	float lerpValue = clamp(tex.r * i.particledata.y * _ColorMultiplier * nEmission, 0.0, 1.0);
#endif

#ifdef BLENDMODE_ALPHABLEND

#if defined(COLOR_RAMP)
	col.xyz = tex2D(_ColorRamp, float2((1.0 - lerpValue), 0.0)) * vcolor.xyz * _EmissionSaturation;
#elif defined(COLOR_TINT)
	col.xyz = tex.x * _BasicColor.xyz * vcolor.xyz * nEmission * _EmissionSaturation;
#else
	col.xyz = lerp(_BasicColor.xyz * vcolor.xyz, _SaturatedColor, lerpValue) * _EmissionSaturation;
#endif	//COLOR_RAMP

	col.a *= _GlobalAlpha;

#else	//BLENDMODE_ALPHABLEND

#if defined(COLOR_RAMP)
	col.xyz = tex2D(_ColorRamp, float2((1.0 - lerpValue), 0.0)) * vcolor.xyz * col.a * _EmissionSaturation;
#elif defined(COLOR_TINT)
	col.xyz = tex.x * _BasicColor.xyz * vcolor.xyz * nEmission * _EmissionSaturation * col.a;
#else
	col.xyz = lerp(_BasicColor.xyz * vcolor.xyz, _SaturatedColor, lerpValue) * col.a * _EmissionSaturation;
#endif	//COLOR_RAMP

	col *= _GlobalAlpha;

#endif	//BLENDMODE_ALPHABLEND

/*
#ifdef COLOR_TINT
	col.xyz = tex.x * _BasicColor.xyz * vcolor.xyz * nEmission * _EmissionSaturation * col.a;
#endif
*/
#else	//EXTENDED_PARTICLES
	tex = tex2D(_MainTex, i.texcoord);

#ifdef BLENDMODE_ADDITIVEALPHABLEND
	tex *= _TintColor;
	float luminance = clamp(dot(tex, float4(0.2126, 0.7152, 0.0722, 0.0)) * tex.a * _ABOffset, 0.0, 1.0);
	fixed4 one = fixed4(1, 1, 1, 1);
	col = lerp(2.0 * (i.color * tex), one - 2.0 * (one - i.color) * (one - tex), luminance);

#else	//BLENDMODE_ADDITIVEALPHABLEND

	col = i.color * tex;
	col *= _TintColor;

#ifdef EMISSIVEPOWER
	col *= _EmissivePower;
#endif	//EMISSIVEPOWER

#if defined(BLENDMODE_SOFTADDITIVE)
	col.rgb *= col.a;
#elif defined(BLENDMODE_ALPHABLEND)
	col *= 2.0;
#elif defined(BLENDMODE_ADDITIVEDOUBLE)
	col *= 4.0;
#endif	//BLENDMODE_SOFTADDITIVE

#endif	//BLENDMODE_ADDITIVEALPHABLEND

	col *= _GlobalAlpha;
#endif	//EXTENDED_PARTICLES

	return col;
}

