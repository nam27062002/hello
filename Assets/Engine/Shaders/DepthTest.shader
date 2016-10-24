// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap

Shader "Hungry Dragon/DepthTest" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color("Tint (RGB)", color) = (1, 0, 0, 1)

	}

	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass {  
			Tags { "Lighthing" = "ForwardBase"}
//			Tags { "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
			// Blend One One
			//Blend OneMinusDstColor One
			//Cull Off
//			Lightning Off
			ZWrite Off
			Fog{ Color(0, 0, 0, 0) }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
							// make fog work
				#pragma multi_compile_fog

				//#pragma multi_compile_particles
				#include "UnityCG.cginc"
//				#include "AutoLight.cginc"
//				#include "HungryDragon.cginc"

				#define CAUSTIC_ANIM_SCALE  0.1f
				#define CAUSTIC_RADIUS  0.125f

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 uv : TEXCOORD0;
					float4 scrPos:TEXCOORD1;
					float4 color : COLOR;
				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;


				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.x * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.y * 35.0f) + _Time.y) + sin((v.vertex.y * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					v.vertex.z += (sinX + sinY) * 0.002 * step(0.0, v.vertex.z) * v.color.r;

//					v.vertex.z += ((sin((v.vertex.x * 60.0f) + _Time.z) * 1.0) + sin((v.vertex.y * 75.0f) + _Time.w) * 0.5) * 0.004 * step(0.0, v.vertex.z) * v.color.g;
//					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);
//					o.uv = TRANSFORM_TEX(v.vertex, _MainTex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//					o.uv = TRANSFORM_TEX(mul(v.uv.yx, unity_ObjectToWorld), _MainTex);
//					o.uv = TRANSFORM_TEX(mul(v.uv, unity_WorldToObject), _MainTex);
//					o.uv = TRANSFORM_TEX(o.scrPos, _MainTex);

					o.color = v.color;
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
//					return fixed4(1.0f, 0.0f, 0.0f, 0.5f);
//					float depth = 1.0 - Linear01Depth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 4.0f;
					float depth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x) * 1000.0f * _ProjectionParams.w;
					//float depth = 1.0 - pow(tex2Dproj(_CameraDepthTexture, (i.scrPos)).x * 1.0f, 10.0);
					return float4(depth, depth, depth, 1.0);
				}
			ENDCG
		}
	}
}
