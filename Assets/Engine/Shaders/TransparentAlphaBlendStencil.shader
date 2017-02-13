Shader "Hungry Dragon/TransparentAlphaBlend(Stencil)"
{
	Properties
	{
		_MainTex("Particle Texture", 2D) = "white" {}
		_StencilMask("Stencil Mask", int) = 10
	}

	Category
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "GlowTransparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off Lighting Off ZWrite Off Fog{ Color(0,0,0,0) }

		BindChannels
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}

		SubShader
		{
			Pass
			{
				Stencil
				{
					Ref [_StencilMask]
					Comp always
					Pass Replace
					ZFail keep
				}

				AlphaTest greater 0.5

				SetTexture[_MainTex]
				{
//					constantColor (0.0, 0.0, 0.0, 1.0)
					combine texture * primary
//					combine texture * constant
				}
			}
		}
	}

	CustomEditor "GlowMaterialInspector"
}
