Shader "Hungry Dragon/TransparentAdditiveDouble(Glow)"
{

	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)

//		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
		Blend SrcAlpha One

		//		AlphaTest Greater .01
		Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }

		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		// ---- Fragment program cards
		SubShader{			

			Pass{
				SetTexture[_MainTex] {
					constantColor[_Color]
					combine constant * texture, constant * texture double
				}
			}
		}

	}

	CustomEditor "GlowMaterialInspector"

}
