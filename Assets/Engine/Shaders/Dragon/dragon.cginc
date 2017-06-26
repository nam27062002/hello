
struct v2f {
	float4 vertex : SV_POSITION;
	half2 texcoord : TEXCOORD0;
	// float3 normal : NORMAL;
	// float3 halfDir : VECTOR;
	float3 vLight : TEXCOORD1;
	float3 normalWorld : TEXCOORD3;
#ifdef NORMALMAP
	float3 tangentWorld : TEXCOORD2;
	float3 binormalWorld : TEXCOORD4;
#endif

	//		        fixed3 posWorld : TEXCOORD5;
	fixed3 viewDir : TEXCOORD5;
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
uniform float4 _FresnelColor;
uniform float4 _AmbientAdd;

uniform float _SpecExponent;
uniform float _Fresnel;

uniform float3 _SecondLightDir;
uniform float4 _SecondLightColor;

#ifdef FIRE
uniform sampler2D _FireMap;
uniform float _FireAmount;
#endif

#ifdef REFL
uniform samplerCUBE _ReflectionMap;
uniform float _ReflectionAmount;
#endif

#ifdef AUTOINNERLIGHT
uniform float _InnerLightWavePhase;
uniform float _InnerLightWaveSpeed;
#endif

#ifdef CUTOUT
uniform float _Cutoff;
#endif


v2f vert(appdata_t v)
{
	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

	// Normal
	float3 normal = UnityObjectToWorldNormal(v.normal);
	// Light Probes
	o.vLight = ShadeSH9(float4(normal, 1.0));

	// Half View - See: Blinn-Phong
	float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);
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
	o.normalWorld *= sign(dot(o.normalWorld, o.viewDir));
#endif
	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	fixed4 main = tex2D(_MainTex, i.texcoord);
	fixed4 detail = tex2D(_DetailTex, i.texcoord);

#ifdef CUTOUT
	clip(main.a - _Cutoff);
#endif

#ifdef NORMALMAP
	float3 encodedNormal = UnpackNormal(tex2D(_BumpMap, i.texcoord));
	encodedNormal.z *= _NormalStrenght;
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));

#else
	float3 normalDirection = i.normalWorld;
#endif

	float3 light0Direction = normalize(_WorldSpaceLightPos0.xyz);
	float3 light1Direction = normalize(_SecondLightDir.xyz);
	// normalDirection = i.normal;
	fixed4 diffuse = max(0,dot(normalDirection, light0Direction)) * _LightColor0;
	diffuse += max(0, dot(normalDirection, light1Direction)) * _SecondLightColor;
	diffuse.w = 1.0;

#ifdef FRESNEL
	// Fresnel
	float fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _Fresnel), 0.0, 1.0);

#else
	float fresnel = 0.0f;
#endif

	// Specular

	float3 halfDir = normalize(i.viewDir + light0Direction);
#ifdef SPEC
	float specularLight = pow(max(dot(normalDirection, halfDir), 0), _SpecExponent) * detail.g;
	halfDir = normalize(i.viewDir + light1Direction);
	specularLight += pow(max(dot(normalDirection, halfDir), 0), _SpecExponent) * detail.g;

#else
	float specularLight = 0.0;
#endif

	fixed4 col;

#if defined (REFL)
	fixed4 reflection = texCUBE(_ReflectionMap, reflect(i.viewDir, normalDirection));

	fixed specMask = 0.2126 * reflection.r + 0.7152 * reflection.g + 0.0722 * reflection.b;

	//fixed4 reflection = texCUBE(_ReflectionMap, reflect(halfDir, normalDirection));
	float ref = specMask * _ReflectionAmount * detail.b;
	col = (1.0 - ref) * main + ref * reflection;

//	col = main * specMask * 1.0;

#elif defined (FIRE)

	i.texcoord.y = 1.0 - (i.texcoord.y * 0.75);
	i.texcoord.y *= i.texcoord.y;

	fixed4 intensity = tex2D(_FireMap, (i.texcoord.xy + half2(0.25, _Time.y * 0.666)));
	intensity *= tex2D(_FireMap, (i.texcoord.xy + float2(-0.25, _Time.y * 0.333)));// +pow(i.uv.y, 3.0);


	col = lerp(main, intensity, _FireAmount * detail.b); // lerp(fixed4(1.0, 0.0, 0.0, 1.0), fixed4(1.0, 1.0, 0.0, 1.0), intensity);

#else
	col = main;
#endif

	// Inner lights
#ifdef AUTOINNERLIGHT
	float wave = (i.texcoord.x * _InnerLightWavePhase) + (_Time.y * _InnerLightWaveSpeed);
	fixed satMask = (0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b) * detail.r;
	satMask = lerp(satMask, 1.0, detail.b);
	//	satMask *= detail.r *(((cos(wave.x) + 1.0) * 0.5) * ((sin(_Time.y) + 1.0) * 0.5)) * 10.0;
	fixed blink = lerp((sin(_Time.y * _InnerLightWavePhase) + 1.0) * 0.5, (cos(wave) + 1.0) * 0.5, detail.b);
	satMask *= blink * 10.0;
	fixed3 selfIlluminate = lerp(fixed3(0.0, 0.0, 0.0), _InnerLightColor.xyz, satMask);

#else

#ifdef BLINKLIGHTS
	float anim = sin(_Time.x * 40.0); // _SinTime.w * 0.5f;
#else
	float anim = 1.0;
#endif

	fixed3 selfIlluminate = (col.xyz * (detail.r * _InnerLightAdd * _InnerLightColor.xyz)) * anim;
#endif
	// fixed4 col = (diffuse + fixeW4(pointLights + ShadeSH9(float4(normalDirection, 1.0)),1)) * main * _Tint + _ColorAdd + specularLight + selfIlluminate; // To use ShaderSH9 better done in vertex shader
//	col = (diffuse + fixed4(i.vLight, 0.0)) * col * _Tint + _ColorAdd + specularLight + selfIlluminate + (fresnel * _FresnelColor) + _AmbientAdd; // To use ShaderSH9 better done in vertex shader
	col.xyz = (diffuse.xyz + i.vLight) * col.xyz * _Tint.xyz + _ColorAdd.xyz + specularLight + selfIlluminate + (fresnel * _FresnelColor.xyz) + _AmbientAdd.xyz; // To use ShaderSH9 better done in vertex shader

#ifndef CUTOUT
	UNITY_OPAQUE_ALPHA(col.a);
#else
	col.w *= _Tint.w;
#endif
	return col;
}
