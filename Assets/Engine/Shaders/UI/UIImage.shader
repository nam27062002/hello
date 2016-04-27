// Extension of Unity's default shader for UI images
// Adding saturation/contrast/brightness and additive color
// Inspired by http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
Shader "Custom/UI/UIImage" {
	// Exposed properties
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		
		_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
		_ColorAdd ("Color Add", Color) = (0,0,0,0)
		_Alpha ("Alpha", Float) = 1	// [AOC] Will be multiplied to the source and tint alpha components
		
		_SaturationAmount ("Saturation", Float) = 1
		_BrightnessAmount ("Brightness", Float) = 1
		_ContrastAmount ("Contrast", Float) = 1
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader {
		Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass {
		CGPROGRAM
			// INCLUDES AND PREPROCESSOR ///////////////////////////////////////////////////////////////////////////////
			#pragma vertex vert
			#pragma fragment frag
			#include "UIShaders.cginc"
			
			// VERTEX SHADER ///////////////////////////////////////////////////////////////////////////////////////////
			v2f vert(appdata_t IN) {
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.color = IN.color;
				OUT.texcoord = IN.texcoord;
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
#endif
				// [AOC] Apply color modifications
				ApplyVertexColorModifiers(OUT.color);
				
				// Done!
				return OUT;
			}

			// FRAGMENT SHADER /////////////////////////////////////////////////////////////////////////////////////////
			fixed4 frag(v2f IN) : SV_Target {
				// Default color + tint
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				
				// Extra modifiers
				ApplyFragmentColorModifiers(c);
				
				// Done!
				return c;
			}
		ENDCG
		}
	}
}
