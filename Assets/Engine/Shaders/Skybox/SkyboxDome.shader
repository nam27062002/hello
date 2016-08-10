﻿// - Unlit
// - Scroll 2 layers /w Multiplicative op

Shader "Hungry Dragon/Skybox/Dome Skybox" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_Scroll2X ("2nd layer Scroll speed X", Float) = 1.0
	_Color("Color", Color) = (1,1,1,1)
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Opaque" }
	
	Lighting Off Fog { Mode Off }
	ZWrite Off
	
	LOD 100
	
		
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#include "UnityCG.cginc"
	#include "../HungryDragon.cginc"

	sampler2D _MainTex;
	sampler2D _DetailTex;

	float4 _MainTex_ST;
	float4 _DetailTex_ST;

	float4 _FogColor;
	float _FogStart;
	float _FogEnd;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
		HG_FOG_COORDS(2)
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex); 
		o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex);
		HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex), _FogStart, _FogEnd, _FogColor);	// Fog
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		fixed4 frag (v2f i) : COLOR
		{
			
			fixed4 tex = tex2D (_MainTex, i.uv);
			fixed4 tex2 = tex2D (_DetailTex, i.uv2);
			

			fixed4 one = fixed4(1,1,1,1);
			fixed4 col = one - (one-tex) * (one-tex2);

			HG_APPLY_FOG(i, col, _FogColor);	// Fog
			UNITY_OPAQUE_ALPHA(col.a);	// Opaque

			return col;
		}
		ENDCG 
	}	
}
}
