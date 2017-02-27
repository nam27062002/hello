// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hungry Dragon/Bumped Diffuse BRDF (Spawners)"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Specular( "Specular", float ) = 1
		_BumpStrength("Bump Strength", float) = 3
		_BRDF ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
			Cull Back
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
				float4 vertex : SV_POSITION;
				// float3 normal : NORMAL; 

				float3 vLight : TEXCOORD2;

				float3 viewDir : VECTOR;
				float3 tangentWorld : TEXCOORD3;  
		        float3 normalWorld : TEXCOORD4;
		        float3 binormalWorld : TEXCOORD5;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BRDF;

			uniform float4 _MainTex_TexelSize;
			uniform float _Specular;
			uniform float _BumpStrength;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				fixed3 worldPos = mul(unity_ObjectToWorld, v.vertex);
				float3 normal = UnityObjectToWorldNormal(v.normal);
				o.vLight = ShadeSH9(float4(normal, 1.0));

				// View Dir
				o.viewDir = normalize(_WorldSpaceCameraPos - worldPos.xyz);

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

				// Calc normal from detail texture normal and tangent world
				float heightSampleCenter = col.a;

	            float heightSampleRight = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0) ).a;

	            float heightSampleUp = tex2D (_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).a;
	     
	            float sampleDeltaRight = heightSampleRight - heightSampleCenter;
	            float sampleDeltaUp = heightSampleUp - heightSampleCenter;

	            float3 encodedNormal = cross(float3(1, 0, sampleDeltaRight * _BumpStrength ),float3(0, 1, sampleDeltaUp * _BumpStrength));
	            float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
     			float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));
     			 
     			half NdotL = dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz));
     			NdotL = NdotL * 0.5 + 0.5;
     			half NdotV = dot(normalDirection, i.viewDir);

     			fixed4 brdf = tex2D (_BRDF, float2(NdotL, NdotV)); 

     			fixed4 specular = fixed4(0,0,0,0);
     			if (_Specular > 0) 
     				specular = pow(brdf.r, _Specular) * brdf;

     			col = (brdf + fixed4(i.vLight,1)) * col + specular;

				UNITY_OPAQUE_ALPHA(col.a);	// Opaque

				return col;
			}
			ENDCG
		}
	}
}
