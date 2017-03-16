// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/Bumped Diffuse (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Specular( "Specular", float ) = 1
		_BumpStrength("Bump Strength", float) = 3
		_FresnelFactor("Fresnel factor", Range(0.0, 5.0)) = 0.27
		_FresnelColor("Fresnel color (RGB)", Color) = (0, 0, 0, 0)
		_StencilMask("Stencil Mask", int) = 10
	}
	SubShader
	{
		Pass
		{
			Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase"}
			Cull Back

			ZWrite on

			Stencil
			{
				Ref [_StencilMask]
				Comp always
				Pass Replace
				ZFail keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl_no_auto_normalization
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile LOW_DETAIL_ON MEDIUM_DETAIL_ON HI_DETAIL_ON

			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "HungryDragon.cginc"

			#if LOW_DETAIL_ON
			#endif

			#if MEDIUM_DETAIL_ON
//			#define RIM
//			#define BUMP
			#endif

			#if HI_DETAIL_ON
//			#define RIM
//			#define BUMP
//			#define SPEC
			#endif

//			#define BUMP

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
				float3 vLight : TEXCOORD2;

				float3 viewDir : VECTOR;
//				float3 halfDir : VECTOR;
		        float3 normalWorld : TEXCOORD4;
				#ifdef BUMP
				float3 tangentWorld : TANGENT;  
		        float3 binormalWorld : TEXCOORD5;
				#endif
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _Specular;
			uniform float _BumpStrength;
			uniform float _FresnelFactor;
			uniform float4 _FresnelColor;

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

				// To calculate tangent world
	            float4x4 modelMatrix = unity_ObjectToWorld;
     			float4x4 modelMatrixInverse = unity_WorldToObject;
				#ifdef BUMP
	            o.tangentWorld = normalize( mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
     			o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
     			o.binormalWorld = normalize( cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity
				#else
				o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				#endif

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				#ifdef BUMP
				// Calc normal from detail texture normal and tangent world
				float heightSampleCenter = col.a;

	            float heightSampleRight = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0) ).a;

	            float heightSampleUp = tex2D (_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).a;
	     
	            float sampleDeltaRight = heightSampleRight - heightSampleCenter;
	            float sampleDeltaUp = heightSampleUp - heightSampleCenter;

	            float3 encodedNormal = cross(float3(1, 0, sampleDeltaRight * _BumpStrength ),float3(0, 1, sampleDeltaUp * _BumpStrength));
	            float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
     			float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));
				#else
				float3 normalDirection = i.normalWorld;
				#endif

     			fixed4 diffuse = max(0,dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;

//     			fixed specular = pow(max(dot(normalDirection, i.halfDir), 0), _Specular);
//				fixed fresnel = pow(max(dot(normalDirection, i.viewDir), 0), _FresnelFactor);
				fixed fresnel = clamp(pow(max(1.0 - dot(i.viewDir, normalDirection), 0.0), _FresnelFactor), 0.0, 1.0);

     			// col = (diffuse + fixed4(UNITY_LIGHTMODEL_AMBIENT.rgb,1)) * col + specular * _LightColor0;
				col = (diffuse + fixed4(i.vLight, 1)) * col + /*(specular * _LightColor0) + */(fresnel * _FresnelColor);
//				col = (diffuse + fixed4(i.vLight, 1)) * col;
//				col = lerp(col, _FresnelColor, fresnel * _FresnelColor.a * 4.0);

				UNITY_OPAQUE_ALPHA(col.a);	// Opaque
				return col;
			}
			ENDCG
		}
	}
}
