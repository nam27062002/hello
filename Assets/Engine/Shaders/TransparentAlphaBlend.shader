Shader "Hungry Dragon/TransparentAlphaBlend"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Particle Texture", 2D) = "white" {}
		[Enum(LEqual, 2, Always, 6)] _ZTest("Ztest:", Float) = 2.0

	}

	Category
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Color(0,0,0,0) }
		ZTest[_ZTest]


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
				SetTexture[_MainTex] {
					constantColor[_TintColor]
					combine constant * primary
				}
				SetTexture[_MainTex]{
					combine texture * previous DOUBLE
				}
/*
				SetTexture[_MainTex]
				{
					combine texture * primary
				}
*/
			}
		}
	}
}
