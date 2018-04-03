// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members _LightmapIntensity)
#pragma exclude_renderers d3d11

struct appdata_t {
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 texcoord : TEXCOORD0;
#ifdef EXTENDED_PARTICLES
	float2 particledata : TEXCOORD1;
#endif
	fixed4 color : COLOR;
};

sampler2D _MainTex;
float4 _MainTex_ST;

#ifdef EXTENDED_PARTICLES
float4 _BasicColor;
float4 _SaturatedColor;
float _EmissionSaturation;
float _OpacitySaturation;
float _ColorMultiplier;

#ifdef DISSOLVE
float4 _DissolveStep;
#endif

#ifdef COLOR_RAMP
sampler2D _ColorRamp;
float4 _ColorRamp_ST;
#endif

#else

float4 _TintColor;

#ifdef EMISSIVEPOWER
float _EmissivePower;
#endif

#endif

#ifdef AUTOMATICPANNING
float2 _Panning;
#endif

#ifdef BLENDMODE_ADDITIVEALPHABLEND
float _ABOffset;
#endif

v2f vert(appdata_t v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.color = v.color;
#ifdef AUTOMATICPANNING
	v.texcoord.xy += _Panning.xy * _Time.yy;
#endif

	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

#ifdef EXTENDED_PARTICLES
	o.particledata = v.texcoord.zw;
#endif
	return o;
}


fixed4 frag(v2f i) : COLOR
{
	fixed4 tex = tex2D(_MainTex, i.texcoord);
	fixed4 col;

//#ifdef AUTOMATICPANNING
//	return fixed4(1.0, 1.0, 0.0, 1.0);
//#endif

#ifdef EXTENDED_PARTICLES

#ifdef APPLY_RGB_COLOR_VERTEX
	float4 vcolor = i.color;
#else
	float4 vcolor = float4(1.0, 1.0, 1.0, i.color.w);
#endif	//APPLY_RGB_COLOR_VERTEX

#ifdef DISSOLVE
	float ramp = -1.0 + (i.particledata.x * 2.0);
	col.a = clamp(tex.g * smoothstep(_DissolveStep.x, _DissolveStep.y, tex.b + ramp) * _OpacitySaturation * vcolor.w, 0.0, 1.0);
#else
	col.a = clamp(tex.g * _OpacitySaturation * vcolor.w, 0.0, 1.0);
#endif	//DISSOLVE

	float lerpValue = clamp(tex.r * i.particledata.y * _ColorMultiplier, 0.0, 1.0);
#ifdef BLENDMODE_ALPHABLEND
#if COLOR_RAMP
	col.xyz = tex2D(_ColorRamp, float2((1.0 - lerpValue), 0.0)) * vcolor.xyz * _EmissionSaturation;
#else
	col.xyz = lerp(_BasicColor.xyz * vcolor.xyz, _SaturatedColor, lerpValue) * _EmissionSaturation;
#endif	//COLOR_RAMP

#else
#if COLOR_RAMP
	col.xyz = tex2D(_ColorRamp, float2((1.0 - lerpValue), 0.0)) * vcolor.xyz * col.a * _EmissionSaturation;
#else
	col.xyz = lerp(_BasicColor.xyz * vcolor.xyz, _SaturatedColor, lerpValue) * col.a * _EmissionSaturation;
#endif	//COLOR_RAMP

#endif	//BLENDMODE_ALPHABLEND

#else	//EXTENDED_PARTICLES

#ifdef BLENDMODE_ADDITIVEALPHABLEND
	tex *= _TintColor;
	float luminance = clamp(dot(tex, float4(0.2126, 0.7152, 0.0722, 0.0)) * tex.a * _ABOffset, 0.0, 1.0);
	fixed4 one = fixed4(1, 1, 1, 1);
	col = lerp(2.0 * (i.color * tex), one - 2.0 * (one - i.color) * (one - tex), luminance);

#else	//BLENDMODE_ADDITIVEALPHABLEND

	col = i.color * tex;
#ifdef EMISSIVEPOWER
	col *= _EmissivePower;
#endif	//EMISSIVEPOWER

	col *= _TintColor;

#if defined(BLENDMODE_SOFTADDITIVE)
	col.rgb *= col.a;
#elif defined(BLENDMODE_ALPHABLEND)
	col *= 2.0;
#elif defined(BLENDMODE_ADDITIVEDOUBLE)
	col *= 4.0;
#endif	//BLENDMODE_SOFTADDITIVE

#endif	//BLENDMODE_ADDITIVEALPHABLEND

#endif	//EXTENDED_PARTICLES

	return col;
}

