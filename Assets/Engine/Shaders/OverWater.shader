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
		_BlendTex("Blend (RGB)", 2D) = "white" {}

		_WaterSpeed("Speed ", Float) = 0.5
		_WaveRadius("Wave radius ", Range(0.0, 1.0)) = 0.15
		_StartTime("Start time", Float) = 0.0
		_StartPosition("Start position", Vector) = (0,0,0,0)
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
				#pragma glsl_no_auto_normalization
				#pragma fragmentoption ARB_precision_hint_fastest

//				#pragma multi_compile_fog
//				#pragma multi_compile_fwdbase
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
					float2 uv2: TEXCOORD2;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv2: TEXCOORD2;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
					HG_FOG_COORDS(1)

				};


				sampler2D _CameraDepthTexture;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DetailTex;
				float4 _DetailTex_ST;
				sampler2D _BlendTex;
				float4 _BlendTex_ST;
				float _WaterSpeed;
				float _WaveRadius;
				HG_FOG_VARIABLES
				uniform float _StartTime;
				uniform fixed4 _StartPosition;


#define SPLASHRADIUS 10.0
#define SPLASHSIZE 1.5
#define SPLASHTIME 2.0

				v2f vert (appdata_t v) 
				{
					v2f o;
					float sinX = sin((v.vertex.x * 22.1f) + _Time.y) + sin((v.vertex.x * 42.2f) + _Time.y * 5.7f) + sin((v.vertex.z * 62.2f) + _Time.y * 6.52f);
					float sinY = sin((v.vertex.z * 35.0f) + _Time.y) + sin((v.vertex.z * 65.3f) + _Time.y * 5.7f) + sin((v.vertex.x * 21.2f) + _Time.y * 6.52f);
					float moveVertex = 1.0;// step(0.0, v.vertex.y);

					float dst = clamp(1.0 - (length(v.vertex - _StartPosition) / SPLASHRADIUS), 0.0, 1.0);
					float dt = _Time.y - _StartTime;
					float ct = clamp(1.0 - (dt / SPLASHTIME), 0.0, 1.0);

//					v.vertex.y += ((sinX + sinY) * _WaveRadius * moveVertex * v.color.w) + (cos((dst * 10.0) + dt * 10.0) * dst * sin(ct * 2.0) * SPLASHSIZE);
					v.vertex.y += ((sinX + sinY) * _WaveRadius * moveVertex * (1.0 - v.color.w));

					o.vertex = UnityObjectToClipPos(v.vertex);
//					o.scrPos = ComputeScreenPos(o.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.uv2 = TRANSFORM_TEX(v.uv2, _BlendTex);

//					o.viewDir = o.vertex - _WorldSpaceCameraPos;

//					o.color = lerp(v.color, float4(1.0, 0.0, 1.0, 1.0), dst * ct);
					o.color = v.color;
//					o.color = float4(1.0, 0.0, 1.0, 1.0);
					//					TRANSFER_VERTEX_TO_FRAGMENT(o);	// Shadows
//					HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
					_FogStart = -12.0;
					o.fogCoord = float2(saturate((worldPos.z - _FogStart) / (_FogEnd - _FogStart)), 0.5);

					return o;
				}


				fixed4 frag (v2f i) : SV_Target
				{
					float2 anim = float2(0.0, _Time.y * _WaterSpeed);

					fixed4 col = tex2D(_MainTex, (i.uv.xy + anim));
					col += tex2D(_DetailTex, 1.0f * (i.uv.xy + anim * 0.75)) * 0.5f;

					fixed4 blend = tex2D(_BlendTex, 1.0f * (i.uv2.xy + anim * 1.5));
					col = lerp(col, blend, i.color.w);


					fixed3 one = fixed3(1, 1, 1);
					col.xyz = one - 2.0 * (one - i.color.xyz * 0.75) * (one - col.xyz);	// Overlay


					HG_APPLY_FOG(i, col);
//					fixed4 fogCol = tex2D(_FogTexture, i.fogCoord);
//					col.rgb = lerp((col).rgb, fogCol.rgb, fogCol.a);
//					col = clamp(col + fixed4(0.25, 0.25, 0.25, 0.25), fixed4(0.0, 0.0, 0.0, 1.0), fixed4(1.0, 1.0, 1.0, 1.0));
					UNITY_OPAQUE_ALPHA(col.a);

//					float attenuation = LIGHT_ATTENUATION(i);	// Shadow
//					col *= attenuation;


					return col;
				}
			ENDCG
		}
	}
}
