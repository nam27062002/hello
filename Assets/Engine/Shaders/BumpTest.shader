Shader "Hungry Dragon/BumpTest"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DetailTex ("Detail (RGB)", 2D) = "white" {}	// r -> inner light, g -> specular, b->bumpmap
		_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
		_ColorAdd ("Color Add", Color) = (0,0,0,0)

		_InnerLightAdd ("Inner Light Add", float) = 0
		_InnerLightColor ("Inner Light Color", Color) = (1,1,1,1)

		_SpecExponent ("Specular Exponent", float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		ZWrite On
		LOD 100

		
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#include "UnityCG.cginc"

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

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

					// Normal
					float3 viewDirection = normalize(_WorldSpaceCameraPos - mul(_Object2World, v.vertex).xyz);
					// o.normal = normalize(mul(v.normal, _World2Object).xyz);
					o.normal = UnityObjectToWorldNormal(v.normal);

					// Half View - See: Blinn-Phong
					float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
					o.halfDir = normalize(lightDirection + viewDirection);

					o.vertexLighting = float3(0,0,0);
					float4 posWorld = mul( _Object2World, v.vertex );
		            for (int index = 0; index <1; index++)
		            {    
			               float4 lightPosition = float4(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index], 1.0);
			               float3 vertexToLightSource = lightPosition.xyz - posWorld.xyz;
			               float3 lightDirection = normalize(vertexToLightSource);
			               float squaredDistance = dot(vertexToLightSource, vertexToLightSource);
			               float attenuation = 1.0 / (1.0 + unity_4LightAtten0[index] * squaredDistance);
			               float3 diffuseReflection = attenuation * unity_LightColor[index].rgb * max(0.0, dot(o.normal, lightDirection));         
			               o.vertexLighting = o.vertexLighting + diffuseReflection;
		            }

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
					// fixed4 diffuse = ( main * (1 + detail.r * _InnerLightAdd * _InnerLightColor)) * _ColorMultiply + _ColorAdd;

					// Calc normal from detail texture normal and tangent world
					float heightSampleCenter = tex2D (_DetailTex, i.texcoord).g;
		            float heightSampleRight = tex2D (_DetailTex, i.texcoord + float2(_DetailTex_TexelSize.x, 0)).g;
		            float heightSampleUp = tex2D (_DetailTex, i.texcoord + float2(0, _DetailTex_TexelSize.y)).g;
		     
		            float sampleDeltaRight = heightSampleRight - heightSampleCenter;
		            float sampleDeltaUp = heightSampleUp - heightSampleCenter;
		     
		            //TODO: Expose?
		            float _BumpStrength = 3.0f;
		            float3 encodedNormal = cross(float3(1, 0, sampleDeltaRight * _BumpStrength),float3(0, 1, sampleDeltaUp * _BumpStrength));
		            float3x3 local2WorldTranspose = float3x3(i.tangentWorld, i.binormalWorld, i.normalWorld);
         			float3 normalDirection = normalize(mul(encodedNormal, local2WorldTranspose));

         			fixed4 diffuse = main * max(0,dot( normalDirection, normalize(_WorldSpaceLightPos0.xyz))) * _ColorMultiply + _ColorAdd;
         			fixed4 selfIlluminate = ( main * (detail.r * _InnerLightAdd * _InnerLightColor));

					// Specular
					float specularLight = pow(max(dot( normalDirection, i.halfDir), 0), _SpecExponent) * detail.g;


					float4 res = diffuse + specularLight + fixed4(i.vertexLighting, 0) * main;
					UNITY_APPLY_FOG(i.fogCoord, res);
					UNITY_OPAQUE_ALPHA(res.a); 

					return res; 
				}
			ENDCG
		}
	
	}
}
