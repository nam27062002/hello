// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members _LightmapIntensity)
#pragma exclude_renderers d3d11


#define LIGHTMAPCONTRAST

struct v2f {
	float4 vertex : SV_POSITION;
	half2 texcoord : TEXCOORD0;
	
#ifdef BLEND_TEXTURE	
	half2 texcoord2 : TEXCOORD5;
#endif

	float4 color : COLOR;
	
#ifdef FOG	
	HG_FOG_COORDS(1)
#endif

#ifdef DARKEN
	HG_DARKEN(8)
#endif

#ifdef DYNAMIC_SHADOWS
	LIGHTING_COORDS(2,3)	
#endif

#ifdef LIGHTMAP_ON
	float2 lmap : TEXCOORD4;
#endif	

	float3 normalWorld : NORMAL;
	
#ifdef NORMALMAP
	float3 tangentWorld : TANGENT;
	float3 binormalWorld : TEXCOORD6;
#endif

#ifdef SPECULAR
	float3 halfDir : TEXCOORD7;
#endif	

};

sampler2D _MainTex;
float4 _MainTex_ST;

#ifdef BLEND_TEXTURE	
sampler2D _SecondTexture;
float4 _SecondTexture_ST;
#endif

#ifdef LIGHTMAP_ON
float _LightmapIntensity;
#endif




#ifdef NORMALMAP
uniform sampler2D _NormalTex;
uniform float4 _NormalTex_ST;
uniform float _NormalStrength;
#endif

#ifdef SPECULAR
uniform float _SpecularPower;
uniform fixed4 _SpecularDir;
#endif

#ifdef CUTOFF
uniform float _CutOff;
#endif

#ifdef FOG
HG_FOG_VARIABLES
#endif

#ifdef DARKEN
uniform float _DarkenPosition;
uniform float _DarkenDistance;
#endif

#ifdef EMISSIVEBLINK
uniform float _EmissivePower;
uniform float _BlinkTimeMultiplier;
#endif

float4 getCustomVertexColor(inout appdata_t v)
{
	//					return float4(v.color.xyz, 1.0 - dot(mul(float4(v.normal,0), unity_WorldToObject).xyz, float3(0,1,0)));
	return float4(v.color.xyz, 1.0 - dot(UnityObjectToWorldNormal(v.normal), float3(0, 1, 0)));
}


v2f vert (appdata_t v) 
{
	v2f o;

#ifdef CUSTOM_VERTEXPOSITION
	o.vertex = getCustomVertexPosition(v);
#else
	o.vertex = UnityObjectToClipPos(v.vertex);
#endif

	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
	
#ifdef BLEND_TEXTURE	
	o.texcoord2 = TRANSFORM_TEX(v.texcoord, _SecondTexture);
#endif

#ifdef CUSTOM_VERTEXCOLOR
	o.color = getCustomVertexColor(v);
#else
	o.color = v.color;
#endif // CUSTOM_VERTEXCOLOR


#if defined (FOG) || defined (DARKEN) || defined (SPECULAR)
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
#endif // FOG || DARKEN || SPECULAR

#ifdef FOG	
	HG_TRANSFER_FOG(o, worldPos);	// Fog
#endif // FOG

#ifdef DARKEN
	HG_TRANSFER_DARKEN(o, worldPos);
#endif // DARKEN
/*
#ifdef DYNAMIC_SHADOWS
	TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
#endif
*/

#if defined(LIGHTMAP_ON) && !defined(EMISSIVEBLINK)
	o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
#endif // LIGHTMAP_ON && !EMISSIVEBLINK

#ifdef NORMALMAP																		// To calculate tangent world
	float4x4 modelMatrix = unity_ObjectToWorld;
	float4x4 modelMatrixInverse = unity_WorldToObject;
	o.normalWorld = UnityObjectToWorldNormal(v.normal);
	o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
	o.normalWorld = UnityObjectToWorldNormal(v.normal);
#endif // NORMALMAP

#ifdef SPECULAR
//	fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	// Half View - See: Blinn-Phong
	float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
	float3 lightDirection = normalize(_SpecularDir.rgb);
	o.halfDir = normalize(lightDirection + viewDirection);	
#endif // SPECULAR

	return o;
}


fixed4 frag (v2f i) : SV_Target
{	
#ifdef DEBUG
	return fixed4(1.0, 0.0, 1.0, 1.0);
#endif	

//#ifdef DARKEN
//	return fixed4(0.0, 0.0, 1.0, 1.0);
//#endif

	fixed4 col = tex2D(_MainTex, i.texcoord);	// Color

#ifdef CUTOFF
	clip(col.a - _CutOff);
#endif // CUTOFF

#ifdef SPECULAR
	float specMask = col.w;
#endif // SPECULAR
	
#ifdef BLEND_TEXTURE
	fixed4 col2 = tex2D(_SecondTexture, i.texcoord2);	// Color
	float l = saturate( col.a + ( (i.color.a * 2) - 1 ) );
//					float l = clamp(col.a + (i.color.a * 2.0) - 1.0, 0.0, 1.0);
	col = lerp( col2, col, l);
#endif // BLEND_TEXTURE

#if defined (VERTEXCOLOR_OVERLAY)
	// Sof Light with vertex color 
	// http://www.deepskycolors.com/archive/2010/04/21/formulas-for-Photoshop-blending-modes.html
	// https://en.wikipedia.org/wiki/Relative_luminance
	float luminance = step(0.5, 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b);
	fixed4 one = fixed4(1, 1, 1, 1);
	col = (2.0 * i.color * col) * (1.0 - luminance) + (one - 2.0 * (one - i.color) * (one - col)) * luminance;

#elif defined (VERTEXCOLOR_ADDITIVE)
	col += i.color;

#elif defined (VERTEXCOLOR_MODULATE)
	col *= i.color;
#endif // VERTEXCOLOR

/*
#ifdef DYNAMIC_SHADOWS
	float attenuation = LIGHT_ATTENUATION(i);	// Shadow
	col *= attenuation;
#endif
*/

#if defined(LIGHTMAP_ON) && !defined(EMISSIVEBLINK)
	fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
//	col.rgb *= lm * 1.3;
	col.rgb *= lm * 1.3;
#endif // LIGHTMAP_ON && !EMISSIVEBLINK

#ifdef NORMALMAP
	float4 encodedNormal = tex2D(_NormalTex, i.texcoord);
	float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	float3 normalDirection = i.normalWorld;
#endif // NORMALMAP

#ifdef SPECULAR
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower);
	col = col + (specular * specMask * i.color * _LightColor0);
#endif // SPECULAR

#ifdef EMISSIVEBLINK
	float intensity = 1.0 + (1.0 + sin((_Time.y * _BlinkTimeMultiplier) + i.vertex.x * 0.01 )) * _EmissivePower;
	col *= intensity;
#endif // EMISSIVEBLINK

#if defined(FOG) && !defined(EMISSIVEBLINK)

#if defined (LIGHTMAP_ON)

#ifdef LIGHTMAPCONTRAST
	fixed4 fogCol = tex2D(_FogTexture, i.fogCoord);
	lm -= 0.5;
	float lmi = (0.2126 * lm.r + 0.7152 * lm.g + 0.0722 * lm.b) * _LightmapIntensity * (1.0 + (1.0 + sin((_Time.y * 2.0) + i.vertex.x * 0.01)));
	col.rgb = lerp((col).rgb, fogCol.rgb, clamp(fogCol.a - lmi, 0.0, 1.0));
#else
	HG_APPLY_FOG(i, col);	// Fog
#endif // LIGHTMAPCONTRAST

#else
	HG_APPLY_FOG(i, col);	// Fog
#endif // LIGHTMAP_ON

#endif	// FOG && !EMISSIVEBLINK

#ifdef DARKEN
	HG_APPLY_DARKEN(i, col);	//darken
#endif // DARKEN

#ifdef OPAQUEALPHA
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
#endif // OPAQUEALPHA
	return col;
}
