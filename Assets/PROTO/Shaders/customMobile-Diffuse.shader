// Custom implementation of Unity's default Mobile-Diffuse shader.
// Differences:
// - Added color tint
// @see http://docs.unity3d.com/Manual/ShaderTut2.html

Shader "Custom/Mobile-Diffuse" {
	// PROPERTIES
	Properties {
		_MainTex("Base (RGB)", 2D) = "white" {}
		_ColorMultiply("Color Multiply", Color) = (1, 1, 1, 1)
		_ColorAdd("Color Add", Color) = (0, 0, 0, 0)
	}
	
	// SHADER IMPLEMENTATION
	SubShader {
		// EXTRA
		Tags { 
			"RenderType"="Opaque" 
		}
		LOD 150

		// CG SURFACE SHADER IMPLEMENTATION
		CGPROGRAM
		
		// Setup - parameters copied from Unity's default Mobile/Diffuse shader
		#pragma surface surf Lambert noforwardadd
		
		// Map exposed properties to local cg vars
		sampler2D _MainTex;
		fixed4 _ColorAdd;
		fixed4 _ColorMultiply;

		// Input data format
		struct Input {
			float2 uv_MainTex;
		};
		
		// Main shader function
		void surf(Input IN, inout SurfaceOutput o) {
			// Get default color from the texture
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			
			// [AOC] Customization: apply multiply and add color to both albedo (diffuse color) and alpha properties
			o.Albedo = c.rgb * _ColorMultiply.rgb + _ColorAdd.rgb;
			o.Alpha = c.a * _ColorMultiply.a + _ColorAdd.a;
		}
		ENDCG
	}

	Fallback "Mobile/Diffuse"
}
