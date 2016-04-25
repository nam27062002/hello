// Custom Dragon Shader.
// - Detail Texture. R: Inner Light value. G: Spec value.

Shader "Hungry Dragon/Dragon" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_DetailTex ("Detail (RGB)", 2D) = "white" {} // r -> inner light, g -> specular, b->noise
	_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
	_ColorAdd ("Color Add", Color) = (0,0,0,0)

	_InnerLightAdd ("Inner Light Add", float) = 0
	_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

	_SpecExponent ("Specular Exponent", float) = 1

	_NoiseColor ("Noise Color", Color) = (1,1,1,1)
	_NoiseValue ("Noise Value",  Range (0, 1)) = 0

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
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float3 halfDir : VECTOR;
				UNITY_FOG_COORDS(1)
				float3 vertexLighting : TEXCOORD2;

				float3 tangentWorld : TEXCOORD3;  
		        float3 normalWorld : TEXCOORD4;
		        float3 binormalWorld : TEXCOORD5;

		        half2 movetexcoord : TEXCOORD6;

		        fixed3 posWorld : TEXCOORD7;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;
			uniform float4 _DetailTex_TexelSize;

			float4 _ColorMultiply;
			float4 _ColorAdd;

			uniform float _InnerLightAdd;
			uniform float4 _InnerLightColor;

			uniform float _SpecExponent;

			uniform float4 _NoiseColor;
			uniform float _NoiseValue;

			uniform float _BumpStrength;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.movetexcoord = TRANSFORM_TEX(v.texcoord + float2( _Time.x * 2, _Time.x * 2 ), _MainTex);
				// Normal
				float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(_Object2World, v.vertex).xyz);
				// o.normal = normalize(mul(v.normal, _World2Object).xyz);
				o.normal = UnityObjectToWorldNormal(v.normal);

				// Half View - See: Blinn-Phong
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				o.halfDir = normalize(lightDirection + viewDirection);


				o.vertexLighting = float3(0,0,0);
				o.posWorld = mul( _Object2World, v.vertex ).xyz;

	            UNITY_TRANSFER_FOG(o,o.vertex);

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
				fixed4 detailMov = tex2D(_DetailTex, i.movetexcoord);

				// Calc normal from detail texture normal and tangent world
				float heightSampleCenter = tex2D (_DetailTex, i.texcoord).b;
	            float heightSampleRight = tex2D (_DetailTex, i.texcoord + float2(_DetailTex_TexelSize.x, 0)).b;
	            float heightSampleUp = tex2D (_DetailTex, i.texcoord + float2(0, _DetailTex_TexelSize.y)).b;
	     
	            float sampleDeltaRight = heightSampleRight - heightSampleCenter;
	            float sampleDeltaUp = heightSampleUp - heightSampleCenter;
	     
	            //TODO: Expose?
	            // float _BumpStrength = 10.0f;
	            float3 encodedNormal = cross(float3(1, 0, sampleDeltaRight * _BumpStrength ),float3(0, 1, sampleDeltaUp * _BumpStrength));
	            float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
     			float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));
     			// return fixed4(normalDirection, 1);

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

				fixed4 col = (diffuse + fixed4(pointLights + (UNITY_LIGHTMODEL_AMBIENT.rgb * 2),1)) * main * _ColorMultiply + _ColorAdd + specularLight + selfIlluminate;
				// fixed4 col = (diffuse + fixed4(pointLights + ShadeSH9(float4(normalDirection, 1.0)),1)) * main * _ColorMultiply + _ColorAdd + specularLight + selfIlluminate; // To use ShaderSH9 better done in vertex shader

				// Noise
				// col += _NoiseColor * _NoiseValue * detailMov.b;

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a); 

				return col; 

			}
		ENDCG
	}

//
//	 Pass {
//             Name "OUTLINE"
//             Tags { "LightMode" = "Always" "RenderType"="Transparent"}
//             Cull Front
//             ZWrite On
//             // ColorMask RGB
//             Blend SrcAlpha OneMinusSrcAlpha
// 
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag
//             #include "UnityCG.cginc"
//
//             struct appdata
//             {
//				float4 vertex : POSITION;
//				float3 normal : NORMAL;
//			};
//
//              struct v2f {
//		         float4 pos : POSITION;
//		     };
//
//		     uniform float4 _NoiseColor;
//
//	          v2f vert(appdata v) 
//	          {
//		         // just make a copy of incoming vertex data but scaled according to normal direction
//		         v2f o;
//		         o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
//		         float3 norm = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));
//		         float2 offset = TransformViewToProjection(norm.xy);
//		 
//		         o.pos.xy += offset * 0.025;
//		         return o;
//		     }
//
//             half4 frag(v2f i) :COLOR { return _NoiseColor; }
//             ENDCG
//         }
	
}
Fallback "Mobile/VertexLit"
}
