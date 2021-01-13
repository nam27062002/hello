// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Extension of Unity's default shader for UI images
// Adding saturation/contrast/brightness and additive color
// Inspired by http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
Shader "Custom/UI/UIFont" {
	// Exposed properties
	Properties {
		_MainTex ("Font Texture", 2D) = "white" {}
		_Color ("Text Color", Color) = (1,1,1,1)
		
		_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
		_ColorAdd ("Color Add", Color) = (0,0,0,0)
		
		// [AOC] Doesn't make much sense in fonts, but adding it for compatibility with UIColorFX
		[Toggle(COLOR_RAMP_ENABLED)] _ColorRampEnabled("Color Ramp Enabled", Float) = 0
		_ColorRampTex("Color Ramp", 2D) = "white" {}
		// _ColorRampIntensity("Color Ramp Intensity", Range(0, 1)) = 0	// [AOC] Make it more optimal by just getting the full value from the gradient
		
		_Alpha ("Alpha", Float) = 1	// [AOC] NEW!! Will be multiplied to the source and tint alpha components
		
		_SaturationAmount ("Saturation", Float) = 1
		_BrightnessAmount ("Brightness", Float) = 1
		_ContrastAmount ("Contrast", Float) = 1

		[Toggle] _LateMultiply("Late Multiply", Float) = 0
		
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
		}
		
		Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Lighting Off 
		Cull Off 
		ZTest [unity_GUIZTestMode]
		ZWrite Off 
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass {
		CGPROGRAM
			// INCLUDES AND PREPROCESSOR ///////////////////////////////////////////////////////////////////////////////
			#pragma vertex vert
			#pragma fragment frag
			
			// Flags
			#pragma shader_feature __ COLOR_RAMP_ENABLED
			
			// Includes
			#include "UIShaders.cginc"
			
			// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////
			uniform fixed4 _MainTex_ST;
			uniform fixed4 _Color;
			
			// VERTEX SHADER ///////////////////////////////////////////////////////////////////////////////////////////
			v2f vert(appdata_t IN) {
				// Default treatment
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.color = IN.color * _Color;
				OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
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
				// Default treatment
				fixed4 col = IN.color;
				col.a *= tex2D(_MainTex, IN.texcoord).a;
				
				// [AOC] Apply color modifications
				ApplyFragmentColorModifiers(col);
				clip (col.a - 0.01);
				
				// Done!
				return col;
			}
		ENDCG
		}
	}
}
