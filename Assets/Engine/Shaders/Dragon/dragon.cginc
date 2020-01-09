
struct appdata_t {
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
#ifdef VERTEXOFFSET
	float4 color : COLOR;
#endif
};

struct v2f {
	float4 vertex : SV_POSITION;
	half2 texcoord : TEXCOORD0;
//	float3 vLight : COLOR;
	half3 normalWorld : NORMAL;
#ifdef NORMALMAP
	half3 tangentWorld : TEXCOORD2;
	half3 binormalWorld : TEXCOORD4;
#endif
	half3 viewDir : TEXCOORD5;
#if defined(FXLAYER_FIRE) || defined(FXLAYER_DISSOLVE) || defined(VERTEXOFFSET)
	half2 screenPos : TEXCOORD1;
#endif
};

sampler2D _MainTex;
float4 _MainTex_ST;

sampler2D _DetailTex;
float4 _DetailTex_ST;

#ifdef NORMALMAP
sampler2D _BumpMap;
float	_NormalStrenght;
#endif

uniform float4 _Tint;
uniform float4 _ColorAdd;

uniform float _InnerLightAdd;
uniform float4 _InnerLightColor;

#ifdef FRESNEL
uniform float _Fresnel;
uniform float4 _FresnelColor;
#endif

//uniform float4 _AmbientAdd;

uniform float3 _SecondLightDir;
uniform float4 _SecondLightColor;
#ifdef SPECULAR
uniform float _SpecExponent;
#endif


#if defined(VERTEXOFFSET)
uniform float _VOAmplitude;
uniform float _VOSpeed;
#endif

#if defined (FXLAYER_REFLECTION)
uniform samplerCUBE _ReflectionMap;
uniform float _ReflectionAmount;

#if defined (REFLECTIONTYPE_COLOR)
uniform float4 _ReflectionColor;
#elif defined (REFLECTIONTYPE_COLORRAMP)
uniform sampler2D _FireMap;
uniform float4 _FireMap_TexelSize;
uniform float4 _FireMap_ST;
#endif

#elif defined (FXLAYER_FIRE)
uniform sampler2D _FireMap;
uniform float4 _FireMap_ST;
uniform float _FireAmount;
uniform float _FireSpeed;
#elif defined (FXLAYER_DISSOLVE)
uniform sampler2D _FireMap;
uniform float _DissolveAmount;
uniform float _DissolveLowerLimit;
uniform float _DissolveUpperLimit;
uniform float _DissolveMargin;
#elif defined (FXLAYER_COLORIZE)
uniform sampler2D _FireMap;
uniform float4 _FireMap_TexelSize;
uniform float4 _FireMap_ST;
uniform float _ColorRampAmount;
uniform float _ColorRampID0;
uniform float _ColorRampID1;
#endif

#ifdef SELFILLUMINATE_AUTOINNERLIGHT
uniform float _InnerLightWavePhase;
#endif
#if defined(SELFILLUMINATE_AUTOINNERLIGHT) || defined(SELFILLUMINATE_BLINKLIGHTS)
uniform float _InnerLightWaveSpeed;
#endif

#ifdef CUTOFF
uniform float _Cutoff;
#endif

v2f vert(appdata_t v)
{
	v2f o;

#if defined(VERTEXOFFSET)
	fixed smooth = v.color.r;		//smoothstep(0.7, -0.0, v.vertex.z);
//	v.vertex.xyz += v.normal * sin(v.vertex.x * 3.0 + _Time.y * 5.0) * 0.2 * smooth;

	half wave = sin((_Time.y * _VOSpeed) + v.vertex.y + v.vertex.x) * smooth * _VOAmplitude;

	half4 axis = float4(
#if defined(VERTEXOFFSETX)
		1.0,
#else
		0.0,
#endif

#if defined(VERTEXOFFSETY)
		1.0,
#else
		0.0,
#endif

#if defined(VERTEXOFFSETZ)
		1.0,
#else
		0.0,
#endif
		0.0
	);

	//float4 tvertex = v.vertex + float4(sin((_Time.y * hMult * _SpeedWave ) * 0.525) * hMult * 0.08, 0.0, 0.0, 0.0f);
	v.vertex += axis * wave;

#endif
//	v.vertex.x *= 0.25;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

	// Normal
	half3 normal = UnityObjectToWorldNormal(v.normal);

	// Light Probes
//	o.vLight = ShadeSH9(float4(normal, 1.0));

	// Half View - See: Blinn-Phong
	half3 viewDirection = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
	o.viewDir = viewDirection;

	// To calculate tangent world
#ifdef NORMALMAP
	o.tangentWorld = UnityObjectToWorldNormal(v.tangent);
	o.normalWorld = normal;
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
	o.normalWorld = normal;
#endif

#ifdef DOUBLESIDED
	half s = sign(dot(o.normalWorld, o.viewDir));
	o.normalWorld *= s;

#endif

#if defined(FXLAYER_FIRE)
	o.screenPos = (v.vertex.xy / v.vertex.w) * _FireMap_ST.xy * 0.1;
#elif defined(FXLAYER_DISSOLVE)
	fixed limit = smoothstep(_DissolveUpperLimit, _DissolveLowerLimit + _DissolveMargin, v.vertex.x);
	o.screenPos = fixed2(0.0, limit);
#endif

	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	fixed4 main = tex2D(_MainTex, i.texcoord);
	fixed4 detail = tex2D(_DetailTex, i.texcoord);

#if defined (CUTOFF) && !defined(FXLAYER_DISSOLVE) 
	clip(main.a - _Cutoff);
#endif

#ifdef SILHOUETTE
	return _Tint;
#endif

#ifdef NORMALMAP
	half3 encodedNormal = UnpackNormal(tex2D(_BumpMap, i.texcoord));
	encodedNormal.z *= _NormalStrenght;
	half3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	half3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));

#else
	half3 normalDirection = i.normalWorld;
#endif

	half3 light0Direction = normalize(_WorldSpaceLightPos0.xyz);
	half3 light1Direction = normalize(_SecondLightDir.xyz);
	// normalDirection = i.normal;
	fixed4 diffuse = max(0,dot(normalDirection, light0Direction)) * _LightColor0;
	diffuse += max(0, dot(normalDirection, light1Direction)) * _SecondLightColor;
	diffuse.w = 1.0;

#ifdef SPECULAR
	// Specular
	half3 halfDir = normalize(i.viewDir + light0Direction);

#ifdef DIFFUSE_AS_SPECULARMASK
	fixed specMsk = dot(main.xyz, float3(0.3, 0.59, 0.11));
#else
	fixed specMsk = detail.g;
#endif

	half specularLight = pow(max(dot(normalDirection, halfDir), 0), _SpecExponent) * specMsk;
	halfDir = normalize(i.viewDir + light1Direction);
	specularLight += pow(max(dot(normalDirection, halfDir), 0), _SpecExponent) * specMsk;

#else
	half specularLight = 0.0;

#endif

	fixed4 col;

#if defined (FXLAYER_REFLECTION)		//Used by chinese dragon
	fixed4 reflection = texCUBE(_ReflectionMap, reflect(i.viewDir, normalDirection));

#if defined (REFLECTIONTYPE_COLOR)
	reflection *= _ReflectionColor;
#elif defined (REFLECTIONTYPE_COLORRAMP)
	reflection = tex2D(_FireMap, fixed2(reflection.r, 0));
#endif

//	fixed specMask = 0.2126 * reflection.r + 0.7152 * reflection.g + 0.0722 * reflection.b;
//	float ref = specMask * _ReflectionAmount * detail.b;

	fixed ref = _ReflectionAmount * detail.b;

	col = (1.0 - ref) * main + ref * reflection;

#elif defined (FXLAYER_FIRE)	//Used by pet phoenix
//	i.texcoord.y = 1.0 - (i.texcoord.y * 0.75);
//	i.texcoord.y *= i.texcoord.y;

	fixed4 intensity = tex2D(_FireMap, (i.screenPos.xy + half2(_Time.y * _FireSpeed, 0.25)));
	intensity *= tex2D(_FireMap, (i.screenPos.xy + float2(_Time.y * _FireSpeed * 0.5, -0.25)));// +pow(i.uv.y, 3.0);

	fixed fireMask = _FireAmount * detail.b;
	col = lerp(main, intensity, fireMask); // lerp(fixed4(1.0, 0.0, 0.0, 1.0), fixed4(1.0, 1.0, 0.0, 1.0), intensity);

#elif defined (FXLAYER_DISSOLVE)
	fixed noise = tex2D(_FireMap, i.texcoord).r * _DissolveMargin;
	fixed limit = i.screenPos.y - noise;
	fixed border = step(0.01, limit - _DissolveAmount);

	clip(limit - _DissolveAmount);
	col = lerp(main, fixed4(1.0, 0.3, 0.0, 1.0), 1.0 - border);

#elif defined (FXLAYER_COLORIZE)
	fixed4 col0 = tex2D(_FireMap, fixed2(main.r, (_ColorRampID0 + 0.5) * _FireMap_TexelSize.y));
	fixed4 col1 = tex2D(_FireMap, fixed2(main.r, (_ColorRampID1 + 0.5) * _FireMap_TexelSize.y));
	col = lerp(col0, col1, _ColorRampAmount);
	col = lerp(main, col, detail.b);

//	col = _FireMap_ST;

#else
	col = main;

#endif
	// Self illumination (fire rush & specific dragons)

#if defined (SELFILLUMINATE_AUTOINNERLIGHT)			// Used in devil dragon
	float wave = /*(i.texcoord.x * _InnerLightWavePhase) + */(_Time.y * _InnerLightWaveSpeed);
	fixed satMask = (0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b) * detail.r;
	satMask = lerp(satMask, 1.0, detail.b);
	fixed blink = lerp((sin(_Time.y * _InnerLightWavePhase) + 1.0) * 0.5, (cos(wave) + 1.0) * 0.5, detail.b);
	satMask *= blink;
	fixed3 selfIlluminate = lerp(fixed3(0.0, 0.0, 0.0), _InnerLightColor.xyz, satMask);

#elif defined (SELFILLUMINATE_BLINKLIGHTS)			//Used by reptile rings
	float anim = 1.0 + sin(_Time.y * _InnerLightWaveSpeed); // _SinTime.w * 0.5f;
	fixed3 selfIlluminate = col.xyz * anim * detail.r * _InnerLightColor.xyz * _InnerLightAdd;

#elif defined(SELFILLUMINATE_EMISSIVE)
	fixed3 selfIlluminate = lerp(fixed3(0.0, 0.0, 0.0), _InnerLightColor.xyz, detail.r * _InnerLightColor.a * _InnerLightAdd);

#else
	fixed3 selfIlluminate = (col.xyz * (detail.r * _InnerLightAdd * _InnerLightColor.xyz));	//fire rush illumination

#endif

//#if defined (FXLAYER_REFLECTION)
//	col.xyz = lerp((diffuse.xyz + i.vLight) * col.xyz * _Tint.xyz + _ColorAdd.xyz + specularLight + selfIlluminate, col.xyz * _Tint.xyz + _ColorAdd.xyz, ref); //+ _AmbientAdd.xyz; // To use ShaderSH9 better done in vertex shader
//#else
	col.xyz = ((diffuse.xyz + UNITY_LIGHTMODEL_AMBIENT.xyz/* + i.vLight*/) * col.xyz * _Tint.xyz) + _ColorAdd.xyz + specularLight + selfIlluminate; //+ _AmbientAdd.xyz; // To use ShaderSH9 better done in vertex shader
//#endif

// Fresnel
#ifdef FRESNEL
	half fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _Fresnel), 0.0, 1.0);
#ifdef BLENDFRESNEL
	col.xyz = lerp(col.xyz, _FresnelColor.xyz, fresnel * _FresnelColor.w);
#else
	col.xyz = lerp(col.xyz, col.xyz + _FresnelColor.xyz, fresnel * _FresnelColor.w);
#endif
//	col.xyz += fresnel * _FresnelColor.xyz;
#endif


// Ambient
//	col.xyz += unity_LightColor[0];

	// Opaque
#ifdef OPAQUEALPHA
	UNITY_OPAQUE_ALPHA(col.a);

#else	// OPAQUEALPHA
//	col.w = 0.0f;
	half opaqueLight = 0.0;
#if defined(FRESNEL) && defined(OPAQUEFRESNEL)
	opaqueLight = fresnel;
//	col.w += fresnel;
#endif

#if defined(SPECULAR) && defined(OPAQUESPECULAR)
	opaqueLight = max(opaqueLight, specularLight);
//	col.w += specularLight;
#endif

	col.w = max(col.w, opaqueLight);
	col.w *= _Tint.w;
#endif	// OPAQUEALPHA

#if defined(NIGHT)
	return col * fixed4(0.5, 0.5, 1.0, 1.0);
#else
    return col;
#endif
}
