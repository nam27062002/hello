Shader "Hungry Dragon/TransparentAdditiveDouble"
{

	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		_TintColor("Color", Color) = (1,1,1,1)
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0

	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha One

		//		AlphaTest Greater .01
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest[_ZTest]

		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		// ---- Fragment program cards
		SubShader{			

			Pass{
				SetTexture[_MainTex] {
					constantColor [_TintColor]
					combine constant * primary
				}

				SetTexture[_MainTex] {
					combine texture * previous QUAD
				}
			}
		}

	}
}
