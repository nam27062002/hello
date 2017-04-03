// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Hungry Dragon/Bumped Diffuse Transparent (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_SpecularPower( "Specular power", float ) = 1
		_FresnelPower("Fresnel power", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_Tint( "Tint", color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull back
		ColorMask RGBA

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#define HG_ENTITIES

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#define FRESNEL
			#define TINT

			#include "entities.cginc"

/*
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
//				float3 normal : NORMAL;

				float3 vLight : TEXCOORD2;

				float3 viewDir : VECTOR;
				float3 tangentWorld : TANGENT;
				float3 normalWorld : TEXCOORD4;
				float3 binormalWorld : TEXCOORD5;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _Specular;
			uniform float _BumpStrength;
			uniform float _FresnelFactor;
			uniform float4 _FresnelColor;

			uniform float4 _Tint;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 normal = UnityObjectToWorldNormal(v.normal);
				o.vLight = ShadeSH9(float4(normal, 1.0));

				// Half View - See: Blinn-Phong
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
				//				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				//				o.halfDir = normalize(lightDirection + viewDirection);
				o.viewDir = viewDirection;

				o.normalWorld = normal;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 normalDirection = i.normalWorld;

//     			float3 normalDirection = i.normal;

     			fixed4 diffuse = max(0,dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;

				fixed fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _FresnelFactor), 0.0, 1.0);

				// col = (diffuse + fixed4(UNITY_LIGHTMODEL_AMBIENT.rgb,1)) * col + specular * _LightColor0;
				col = ((diffuse + fixed4(i.vLight, 1)) * col + (fresnel * _FresnelColor)) * _Tint;
//				col.a = clamp(_Tint.a + fresnel * _Tint.a, 0.0, 1.0);
				col.a = clamp((col.a + fresnel), 0.0, 1.0);

				return col;
			}
*/
			ENDCG
		}
	}
}
