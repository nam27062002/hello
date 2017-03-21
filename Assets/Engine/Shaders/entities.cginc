
struct v2f
{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	float3 vLight : TEXCOORD2;

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
};

uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
uniform float4 _MainTex_TexelSize;

#ifdef NORMALMAP
uniform sampler2D _NormalTex;
uniform float4 _NormalTex_ST;
uniform float _NormalStrength;
#endif

#ifdef SPECULAR
uniform float _SpecularPower;
#endif

#ifdef FRESNEL
uniform float _FresnelPower;
uniform float4 _FresnelColor;
#endif

#ifdef TINT
uniform float4 _Tint;
#endif


v2f vert(appdata_t v)
{
	v2f o;
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
	o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	float3 normal = UnityObjectToWorldNormal(v.normal);
	o.vLight = ShadeSH9(float4(normal, 1.0));

	// To calculate tangent world
	float4x4 modelMatrix = unity_ObjectToWorld;
	float4x4 modelMatrixInverse = unity_WorldToObject;

#ifdef NORMALMAP
	o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
	o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
	o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
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

	return o;
}

fixed4 frag(v2f i) : SV_Target
{
	// sample the texture
	fixed4 col = tex2D(_MainTex, i.uv);

#ifdef NORMALMAP
	// Calc normal from detail texture normal and tangent world
	float4 encodedNormal = tex2D(_NormalTex, i.uv);
	float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	float3 normalDirection = i.normalWorld;
#endif

	fixed4 diffuse = max(0,dot(normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;
	col *= diffuse + fixed4(i.vLight, 1);

#ifdef SPECULAR
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower);
	col += specular * _LightColor0;
#endif
//				fixed fresnel = pow(max(dot(normalDirection, i.viewDir), 0), _FresnelFactor);

#ifdef FRESNEL
	fixed fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _FresnelPower), 0.0, 1.0);
	col += fresnel * _FresnelColor;
#endif

#ifdef OPAQUEALPHA
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
#endif

#ifdef TINT
	col *= _Tint;
#endif
	return col;
}

