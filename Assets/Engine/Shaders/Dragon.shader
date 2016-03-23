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

			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				half2 movetexcoord : TEXCOORD3;
				float3 normal : NORMAL;
				float3 halfDir : VECTOR;
				UNITY_FOG_COORDS(1)
				float3 vertexLighting : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;

			float4 _ColorMultiply;
			float4 _ColorAdd;

			uniform float _InnerLightAdd;
			uniform float4 _InnerLightColor;

			uniform float _SpecExponent;

			uniform float4 _NoiseColor;
			uniform float _NoiseValue;

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
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 main = tex2D(_MainTex, i.texcoord);
				fixed4 detail = tex2D(_DetailTex, i.texcoord);
				fixed4 detailMov = tex2D(_DetailTex, i.movetexcoord);

				fixed4 col = ( main * (1 + detail.r * _InnerLightAdd * _InnerLightColor)) * _ColorMultiply + _ColorAdd;

				// Specular
				float specularLight = pow(max(dot( i.normal, i.halfDir), 0), _SpecExponent) * detail.g;
				col = col + specularLight;

				// Noise
				// col += _NoiseColor * ((sin( _Time.y) + 1) * 0.5) * _NoiseValue * detail.b;
				col += _NoiseColor * _NoiseValue * detailMov.b;
				// col += _NoiseColor * _NoiseValue * ((sin(_Time.y + i.texcoord.y * 5) + 1) * 0.5);

				UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a); 

				return col + fixed4(i.vertexLighting, 0) * main; 
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
