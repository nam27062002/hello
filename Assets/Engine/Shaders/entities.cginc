// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


struct appdata_t
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

struct v2f
{
	float4 vertex : SV_POSITION;

	float2 uv : TEXCOORD0;
	float4 color : COLOR;

#if !defined(GHOST)
	float3 vLight : TEXCOORD2;
#endif

#ifdef SPECULAR
	float3 halfDir : TEXCOORD7;
#endif

#if defined(FRESNEL) || defined(FREEZE)
	float3 viewDir : VECTOR;
#endif

	float3 normalWorld : NORMAL;
#ifdef NORMALMAP
	float3 tangentWorld : TANGENT;
	float3 binormalWorld : TEXCOORD5;
#endif

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

#ifdef NORMALMAP
uniform sampler2D _NormalTex;
uniform float4 _NormalTex_ST;
uniform float _NormalStrength;
#endif

#ifdef SPECULAR
uniform float _SpecularPower;
uniform float4 _SpecularColor;
#endif

#if defined(FRESNEL) || defined(FREEZE)

uniform float _FresnelPower;

uniform float4 _FresnelColor;
#ifdef FREEZE
uniform float4 _FresnelColor2;
#endif

#endif

#if defined(TINT)
uniform float4 _Tint;
#endif

#if defined(EMISSIVE)
uniform float _EmissiveIntensity;
uniform float _EmissiveBlink;
#endif

#if defined(VERTEX_ANIMATION)
uniform float4 _VertexAnimation;
uniform float _AnimationPhase;
#endif

v2f vert(appdata_t v)
{
	v2f o;

#if defined(VERTEX_ANIMATION)
	float4 anim = sin(_Time.y * _AnimationPhase + v.vertex * 30.0);
	v.vertex += anim * _VertexAnimation * v.color.a;
#endif

	o.vertex = UnityObjectToClipPos(v.vertex);

	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	float3 normal = UnityObjectToWorldNormal(v.normal);

#if !defined(GHOST)
	o.vLight = ShadeSH9(float4(normal, 1.0));
#endif

	// To calculate tangent world
#ifdef NORMALMAP
	o.tangentWorld = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
//	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	o.normalWorld = normal;
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
//	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	o.normalWorld = normal;
#endif

	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

#if defined(FRESNEL) || defined(FREEZE) || defined(SPECULAR)
	float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
#endif

#ifdef SPECULAR
	// Half View - See: Blinn-Phong
	//	fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
	o.halfDir = normalize(lightDirection + viewDirection);
#endif

#if defined(FRESNEL) || defined(FREEZE)
	o.viewDir = viewDirection;
#endif

	o.color = v.color;

#if defined(MATCAP) || defined(FREEZE)
	float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
	worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
	o.cap.xy = worldNorm.xy * 0.5 + 0.5;
#endif

	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	// sample the texture
	fixed4 col = tex2D(_MainTex, i.uv);
	fixed specMask = col.a;// 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;

#if defined(EMISSIVE)
	float anim = (sin(_Time.y * _EmissiveBlink) + 1.0) * 0.5 * _EmissiveIntensity * col.a;
	col.xyz *= 1.0 + anim;
#endif

#if defined (TINT)
	col.xyz *= _Tint.xyz;
#endif

#ifdef NORMALMAP
	// Calc normal from detail texture normal and tangent world
	float4 encodedNormal = tex2D(_NormalTex, i.uv);
	float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	float3 normalDirection = i.normalWorld;
#endif

#if !defined(GHOST)
	fixed3 diffuse = max(0,dot(normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0.xyz;
	col.xyz *= diffuse + i.vLight;
#endif

#ifdef SPECULAR
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower) * specMask;
	col.xyz += specular * (col.xyz + _SpecularColor.xyz * 2.0);
#endif

#if defined(FRESNEL) || defined(FREEZE)
	fixed fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _FresnelPower), 0.0, 1.0) * _FresnelColor.w;
//	col.xyz *= lerp(_FresnelColor2.xyz, _FresnelColor.xyz, fresnel);

#ifdef FREEZE
	col.xyz *= _FresnelColor2.xyz;
#endif

	col.xyz += _FresnelColor.xyz * fresnel;
#endif

#if defined(MATCAP) || defined (FREEZE)
	fixed4 mc = tex2D(_MatCap, i.cap) * _GoldColor; // _FresnelColor;

//	col = (col + ((mc*2.0) - 0.5));
	col = lerp(col, mc * 3.0, _GoldColor.w);// (1.0 - clamp(_FresnelPower, 0.0, 1.0)));
	//	res.a = 0.5;
#endif

#if defined (OPAQUEALPHA)
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
#endif

#if defined(GHOST)
	col.a = clamp(fresnel + specMask, 0.0, 1.0);
#endif

#if defined (TINT)
	col.a *= _Tint.a;
#endif
	return col;
}

