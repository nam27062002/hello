Shader "Hungry Dragon/TransparentSoftAdditive"
{
	Properties{
		_Fade("Blend", Range(0, 1)) = 0
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Texture2("Particle Texture2", 2D) = "white" {}
		_InvFade("Soft Particles Factor", Range(0.00,3.0)) = 1.0
	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend One OneMinusSrcColor

		ColorMask RGB
		Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }
		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord0
			Bind "TexCoord1", texcoord1
		}

		// ---- Fragment program cards
		SubShader{
			Pass{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag          
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles    
				#include "UnityCG.cginc"

				fixed4 _TintColor;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					float4 texcoord1 : TEXCOORD1;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
//				#ifdef SOFTPARTICLES_ON
//					float4 projPos : TEXCOORD2;
//				#endif
				};

				uniform float4 _MainTex_ST, _Texture2_ST;

				v2f vert(appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
//				#ifdef SOFTPARTICLES_ON
//					o.projPos = ComputeScreenPos(o.vertex);
//					COMPUTE_EYEDEPTH(o.projPos.z);
//				#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					o.texcoord1 = TRANSFORM_TEX(v.texcoord1,_Texture2);
					return o;
				}

				uniform float _Fade;
				uniform sampler2D _MainTex, _Texture2;
				sampler2D _CameraDepthTexture;
				float _InvFade;

				fixed4 frag(v2f i) : COLOR
				{
//				#ifdef SOFTPARTICLES_ON
//					float sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
//					float partZ = i.projPos.z;
//					float fade = saturate(_InvFade * (sceneZ - partZ));
//					i.color.a *= fade;
//				#endif
					half4   tA = tex2D(_MainTex, i.texcoord),
					tB = tex2D(_Texture2, i.texcoord1);
					fixed3 sum = lerp(tA.rgb * tA.a, tB.rgb * tB.a, _Fade);
					fixed alpha = lerp(tA.a, tB.a, _Fade);
					half4 prev = i.color * tex2D(_MainTex, i.texcoord);
					half4 prev1 = i.color * tex2D(_Texture2, i.texcoord1);
					prev.rgb *= prev.a;
					prev1.rgb *= prev1.a;
					return prev, prev1,fixed4(sum, alpha);
				}
				ENDCG
			}
		}


/*
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
//		Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" "PreviewType"="Plane" }
		Tags{ "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "GlowTransparent" "PreviewType" = "Plane" }

		LOD 100
		ZWrite Off
		Cull Off
		Blend OneMinusDstColor One

//		AlphaTest Less .01
//		ColorMask RGB

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
*/
	}

	CustomEditor "GlowMaterialInspector"

}
