// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members _LightmapIntensity)
#pragma exclude_renderers d3d11

struct appdata_t
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;

#if defined(LIGHTMAP_ON) && defined(FORCE_LIGHTMAP)
	float4 texcoord1 : TEXCOORD1;
#endif

	float4 color : COLOR;

	float3 normal : NORMAL;
	float4 tangent : TANGENT;
};

//#define EMISSIVE_LIGHTMAPCONTRAST
struct v2f {
	float4 vertex : SV_POSITION;
#ifdef MAINCOLOR_TEXTURE
	float2 texcoord : TEXCOORD0;
#endif

#if defined(LIGHTMAP_ON) && defined(FORCE_LIGHTMAP)
	float2 lmap : TEXCOORD1;
#endif	

#ifdef BLEND_TEXTURE	
	float2 texcoord2 : TEXCOORD2;
#endif

#ifdef EMISSIVE_REFLECTIVE
	float3 reflectionData : TEXCOORD4;
#endif

	float4 color : COLOR;

#ifdef FOG	
	HG_FOG_COORDS(3)
#endif

#ifdef DYNAMIC_SHADOWS
	LIGHTING_COORDS(2, 3)
#endif
	float3 normalWorld : NORMAL;

#ifdef NORMALMAP
	float3 tangentWorld : TANGENT;
	float3 binormalWorld : TEXCOORD5;
#endif

#ifdef SPECULAR
	float3 halfDir : TEXCOORD6;
#endif	

};

#ifdef MAINCOLOR_TEXTURE
sampler2D _MainTex;
float4 _MainTex_ST;

#endif

float4 _Panning;
float4 _Tint;

#ifdef BLEND_TEXTURE	
sampler2D _SecondTexture;
float4 _SecondTexture_ST;
#endif

/*
#ifdef LIGHTMAP_ON
float _LightmapIntensity;
#endif
*/

#ifdef NORMALMAP
uniform sampler2D _NormalTex;
//uniform float4 _NormalTex_ST;
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

#if defined(EMISSIVE_BLINK) || defined(EMISSIVE_CUSTOM)
uniform float _EmissivePower;
uniform float _BlinkTimeMultiplier;

#if defined(WAVE_EMISSION)
uniform float _WaveEmission;
#endif

#elif defined(EMISSIVE_REFLECTIVE)
uniform sampler2D _ReflectionMap;
uniform float4 _ReflectionMap_ST;
//uniform float4 _ReflectionColor;
uniform float _ReflectionAmount;

#elif defined(EMISSIVE_LIGHTMAPCONTRAST)
uniform float _LightmapContrastIntensity;
uniform float _LightmapContrastMargin;
uniform float _LightmapContrastPhase;
#endif


//Used by plants, simulates wind movement
#if defined(CUSTOM_VERTEXPOSITION)
float _SpeedWave;
float _Amplitude;
float4 getCustomVertexPosition(inout appdata_t v)
{
	float hMult = v.vertex.y;
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	float wave = sin((_Time.y * _SpeedWave) + hMult + worldPos.x) * hMult * _Amplitude;

	//float4 tvertex = v.vertex + float4(sin((_Time.y * hMult * _SpeedWave ) * 0.525) * hMult * 0.08, 0.0, 0.0, 0.0f);
	float4 tvertex = v.vertex + float4(wave, 0.0, wave, 0.0f);
	//					tvertex.w = -0.5f;
	return UnityObjectToClipPos(tvertex);
}
#endif

//used by automatic blend to blend grass and stone from up to down
#if defined(CUSTOM_VERTEXCOLOR)
float4 getCustomVertexColor(inout appdata_t v)
{
	//					return float4(v.color.xyz, 1.0 - dot(mul(float4(v.normal,0), unity_WorldToObject).xyz, float3(0,1,0)));
	return float4(v.color.xyz, 1.0 - dot(UnityObjectToWorldNormal(v.normal), float3(0, 1, 0)));
}
#endif

v2f vert (appdata_t v) 
{
	v2f o;

#ifdef CUSTOM_VERTEXPOSITION
	o.vertex = getCustomVertexPosition(v);
#else
	o.vertex = UnityObjectToClipPos(v.vertex);
#endif

#ifdef MAINCOLOR_TEXTURE
	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex) + (_Time.y * _Panning.xy);
#endif

#ifdef BLEND_TEXTURE	
	o.texcoord2 = TRANSFORM_TEX(v.texcoord, _SecondTexture) + (_Time.y * _Panning.zw);
#endif

#ifdef CUSTOM_VERTEXCOLOR
	o.color = getCustomVertexColor(v);
#else
	o.color = v.color;
#endif

#ifdef EMISSIVE_REFLECTIVE
	o.reflectionData.xy = TRANSFORM_TEX(v.texcoord, _ReflectionMap);
	o.reflectionData.z = v.color.a;
#endif

#if defined (FOG) || defined (SPECULAR)
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
#endif

#ifdef FOG	
	HG_TRANSFER_FOG(o, worldPos);	// Fog
#endif
/*
#ifdef EMISSIVE_REFLECTIVE
//	float3 worldNorm = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
//	worldNorm = mul((float3x3)UNITY_MATRIX_V, worldNorm);
//	o.cap.xy = worldNorm.xy * 0.5 + 0.5;
	float s = sin(_Time.y * 3.0 + o.vertex.x * 24.0 * o.vertex.y * 32.0);
	float c = cos(_Time.y * 2.0 + o.vertex.y * 24.0 + o.vertex.x * 32.0);
	o.cap = v.texcoord;//normalize(float2(s, c));
#endif
*/
#ifdef DYNAMIC_SHADOWS
	TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
#endif

#if defined(LIGHTMAP_ON) && defined(FORCE_LIGHTMAP) //&& !defined(EMISSIVE_BLINK)
	o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
#endif

#ifdef NORMALMAP																		// To calculate tangent world
	float4x4 modelMatrix = unity_ObjectToWorld;
	float4x4 modelMatrixInverse = unity_WorldToObject;
	o.normalWorld = UnityObjectToWorldNormal(v.normal);
	o.tangentWorld = normalize(mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
	o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
#else
	o.normalWorld = UnityObjectToWorldNormal(v.normal);
#endif

#ifdef SPECULAR
//	fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	// Half View - See: Blinn-Phong
	float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
	float3 lightDirection = normalize(_SpecularDir.rgb);
	o.halfDir = normalize(lightDirection + viewDirection);	
#endif

	return o;
}

fixed4 frag (v2f i) : SV_Target
{	
#ifdef DEBUG
	return fixed4(1.0, 0.0, 1.0, 1.0);
#endif	

#ifdef MAINCOLOR_TEXTURE
	fixed4 col = tex2D(_MainTex, i.texcoord);	// Color
#else
	fixed4 col = _Tint;
#endif

#ifdef CUTOFF
	clip(col.a - _CutOff);
#endif

	float diffuseAlpha = col.w;
	
#ifdef BLEND_TEXTURE
	fixed4 col2 = tex2D(_SecondTexture, i.texcoord2);	// Color
#ifdef ADDITIVE_BLEND			//Used in fog_tirilla_background to see night sky stars
	col += col2 * (1.0 - i.color.a);
#else
	float l = saturate( col.a + ( (i.color.a * 2) - 1 ) );
	col = lerp( col2, col, l);
#endif


#endif	

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
#endif	

#if defined(EMISSIVE_REFLECTIVE)
	float c = cos(_Time.y * 4.0 + i.reflectionData.x * 4.0 + i.reflectionData.y * 4.0);
	float s = sin(_Time.y * 4.0 + i.reflectionData.x * 4.0 + i.reflectionData.y * 4.0);
	float2 uvc = i.reflectionData.xy + float2(s, c) * 0.09;
//	fixed4 mc = tex2D(_ReflectionMap, uvc) * _ReflectionColor * 3.0;
	fixed4 mc = tex2D(_ReflectionMap, uvc) * 3.0;
//	col = lerp(col, mc, _ReflectionAmount * i.reflectionData.z);
	col += mc * _ReflectionAmount * i.reflectionData.z;
#endif

#ifdef DYNAMIC_SHADOWS
	float attenuation = LIGHT_ATTENUATION(i);	// Shadow
	col *= attenuation;
#endif


#if defined(LIGHTMAP_ON) && defined(FORCE_LIGHTMAP) && defined(EMISSIVE_LIGHTMAPCONTRAST)
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
	fixed lmintensity = (lm.r + lm.g + lm.b) - _LightmapContrastMargin;
//	col.rgb *= lm * _LightmapContrastIntensity * (1.0 + sin((_Time.y * 2.0) + i.vertex.x * 0.01)) * (lm.r + lm.g + lm.b);
	col.rgb *= lm * (1.0 + (lmintensity * _LightmapContrastIntensity * (sin(_Time.y * _LightmapContrastPhase) + 1.0)));
//	col.rgb *= lm * (1.0 + (lmintensity * _LightmapContrastIntensity));

#elif defined(LIGHTMAP_ON) && defined(FORCE_LIGHTMAP)// && !defined(EMISSIVE_BLINK)
	fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap

#if defined(EMISSIVE_BLINK)// || defined(EMISSIVE_REFLECTIVE)
	col.xyz = lerp(col.xyz * lm * 1.3, col.xyz, diffuseAlpha);
#else
	col.rgb *= lm * 1.3;
#endif

#endif

#if defined(NORMALMAP) && defined(MAINCOLOR_TEXTURE)
	float4 encodedNormal = tex2D(_NormalTex, i.texcoord);
	float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	float3 normalDirection = i.normalWorld;
#endif

#ifdef SPECULAR
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower);
	col = col + (specular * diffuseAlpha * i.color * _LightColor0);
#endif	

#if defined(EMISSIVE_CUSTOM)
	diffuseAlpha = frac(diffuseAlpha + (1.0 / 255.0));
#endif

#if defined(EMISSIVE_BLINK) || defined(EMISSIVE_CUSTOM)

#if defined(WAVE_EMISSION)
	float intensity = 1.0 + (1.0 + sin((_Time.y * _BlinkTimeMultiplier) + i.vertex.x * _WaveEmission)) * _EmissivePower * diffuseAlpha;
#else 
	float intensity = 1.0 + (1.0 + sin(_Time.y * _BlinkTimeMultiplier)) * _EmissivePower * diffuseAlpha;

#endif
	col *= intensity;
#endif

/*
#if defined(FOG) && !defined(EMISSIVE_BLINK)

#if defined (LIGHTMAP_ON && FORCE_LIGHTMAP)

#ifdef EMISSIVE_LIGHTMAPCONTRAST
	fixed4 fogCol = tex2D(_FogTexture, i.fogCoord);
	lm -= 0.5;
//	float lmi = 0.0f;// (0.2126 * lm.r + 0.7152 * lm.g + 0.0722 * lm.b) * _LightmapContrastIntensity * (1.0 + (1.0 + sin((_Time.y * 2.0) + i.vertex.x * 0.01)));
//	float lmi = (0.2126 * lm.r + 0.7152 * lm.g + 0.0722 * lm.b) * _LightmapContrastIntensity * (1.0 + (1.0 + sin((_Time.y * 2.0) + i.vertex.x * 0.01)));
	float lmi = (lm.r + lm.g + lm.b) * _LightmapContrastIntensity * (1.0 + (1.0 + sin((_Time.y * 2.0) + i.vertex.x * 0.01)));
	col.rgb = lerp((col).rgb, fogCol.rgb, clamp(fogCol.a - lmi, 0.0, 1.0));
#else
	HG_APPLY_FOG(i, col);	// Fog
#endif	//  EMISSIVE_LIGHTMAPCONTRAST

#else
	HG_APPLY_FOG(i, col);	// Fog
#endif	// LIGHTMAP_ON && FORCE_LIGHTMAP

#endif	// defined(FOG) && !defined(EMISSIVE_BLINK)
*/

#if defined(FOG)// && !defined(EMISSIVE_BLINK)

#if defined(EMISSIVE_BLINK) || defined(EMISSIVE_CUSTOM)// || defined(EMISSIVE_REFLECTIVE)
	fixed4 colc = col;
	HG_APPLY_FOG(i, col);	// Fog
	col = lerp(col, colc, diffuseAlpha);
#else
	HG_APPLY_FOG(i, col);	// Fog
#endif

#endif

#ifdef OPAQUEALPHA
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
#endif

#if !defined(MAINCOLOR_TEXTURE) && defined(TINT)
	col *= _Tint;
#endif

	return col;
}

