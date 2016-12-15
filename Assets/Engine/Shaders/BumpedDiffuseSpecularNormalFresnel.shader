// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/NormalMap + Diffuse + Specular + Fresnel (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_NormalTex("Normal (RGBA)", 2D) = "white" {}
		_EmissiveTex("Emissive (RGBA)", 2D) = "white" {}
		_NormalStrength("Normal Strength", float) = 3
		_SpecularPower( "Specular Power", float ) = 1
		_SpecularDir("Specular Dir", Vector) = (0,0,-1,0)
		_FresnelFactor("Fresnel factor", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_EmissiveColor("Emissive color (RGB)", Color) = (0, 0, 0, 0)

	}


	SubShader
	{
		Pass
		{
			Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase"}
			Cull Back

			Stencil
			{
				Ref 5
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag


			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

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
				float2 uv2 : TEXCOORD3;
				float4 vertex : SV_POSITION;
//				float3 vLight : TEXCOORD2;

				float3 viewDir : TEXTCOORD1;
				float3 halfDir : TEXTCOORD2;
				float3 tangentWorld : TANGENT;  
		        float3 normalWorld : TEXCOORD4;
		        float3 binormalWorld : TEXCOORD5;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform sampler2D _NormalTex;
			uniform float4 _NormalTex_ST;
			uniform sampler2D _EmissiveTex;
			uniform float _SpecularPower;
			uniform fixed4 _SpecularDir;
			uniform float _NormalStrength;
			uniform float _FresnelFactor;
			uniform float4 _FresnelColor;
			uniform float4 _EmissiveColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv, _NormalTex);
				fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 normal = UnityObjectToWorldNormal(v.normal);
//				o.vLight = ShadeSH9(float4(normal, 1.0));

				// Half View - See: Blinn-Phong
				float3 viewDirection = normalize(_WorldSpaceCameraPos - worldPos.xyz);
//				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
//				o.halfDir = normalize(lightDirection + viewDirection);
				o.viewDir = viewDirection;
				float3 lightDirection = normalize(_SpecularDir.rgb);
				o.halfDir = normalize(lightDirection + viewDirection);


				// To calculate tangent world
	            float4x4 modelMatrix = unity_ObjectToWorld;
     			float4x4 modelMatrixInverse = unity_WorldToObject; 
	            o.tangentWorld = normalize( mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
     			o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
     			o.binormalWorld = normalize( cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				float specMask = col.w;

	            float3 encodedNormal = tex2D(_NormalTex, i.uv2);
				float3 localCoords = float3(2.0 * encodedNormal.xy - float2(1.0, 1.0), 1.0 / _NormalStrength);
				float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
     			float3 normalDirection = normalize(mul(localCoords, local2WorldTranspose));

				fixed4 diffuse = max(0, dot(normalDirection, normalize(_SpecularDir.xyz))) * _LightColor0;

     			fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _SpecularPower);
//				fixed fresnel = pow(max(dot(normalDirection, i.viewDir), 0), _FresnelFactor);
				fixed fresnel = clamp(pow(max(dot(i.viewDir, normalDirection), 0.0), _FresnelFactor), 0.0, 1.0);

				col = diffuse * col + (specular * _LightColor0) + (fresnel * _FresnelColor);

				float3 emissive = tex2D(_EmissiveTex, i.uv2);
				col = lerp(col, _EmissiveColor, emissive.r + emissive.g + emissive.b);


				UNITY_OPAQUE_ALPHA(col.a);	// Opaque
				return col;
			}
			ENDCG
		}
	}
Fallback "Mobile/VertexLit"
}
