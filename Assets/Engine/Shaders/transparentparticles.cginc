// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members _LightmapIntensity)
#pragma exclude_renderers d3d11

struct appdata_t {
	float4 vertex : POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
};

struct v2f {
	float4 vertex : SV_POSITION;
	float2 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
};

sampler2D _MainTex;
float4 _MainTex_ST;

float4 _TintColor;

#ifdef EMISSIVEPOWER
float _EmissivePower;
#endif

#ifdef AUTOMATICPANNING
float4 _Panning;
#endif

v2f vert (appdata_t v) 
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.color = v.color;

#ifdef AUTOMATICPANNING
	v.texcoord += _Panning.xy * _Time.y;
#endif

	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

	return o;
}

fixed4 frag (v2f i) : SV_Target
{	
#ifdef DEBUG
	return fixed4(1.0, 0.0, 1.0, 1.0);
#endif	

#ifdef EMISSIVEPOWER
	half4 prev = i.color * tex2D(_MainTex, i.texcoord) * _EmissivePower;
#else
	half4 prev = i.color * tex2D(_MainTex, i.texcoord);
#endif

	prev *= _TintColor;

#if defined(SOFTADDITIVE)
	prev.rgb *= prev.a;
#elif defined(ALPHABLEND)
	prev *= 2.0;
#endif

	return prev;
}
