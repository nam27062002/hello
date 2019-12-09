// Unlit shader, with shadows
// - no lighting
// - can receive shadows
// - has lightmap


Shader "Hungry Dragon/Waterfall" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_DetailTex("Detail (RGB)", 2D) = "white" {}
		_BlendTex("Blend (RGB)", 2D) = "white" {}
		_WaterSpeed("Speed: ", Float) = 0.5
		_BackColor("Back Color: ", Color) = (0.0, 0.0, 0.0, 0.0)
		_ZLimit("Fog Z limit", Range(0.0, 0.3)) = 0.0
		_StencilMask("Stencil Mask: ", int) = 10
	}

	SubShader {
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
//		LOD 100
		Lighting Off
		Cull back
		ZWrite Off
		ZTest LEqual


		Pass {  
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil
			{
				Ref[_StencilMask]
				Comp NotEqual
//				Pass DecrWrap//keep
			}

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
                #pragma multi_compile __ NIGHT

				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "HungryDragon.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
					float4 color : COLOR;
				}; 

				struct v2f {
					float4 vertex : SV_POSITION;
//					float3 viewDir: TEXCOORD2;
					float2 uv0 : TEXCOORD0;
					float2 uv1 : TEXCOORD1;
					float2 uv2 : TEXCOORD2;
					float4 color : COLOR;
					HG_FOG_COORDS(3)
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _DetailTex;
				float4 _DetailTex_ST;
				sampler2D _BlendTex;
				float4 _BlendTex_ST;
				float _WaterSpeed;
				fixed4 _BackColor;
				HG_FOG_VARIABLES
				float _ZLimit;


				v2f vert (appdata_t v) 
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);

					float time = frac(_Time.x);
					float2 anim = float2(0.0, time * _WaterSpeed * 20.0);

					o.uv1 = o.uv0 = TRANSFORM_TEX(v.uv, _MainTex);
					o.uv0 += anim;
					o.uv1 += anim * 0.75;
					o.uv2 = TRANSFORM_TEX(v.uv2, _BlendTex) + anim * 1.5;

					o.color = v.color;

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					HG_TRANSFER_FOG(o, worldPos);	// Fog
//					float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
//					_FogStart = -12.0;
//					o.fogCoord = float2(saturate((worldPos.z - _FogStart) / (_FogEnd - _FogStart)), 0.5);

					return o;
				}


				fixed4 frag (v2f i) : SV_Target
				{

					fixed4 col = tex2D(_MainTex, i.uv0);
					col += tex2D(_DetailTex, i.uv1) * 0.5f;
					fixed4 blend = tex2D(_BlendTex, i.uv2);
					col = lerp(col, blend, i.color.w);

					fixed3 one = fixed3(1, 1, 1);
					col.xyz = one - 2.0 * (one - i.color.xyz * 0.75) * (one - col.xyz);	// Overlay

					fixed4 fogCol = tex2D(_FogTexture, i.fogCoord);
					float intensity = smoothstep(0.0, _ZLimit, i.fogCoord.x);
					col.rgb = lerp((col).rgb, fogCol.rgb, fogCol.a * intensity);

					col.a *= _BackColor.a;

					return col  * fixed4(0.25, 0.25, 0.5, 1.0);
				}
			ENDCG
		}


		Pass{
			Blend SrcAlpha OneMinusSrcAlpha

			Stencil
			{
				Ref[_StencilMask]
				Comp Equal
//				Pass DecrWrap//keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
            #pragma multi_compile __ NIGHT

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "HungryDragon.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				//					float3 viewDir: TEXCOORD2;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float4 color : COLOR;
				HG_FOG_COORDS(3)
			};

			fixed4 _BackColor;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _DetailTex;
			float4 _DetailTex_ST;
			sampler2D _BlendTex;
			float4 _BlendTex_ST;
			float _WaterSpeed;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				float time = frac(_Time.x);
				float2 anim = float2(0.0, time * _WaterSpeed * 20.0);

				o.uv1 = o.uv0 = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv0 += anim;
				o.uv1 += anim * 0.75;
				o.uv2 = TRANSFORM_TEX(v.uv2, _BlendTex) + anim * 1.5;

				o.color = v.color;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv0);
				col += tex2D(_DetailTex, i.uv1) * 0.5f;
				fixed4 blend = tex2D(_BlendTex, i.uv2);
				col = lerp(col, blend, i.color.w);

				fixed saturate = (col.r + 0.7152 * col.g + 0.0722 * col.b) * col.a * 0.5;

				fixed4 fcol = _BackColor;
				fcol.a *= (1.0 - i.color.a) + saturate;
				return fcol  * fixed4(0.25, 0.25, 0.5, 1.0);
			}

			ENDCG
		}
	}
}
