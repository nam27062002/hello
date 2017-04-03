
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

#ifdef NORMALMAP
uniform sampler2D _NormalTex;
uniform float4 _NormalTex_ST;
uniform float _NormalStrength;
#endif

#ifdef SPECULAR
uniform float _Specular;
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


v2f vert (appdata_t v) 
{
	v2f o;

#ifdef CUSTOM_VERTEXPOSITION
	o.vertex = getCustomVertexPosition(v);
#else
	o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
#endif

	o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
	
#ifdef BLEND_TEXTURE	
	o.texcoord2 = TRANSFORM_TEX(v.texcoord, _SecondTexture);
#endif

#ifdef CUSTOM_VERTEXCOLOR
	o.color = getCustomVertexColor(v);
#else
	o.color = v.color;
#endif


#if defined (FOG) || defined (DARKEN) || defined (SPECULAR)
	float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
#endif

#ifdef FOG	
	HG_TRANSFER_FOG(o, worldPos);	// Fog
#endif

#ifdef DARKEN
	HG_TRANSFER_DARKEN(o, worldPos);
#endif

#ifdef DYNAMIC_SHADOWS
	TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
#endif

#ifdef LIGHTMAP_ON
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

	fixed4 col = tex2D(_MainTex, i.texcoord);	// Color

#ifdef CUTOFF
	clip(col.a - _CutOff);
#endif

#ifdef SPECULAR
	float specMask = col.w;
#endif
	
#ifdef BLEND_TEXTURE
	fixed4 col2 = tex2D(_SecondTexture, i.texcoord2);	// Color
	float l = saturate( col.a + ( (i.color.a * 2) - 1 ) );
//					float l = clamp(col.a + (i.color.a * 2.0) - 1.0, 0.0, 1.0);
	col = lerp( col2, col, l);
#endif	

#if defined (COLOR_OVERLAY)
	// Sof Light with vertex color 
	// http://www.deepskycolors.com/archive/2010/04/21/formulas-for-Photoshop-blending-modes.html
	// https://en.wikipedia.org/wiki/Relative_luminance
	float luminance = step(0.5, 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b);
	fixed4 one = fixed4(1, 1, 1, 1);
	col = (2.0 * i.color * col) * (1.0 - luminance) + (one - 2.0 * (one - i.color) * (one - col)) * luminance;

#elif defined (COLOR_ADDITIVE)
	col += i.color;

#elif defined (COLOR_MODULATE)
	col *= i.color;
#endif	


#ifdef DYNAMIC_SHADOWS
	float attenuation = LIGHT_ATTENUATION(i);	// Shadow
	col *= attenuation;
#endif

#ifdef LIGHTMAP_ON
	fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
	col.rgb *= lm;
#endif

#ifdef FOG	
	HG_APPLY_FOG(i, col);	// Fog
#endif	

#ifdef DARKEN
	HG_APPLY_DARKEN(i, col);	//darken
#endif

#ifdef NORMALMAP
	float4 encodedNormal = tex2D(_NormalTex, i.texcoord);
	float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
	float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
	float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));
#else
	float3 normalDirection = i.normalWorld;
#endif

#ifdef SPECULAR
	fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _Specular);
	col = col + (specular * specMask * i.color * _LightColor0);
#endif	

#ifdef OPAQUEALPHA
	UNITY_OPAQUE_ALPHA(col.a);	// Opaque
#endif
	return col;
}
