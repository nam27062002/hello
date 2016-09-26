Shader "Hungry Dragon/TransparentAdditive"
{

	Properties{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
		Blend SrcAlpha One
//		Blend DstAlpha One
//		Blend One OneMinusSrcColor

		//		AlphaTest Greater .01
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }
		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
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

				sampler2D _MainTex;
				fixed4 _TintColor;

				struct appdata_t {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
//				#ifdef SOFTPARTICLES_ON
//					float4 projPos : TEXCOORD1;
//				#endif
				};

				float4 _MainTex_ST;

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
					return o;
				}

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
					return 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				}
				ENDCG
			}
		}

		// ---- Dual texture cards
		SubShader{
			Pass{
				SetTexture[_MainTex]{
					constantColor[_TintColor]
					combine constant * primary
				}
				SetTexture[_MainTex]{
					combine texture * previous DOUBLE
				}
			}
		}

		// ---- Single texture cards (does not do color tint)
		SubShader{
			Pass{
				SetTexture[_MainTex]{
					combine texture * primary
				}
			}
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
//		Blend SrcAlpha One 
		Blend One One

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
	}
*/
	CustomEditor "GlowMaterialInspector"

}
