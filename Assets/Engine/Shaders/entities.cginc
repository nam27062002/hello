
struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;

	float4 color : COLOR;

#ifdef SPECULAR
	float3 halfDir : TEXCOORD7;
#endif	

#ifdef FRESNEL
	float3 viewDir : VECTOR;
#endif

	float3 normalWorld : TEXCOORD4;
#ifdef NORMALMAP
	float3 tangentWorld : TANGENT;
	float3 binormalWorld : TEXCOORD5;
#endif

#ifdef CUSTOM_ALPHA
	float height : TEXCOORD3;
#endif 

};

uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
uniform float4 _MainTex_TexelSize;

#ifdef NORMALMAP
uniform sampler2D _NormalTex;
uniform float4 _NormalTex_ST;
uniform float _NormalStrength;
#endif

#ifdef CUSTOM_ALPHA
uniform sampler2D _AlphaTex;
uniform float4 _AlphaTex_ST;
uniform float _AlphaMSKScale;
uniform float _AlphaMSKOffset;
#endif

#ifdef SPECULAR
uniform float _SpecularPower;
uniform float4 _SpecularColor;
#endif

#ifdef FRESNEL
uniform float _FresnelPower;
uniform float4 _FresnelColor;
#endif

#if defined (TINT) || defined (CUSTOM_TINT)
uniform float4 _Tint;
#endif


v2f vert(appdata_t v)
{
	v2f o;

#ifdef CUSTOM_VERTEXPOSITION
	o.vertex = getCustomVertexPosition(v);
#else
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif

	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	float3 normal = UnityObjectToWorldNormal(v.normal);

	// To calculate tangent world
#ifdef NORMALMAP
	o.tangentWorld = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
//	o.normalWorld = normal;
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
//	o.normalWorld = normal;
#endif

	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);

#ifdef SPECULAR
	// Half View - See: Blinn-Phong
	//	fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
	o.halfDir = normalize(lightDirection + viewDirection);
#endif

#ifdef FRESNEL
	o.viewDir = viewDirection;
#endif

	o.color = v.color;

#ifdef CUSTOM_ALPHA
	o.height = v.vertex.y;
#endif

	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	// sample the texture
	fixed4 col = tex2D(_MainTex, i.uv);
//	fixed specMask = col.a;
	fixed specMask = 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
	fixed emmisiveMask = col.a;
//	col.a = 1.0;

#ifdef NORMALMAP
	// Calc normal from detail texture normal and tangent world
	float4 encodedNormal = tex2D(_NormalTex, i.uv);
	float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	float3 normalDirection = i.normalWorld;
#endif

#if defined (TINT)
	col += _Tint;
#elif defined (CUSTOM_TINT)
	col = getCustomTint(col, _Tint, i.color);
#endif

	fixed4 unlitColor = col;

	fixed4 diffuse = max(0,dot(normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;
	col *= diffuse;

#ifdef SPECULAR
//	specMask = 1.0;
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower) * specMask;
	col += specular * (col + _SpecularColor * 2.0);
#endif
//				fixed fresnel = pow(max(dot(normalDirection, i.viewDir), 0), _FresnelFactor);

#ifdef FRESNEL
	fixed fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _FresnelPower), 0.0, 1.0);
	col += fresnel * _FresnelColor;
#endif

	col = lerp(col, unlitColor, emmisiveMask);

#if defined (OPAQUEALPHA)
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
#elif defined (CUSTOM_ALPHA)

//	#define TEX_ALPHA_SCALE 3.0

//	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
	float st = smoothstep(_AlphaMSKOffset - 0.3, _AlphaMSKOffset, i.height);
//	return st;
//	float s1 = 0.5 + sin(_Time.y * 5.0) * 0.45;
//	float s2 = 0.5 + sin(_Time.y * 8.0) * 0.45;
	float2 off = float2(0.3333, _Time.y * 0.75);
	float alpha = tex2D(_AlphaTex, (i.uv * _AlphaMSKScale) + off).w;
//	alpha += tex2D(_AlphaTex, (i.uv * _AlphaMSKScale) + off * 2.0).w;
//	alpha *= 0.55;
	col.a = clamp(st + alpha, 0.0, 1.0);
//	clip(st + alpha - 0.1);
#endif
	return col;
}

