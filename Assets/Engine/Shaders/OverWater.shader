// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap


Shader "Hungry Dragon/OverWater" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_WaterSpeed("Speed ", Float) = 0.5
	}

	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent"  "LightMode" = "ForwardBase" }
		LOD 100

		Pass {  
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite On
			Fog{ Color(0, 0, 0, 0) }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase
//				#pragma multi_compile_particles

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
//				#include "Lighting.cginc"
				#include "HungryDragon.cginc"


				#define CAUSTIC_ANIM_SCALE  4.0f
				#define CAUSTIC_RADIUS  0.1125f

				struct appdata_t {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					float3 viewDir: TEXCOORD2;
					half2 uv : TEXCOORD0;
//					float4 scrPos:TEXCOORD1;
					float4 color : COLOR;
//					LIGHTING_COORDS(2, 3)

				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DetailTex;
				float4 _DetailTex_ST;
				float _WaterSpeed;


				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * 0.15 * moveVertex * v.color.w;

					o.vertex = UnityObjectToClipPos(v.vertex);
//					o.scrPos = ComputeScreenPos(o.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.viewDir = o.vertex - _WorldSpaceCameraPos;

					o.color = v.color;
//					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows

					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					float2 anim = float2(0.0, _Time.y * _WaterSpeed);

					fixed4 col = tex2D(_MainTex, (i.uv.xy + anim));
					col += tex2D(_DetailTex, 1.0f * (i.uv.xy + anim * 0.75)) * 0.5f;

					fixed3 one = fixed3(1, 1, 1);
					col.xyz = one - 2.0 * (one - i.color.xyz * 0.75) * (one - col.xyz);	// Overlay

//					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
//					col *= attenuation;


					return col;
				}
			ENDCG
		}
	}
}
