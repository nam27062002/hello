Shader "Hungry Dragon/Unlit Diffuse Transparent Two Sided(Glow)"
{

	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
//		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Tint("Translucent Color", Color) = (0.0, 0.0, 0.0, 0.5)
	}

	Category{

		//		AlphaTest Greater .01
		Lighting Off Fog{ Color(0,0,0,0) }

		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		// ---- Fragment program cards
		SubShader{
			Blend SrcAlpha OneMinusSrcAlpha
			Pass{
				Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Glow" }
				Cull front
				SetTexture[_MainTex] {
					constantColor[_Tint]
					combine texture * constant, texture * primary
				}
			}
			Pass{
				Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "TransparentGlow" }
				Cull back
				Zwrite off
//				Blend SrcAlpha OneMinusSrcAlpha

				SetTexture[_MainTex]{
					constantColor[_Tint]
					combine texture * previous, constant * previous
				}
			}
		}

	}

	CustomEditor "GlowMaterialInspector"

}
