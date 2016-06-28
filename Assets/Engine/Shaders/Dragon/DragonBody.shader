// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon/Body" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normal Map (RGB)", 2D) = "white" {}
	_DetailTex ("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular
	_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
	_ColorAdd ("Color Add", Color) = (0,0,0,0)

	_InnerLightAdd ("Inner Light Add", float) = 0
	_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

	_SpecExponent ("Specular Exponent", float) = 1

	_BumpStrength("Bump Strength", float) = 3
}

SubShader {
	Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
	ZWrite On
	LOD 100
	
	Pass {
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc" 
			#include "Lighting.cginc"
			#include "../HungryDragon.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				// float3 normal : NORMAL;
				float3 halfDir : VECTOR;

				float3 tangentWorld : TEXCOORD2;  
		        float3 normalWorld : TEXCOORD3;
		        float3 binormalWorld : TEXCOORD4;

		        fixed3 posWorld : TEXCOORD5;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;

			float4 _ColorMultiply;
			float4 _ColorAdd;

			uniform float _InnerLightAdd;
			uniform float4 _InnerLightColor;

			uniform float _SpecExponent;
			uniform float _BumpStrength;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				// Normal
				// o.normal = UnityObjectToWorldNormal(v.normal);

				// Half View - See: Blinn-Phong
				float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(_Object2World, v.vertex).xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				o.halfDir = normalize(lightDirection + viewDirection);

				o.posWorld = mul( _Object2World, v.vertex ).xyz;

	            // To calculate tangent world
	            float4x4 modelMatrix = _Object2World;
     			float4x4 modelMatrixInverse = _World2Object; 
	            o.tangentWorld = normalize( mul(modelMatrix, float4(v.tangent.xyz, 0.0)).xyz);
     			o.normalWorld = normalize(mul(float4(v.normal, 0.0), modelMatrixInverse).xyz);
     			o.binormalWorld = normalize( cross(o.normalWorld, o.tangentWorld) * v.tangent.w); // tangent.w is specific to Unity

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 main = tex2D(_MainTex, i.texcoord);
				fixed4 detail = tex2D(_DetailTex, i.texcoord);

	            float3 encodedNormal = UnpackNormal (tex2D (_BumpMap, i.texcoord));
	            float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
     			float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));
     			// normalDirection = i.normal;
     			fixed4 diffuse = max(0,dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _LightColor0;

     			fixed3 pointLights = fixed3(0,0,0);
     			for (int index = 0; index <1; index++)
	            {    
		               float4 lightPosition = float4(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index], 1.0);
		               float3 vertexToLightSource = lightPosition.xyz - i.posWorld.xyz;
		               float3 lightDirection = normalize(vertexToLightSource);
		               float squaredDistance = dot(vertexToLightSource, vertexToLightSource);
		               float attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * squaredDistance);
		               float3 diffuseReflection = attenuation * unity_LightColor[index].rgb * max(0.0, dot(normalDirection, lightDirection));         
		               pointLights = pointLights + diffuseReflection;
	            }

	            // Inner lights
     			fixed4 selfIlluminate = ( main * (detail.r * _InnerLightAdd * _InnerLightColor));

				// Specular
				float specularLight = pow(max(dot( normalDirection, i.halfDir), 0), _SpecExponent) * detail.g;

				// fixed4 col = (diffuse + fixed4(pointLights + (UNITY_LIGHTMODEL_AMBIENT.rgb),1)) * main * _ColorMultiply + _ColorAdd + specularLight + selfIlluminate;
				fixed4 col = (diffuse + fixed4(pointLights + ShadeSH9(float4(normalDirection, 1.0)),1)) * main * _ColorMultiply + _ColorAdd + specularLight + selfIlluminate; // To use ShaderSH9 better done in vertex shader
				UNITY_OPAQUE_ALPHA(col.a); 

				return col; 

			}
		ENDCG
	}

	
}
Fallback "Mobile/VertexLit"
}
