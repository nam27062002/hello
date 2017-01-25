﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hungry Dragon/Unlit Transparent Custom Fog (Background entities & Clouds)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags {  "Queue"="Transparent+20" "IgnoreProjector"="true" "RenderType"="Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest

			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				// float3 normal : NORMAL;
				HG_FOG_COORDS(1)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			HG_FOG_VARIABLES
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				// float3 normal = UnityObjectToWorldNormal(v.normal);
				HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
				return o;
			} 
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
     			// fixed4 diffuse = max(0,dot( i.normal, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;
     			HG_APPLY_FOG(i, col);	// Fog
				return col;
			}
			ENDCG
		}
	}
}
