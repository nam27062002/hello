Shader "Hungry Dragon/TransparentAdditive Fire Rush"
{

	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0
		_StencilMask("Stencil Mask", int) = 10

	}

	Category{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha One

		//		AlphaTest Greater .01
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest [_ZTest]
		AlphaTest Greater 0.01

		BindChannels{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		// ---- Fragment program cards
		SubShader{
			Pass{

				Stencil
				{
					Ref[_StencilMask]
					Comp always
					Pass Replace
					ZFail keep
				}

				SetTexture[_MainTex] {
					combine texture * primary
				}
			}
		}

	}
}
