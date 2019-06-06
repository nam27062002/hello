// Extension of Unity's default shader for UI images
// Adding saturation/contrast/brightness and additive color
// Inspired by http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
Shader "Custom/UI/UIImage" {
	// Exposed properties
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		
		_ColorMultiply ("Color Multiply", Color) = (1,1,1,1)
		_ColorAdd ("Color Add", Color) = (0,0,0,0)
		
		[Toggle(COLOR_RAMP_ENABLED)] _ColorRampEnabled("Color Ramp Enabled", Float) = 0
		_ColorRampTex("Color Ramp", 2D) = "red" {}
		//_ColorRampIntensity("Color Ramp Intensity", Range(0, 1)) = 0	// [AOC] Make it more optimal by just getting the full value from the gradient
		
		_Alpha ("Alpha", Float) = 1	// [AOC] Will be multiplied to the source and tint alpha components
		
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

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		// Soft Mask support
        // Soft Mask determines that shader supports soft masking by presence of this
        // property. All other properties listed in SoftMask.shader aren't required
        // to include here. 
        _SoftMask("Mask", 2D) = "white" {}
	}

	SubShader {
		Tags {
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		/*Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}*/

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass {
		Name "Default"
		CGPROGRAM
			// INCLUDES AND PREPROCESSOR ///////////////////////////////////////////////////////////////////////////////
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			
			// Flags
			#pragma shader_feature __ COLOR_RAMP_ENABLED
			#pragma multi_compile __ UNITY_UI_ALPHACLIP

			// Soft Mask Support
            #pragma multi_compile __ SOFTMASK_SIMPLE SOFTMASK_SLICED SOFTMASK_TILED
            
            #include "UIShaders.cginc"
			
			// VERTEX SHADER ///////////////////////////////////////////////////////////////////////////////////////////
			v2f vert(appdata_t IN) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.color = IN.color;
				OUT.texcoord = IN.texcoord;

				// [AOC] Apply color modifications
				ApplyVertexColorModifiers(OUT.color);

				// Soft Mask Support
				SOFTMASK_CALCULATE_COORDS(OUT, IN.vertex)
				
				// Done!
				return OUT;
			}

			// FRAGMENT SHADER /////////////////////////////////////////////////////////////////////////////////////////
			fixed4 frag(v2f IN) : SV_Target {
				// Default color + tint
				fixed4 c = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				
				// Extra modifiers
				ApplyFragmentColorModifiers(c);

				// Soft Mask Support
				c.a *= SOFTMASK_GET_MASK(IN);

				// Apply clipping
				c.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#ifdef UNITY_UI_ALPHACLIP
				clip(c.a - 0.001);
				#endif
				
				// Done!
				return c;
			}
		ENDCG
		}
	}
}
