Shader "Hungry Dragon/TransparentAdditive Ztest Always"
{

	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 6.0

	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
		Blend SrcAlpha One

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
					combine texture * primary
				}
			}
		}

	}
}
