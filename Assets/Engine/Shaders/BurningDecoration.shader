// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Hungry Dragon/Burning Decoration" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_BurnMask("Burn Mask", 2D) = "white" {}
	_ColorRamp("Color Ramp", 2D) = "white" {}

	_BurnLevel("Burn Level", Range(0.0, 3.0)) = 0.0
	_BurnWidth("Burn Width", Range(0.0, 0.1)) = 0.02
	_BurnMaskScale("Burn Mask Scale", Range(1.0, 8.0)) = 1.0
}

SubShader {
	Tags {"Queue"="Transparent" "RenderType"="Transparent"}
	LOD 100
	
	ZWrite on
	Blend SrcAlpha OneMinusSrcAlpha 
	ColorMask RGBA
	
	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ LIGHTMAP_ON

			#include "UnityCG.cginc"
			#include "HungryDragon.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"


			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
#ifdef LIGHTMAP_ON
				float4 texcoord1 : TEXCOORD1;
#endif

			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
#ifdef LIGHTMAP_ON
				float2 lmap : TEXCOORD1;
#endif	
				HG_FOG_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BurnMask;
			sampler2D _ColorRamp;

			float _BurnLevel;
			float _BurnWidth;
			float _BurnMaskScale;

			HG_FOG_VARIABLES

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

#if defined(LIGHTMAP_ON)
				o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;	// Lightmap
#endif

				HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed fragAlpha = tex2D(_BurnMask, i.texcoord * _BurnMaskScale).r - (_BurnLevel - 2.0);

				clip(fragAlpha);	// Remove ashes pixels

				fixed4 col = tex2D(_MainTex, i.texcoord);

#if defined(LIGHTMAP_ON)
				fixed3 lm = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));	// Lightmap
				col.rgb *= lm * 1.3;
#endif


				fixed burnedFactor = tex2D(_BurnMask, i.texcoord * _BurnMaskScale).r - _BurnLevel;

				fixed c1 = step(-0.7, burnedFactor);
				fixed c2 = step(-2.0, burnedFactor);

				fixed delta = 1.0 - (burnedFactor + 0.7) / -0.3f;
				col = lerp(col, col * fixed4(delta, 0, 0, 1), 1.0 - c1);
				delta = 1.0 - burnedFactor / -0.7f;
				col = lerp(col, col * fixed4(1, delta, delta, 1), 1.0 - c2);

				fixed idxBurn = clamp(fragAlpha / _BurnWidth, 0.0, 1.0);
				col = max(col, tex2D(_ColorRamp, float2(idxBurn, 0.0)));

				HG_APPLY_FOG(i, col);	// Fog
				return col;
			}
		ENDCG
	}
}

}
