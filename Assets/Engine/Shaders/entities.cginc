// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//#undef DYNAMIC_LIGHT

struct appdata_t
{
	float4 vertex : POSITION;
	half2 uv : TEXCOORD0;
	fixed4 color : COLOR;
	half3 normal : NORMAL;
	half4 tangent : TANGENT;
};

struct v2f
{
	float4 vertex : SV_POSITION;
	fixed4 color : COLOR;


	half3 normalWorld : NORMAL;

#if defined(LITMODE_LIT)


#if defined(NORMALMAP)
	half3 tangentWorld : TANGENT;
	half3 binormalWorld : TEXCOORD5;
#endif

#if defined(DYNAMIC_LIGHT)
	half3 vLight : TEXCOORD2;
#endif

#if defined(SPECULAR) || defined(SPECMASK)
	half3 halfDir : TEXCOORD7;
#endif

#endif //defined(LITMODE_LIT)


#if defined(FRESNEL) || defined(FREEZE) || defined(REFLECTIONMAP)
	half3 viewDir : VECTOR;
#endif

	half2 uv : TEXCOORD0;

#if defined(MATCAP) || defined(FREEZE)
	float2 cap : TEXCOORD1;
#endif

};

uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
uniform float4 _MainTex_TexelSize;

#if defined(MATCAP) || defined (FREEZE)
uniform sampler2D _MatCap;
uniform float4 _GoldColor;
#endif


#if defined(LITMODE_LIT)

#if defined(NORMALMAP)
uniform sampler2D _NormalTex;
uniform float4 _NormalTex_ST;
uniform float _NormalStrength;
#endif

#if defined(SPECULAR)
uniform float _SpecularPower;
uniform float4 _SpecularColor;
#endif

#if defined(SPECMASK)
uniform sampler2D _SpecMask;
uniform float _SpecExponent;
uniform float4 _SecondLightDir;

#endif

#endif //defined(LITMODE_LIT)

#if defined(FRESNEL) || defined(FREEZE)
uniform float _FresnelPower;
uniform float4 _FresnelColor;

#if defined(FREEZE)
uniform float4 _FresnelColor2;
#endif

#endif

#if defined(TINT)
uniform float4 _Tint;
#endif

#if defined(EMISSIVE)
uniform float _EmissiveIntensity;
uniform float _EmissiveBlink;
uniform float _EmissiveOffset;
#endif

#if defined(VERTEX_ANIMATION)
uniform float _TimePhase;
uniform float _Period;
uniform float4 _VertexAnimation;

#if defined(JELLY)
uniform float _TimePhase2;
uniform float _Period2;
uniform float4 _VertexAnimation2;
uniform float4 _VertexAnimation3;
#endif

#endif

#if defined(AMBIENTCOLOR)
uniform float4 _AmbientColor;
#endif

#if defined(REFLECTIONMAP)
uniform samplerCUBE _ReflectionMap;
uniform float _ReflectionAmount;
#endif

#if defined(COLORMODE_TINT)
uniform float4 _Tint1;
#elif defined(COLORMODE_GRADIENT)
uniform float4 _Tint1;
uniform float4 _Tint2;
#elif defined(COLORMODE_COLORRAMP) || defined(COLORMODE_COLORRAMPMASKED)
uniform sampler2D _RampTex;
uniform float4 _RampTex_TexelSize;

#endif


v2f vert(appdata_t v)
{
	v2f o;

#if defined(VERTEX_ANIMATION)

#if defined(JELLY)
	half4 anim = sin(_Time.y * _TimePhase + v.vertex.y * _Period);
	v.vertex.xyz += anim.y * _VertexAnimation.y * v.normal * v.color.g;
	anim = sin(_Time.y * _TimePhase2 + v.vertex.y * _Period2);
	v.vertex += anim * _VertexAnimation2 * v.color.r; //* (1.0 - s);
	anim = sin(_Time.y * _TimePhase2 * 0.85 + v.vertex.y * _Period2);
	v.vertex += anim * _VertexAnimation3 * v.color.b; // *(1.0 - s);

#else
	half4 anim = sin(_Time.y * _TimePhase + v.vertex.y * _Period);
	v.vertex += anim * _VertexAnimation * v.color.g;

#endif

#endif
	o.vertex = UnityObjectToClipPos(v.vertex);

	o.uv = TRANSFORM_TEX(v.uv, _MainTex);

	half3 normal = UnityObjectToWorldNormal(v.normal);

#if defined(DYNAMIC_LIGHT) && defined(LITMODE_LIT)
	o.vLight = ShadeSH9(float4(normal, 1.0));
#endif
	// To calculate tangent world
#if defined(NORMALMAP) && defined(LITMODE_LIT)
	o.tangentWorld = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
//	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	o.normalWorld = normal;
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
//	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	o.normalWorld = normal;
#endif

	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

#if defined(FRESNEL) || defined(FREEZE) || defined(REFLECTIONMAP) || (defined(LITMODE_LIT) && (defined(SPECULAR) || defined(SPECMASK)))
	half3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
#endif

#if defined(SPECULAR) && defined(LITMODE_LIT)
	// Half View - See: Blinn-Phong
	//	fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
	o.halfDir = normalize(lightDirection + viewDirection);

#elif defined(SPECMASK) && defined(LITMODE_LIT)
	half3 lightDirection = normalize(_SecondLightDir.xyz);
	o.halfDir = normalize(lightDirection + viewDirection);

#endif

#if defined(FRESNEL) || defined(FREEZE) || defined(REFLECTIONMAP)
	o.viewDir = viewDirection;
#endif



	o.color = v.color;

#if defined(MATCAP) || defined(FREEZE)
	half3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
	worldNorm = mul((half3x3)UNITY_MATRIX_V, worldNorm);
	o.cap.xy = worldNorm.xy * 0.5 + 0.5;
#endif

	return o;
}

fixed4 frag(v2f i) : SV_Target
{
#if defined(NORMALMAP) && defined(LITMODE_LIT)
	// Calc normal from detail texture normal and tangent world
	half4 encodedNormal = tex2D(_NormalTex, i.uv);
	half3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	half3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	half3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	half3 normalDirection = i.normalWorld;
#endif

	fixed4 diff = tex2D(_MainTex, i.uv);

#if defined (REFLECTIONMAP)
	fixed4 reflection = texCUBE(_ReflectionMap, reflect(i.viewDir, normalDirection));

	//	fixed specMask = 0.2126 * reflection.r + 0.7152 * reflection.g + 0.0722 * reflection.b;
	//	float ref = specMask * _ReflectionAmount * detail.b;

	half ref = _ReflectionAmount;

#if defined(COLORMODE_COLORRAMPMASKED)
	diff.x = (1.0 - ref) * diff.x + ref * reflection.x;
#else
	diff = (1.0 - ref) * diff + ref * reflection;
#endif

#endif

	// sample the texture
#if defined(COLORMODE_TINT)
	fixed4 col = diff.x * _Tint1;
#elif defined(COLORMODE_GRADIENT)
	fixed4 col = lerp(_Tint1, _Tint2, diff.x);
#elif defined(COLORMODE_COLORRAMP)
//	fixed4 diff = tex2D(_MainTex, i.uv);
	fixed2 offset = fixed2(diff.x, 0.0);
	fixed4 col = fixed4(tex2D(_RampTex, offset).xyz, diff.w);

#elif defined(COLORMODE_COLORRAMPMASKED)
//	fixed4 diff = tex2D(_MainTex, i.uv);
	fixed vy = (floor(diff.y + 0.5) * 2.0) + floor(diff.z + 0.5) + 0.5;
	fixed2 offset = fixed2(diff.x, vy * _RampTex_TexelSize.y );
	fixed4 col = fixed4(tex2D(_RampTex, offset).xyz, diff.w);
#else
	fixed4 col = diff;
#endif

#if defined(LITMODE_LIT)

#if defined(SPECMASK)
	fixed4 colspec = tex2D(_SpecMask, i.uv);
	half specMask = 0.2126 * colspec.r + 0.7152 * colspec.g + 0.0722 * colspec.b + col.a;
#elif defined(OPAQUESPECULAR)
	half specMask = 1.0;
#else
	half specMask = 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
#endif

#endif //(LITMODE_LIT)



#if defined(EMISSIVE)
	float anim = (((sin(_Time.y * _EmissiveBlink) + 1.0) * 0.5 * _EmissiveIntensity) + _EmissiveOffset) * col.a;
	col.xyz *= 1.0 + anim;
#endif

#if defined(TINT)
	col.xyz *= _Tint.xyz;
#endif

#if defined(LITMODE_LIT)

	fixed3 diffuse = max(0, dot(normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0.xyz;
#if defined(DYNAMIC_LIGHT)
	col.xyz *= diffuse + i.vLight;
#else

#if defined(NOAMBIENT)
	col.xyz *= diffuse;// +UNITY_LIGHTMODEL_AMBIENT.xyz;
#else
	col.xyz *= diffuse + UNITY_LIGHTMODEL_AMBIENT.xyz;
#endif

#endif


#if defined(SPECULAR)
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower) * specMask;
	col.xyz += specular * (col.xyz + _SpecularColor.xyz * 2.0);

#if defined(OPAQUESPECULAR)
	col.a = max(col.a, specular * 4.0);
#endif


#elif defined(SPECMASK)
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecExponent) * specMask;
	col.xyz = lerp(col.xyz, colspec.xyz, specular);
	col.a = max(col.a, specular);
#endif

#endif //defined(LITMODE_LIT)


#if defined(AMBIENTCOLOR)
	col.xyz += _AmbientColor.xyz;
#endif

#if defined(FRESNEL) || defined(FREEZE)
	half fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _FresnelPower), 0.0, 1.0) * _FresnelColor.w;
//	col.xyz *= lerp(_FresnelColor2.xyz, _FresnelColor.xyz, fresnel);

#if defined(FREEZE)
	col.xyz *= _FresnelColor2.xyz;
#endif

	col.xyz += _FresnelColor.xyz * fresnel;
#endif

#if defined(MATCAP) || defined(FREEZE)
	fixed4 mc = tex2D(_MatCap, i.cap) * _GoldColor; // _FresnelColor;

//	col = (col + ((mc*2.0) - 0.5));
	col = lerp(col, mc * 3.0, _GoldColor.w);// (1.0 - clamp(_FresnelPower, 0.0, 1.0)));
	//	res.a = 0.5;
#endif

#if defined(OPAQUEALPHA)
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque

#elif defined(FRESNEL) && defined(GHOST)
	col.a = clamp(fresnel + specMask, 0.0, 1.0);
#elif defined(FRESNEL) && defined(OPAQUESPECULAR)
	col.a = max(fresnel, col.a);
#endif

#if defined(TINT)
	col.a *= _Tint.a;
#endif
	return col;
}
