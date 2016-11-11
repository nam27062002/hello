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
		_Color("Tint (RGB)", color) = (1, 0, 0, 1)
		_WaterSpeed("Speed ", Float) = 0.5
	}

	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		Pass {  
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite On
			Fog{ Color(0, 0, 0, 0) }

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
							// make fog work
				#pragma multi_compile_fog

				#pragma multi_compile_particles
				#include "UnityCG.cginc"

				#define CAUSTIC_ANIM_SCALE  4.0f
				#define CAUSTIC_RADIUS  0.1125f

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					float3 viewDir: TEXCOORD2;
					half2 uv : TEXCOORD0;
					float4 scrPos:TEXCOORD1;
					float4 color : COLOR;
				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DetailTex;
				float4 _DetailTex_ST;
				float4 _Color;
				float _WaterSpeed;


				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);
					v.vertex.y += (sinX + sinY) * 0.15 * moveVertex * v.color.w;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.scrPos = ComputeScreenPos(o.vertex);

					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					o.viewDir = o.vertex - _WorldSpaceCameraPos;


					o.color = v.color;
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{

//					float2 anim = float2(sin(i.uv.x * CAUSTIC_ANIM_SCALE + _Time.y * 4.02f) * CAUSTIC_RADIUS,
//										 sin(i.uv.y * CAUSTIC_ANIM_SCALE + _Time.y * 3.04f) * CAUSTIC_RADIUS + _Time.y * 0.5);
					float2 anim = float2(0.0, _Time.y * _WaterSpeed);

					fixed4 col = tex2D(_MainTex, 1.0f * (i.uv.xy + anim)) * 1.0f;
					col += tex2D(_DetailTex, 1.0f * (i.uv.xy + anim * 0.75)) * 0.5f;

					fixed3 one = fixed3(1, 1, 1);
					col.xyz = one - 2.0 * (one - i.color.xyz * 0.75) * (one - col.xyz);	// Overlay

					return col;
				}
			ENDCG
		}
	}
}
