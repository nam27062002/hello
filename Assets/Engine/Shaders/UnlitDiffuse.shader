Shader "Hungry Dragon/UnlitDiffuse(Glow)"
{

	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
//		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}

	Category{
		Tags{ "Queue" = "Geometry" "IgnoreProjector" = "True" "RenderType" = "Glow" }
//		Blend SrcAlpha One

		//		AlphaTest Greater .01
		Cull back Lighting Off Fog{ Color(0,0,0,0) }

		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		// ---- Fragment program cards
		SubShader{
			Pass{
				SetTexture[_MainTex] {
					combine texture * primary
				}
			}
		}

	}

	CustomEditor "GlowMaterialInspector"

}
