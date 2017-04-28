// Shared stuff for UI custom shaders

// INCLUDES AND PREPROCESSOR ///////////////////////////////////////////////////////////////////////////////
#ifndef UI_SHADERS_INCLUDED
#define UI_SHADERS_INCLUDED
#include "UnityCG.cginc"
#include "UnityUI.cginc"

// Soft Mask Support
#include "Assets/External/SoftMask/Shaders/SoftMask.cginc" 

// TYPES ///////////////////////////////////////////////////////////////////////////////////////////////////
struct appdata_t {
	fixed4 vertex   : POSITION;
	fixed4 color    : COLOR;
	fixed2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f {
	fixed4 vertex   : SV_POSITION;
	fixed4 color    : COLOR;
	fixed2 texcoord  : TEXCOORD0;
	float4 worldPosition : TEXCOORD1;
	UNITY_VERTEX_OUTPUT_STEREO

	// Soft Mask Support
        // The number in braces determines what TEXCOORDn Soft Mask may use
        // (it required only one TEXCOORD).
        SOFTMASK_COORDS(2)
};

// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////
sampler2D _MainTex;
uniform fixed4 _ColorMultiply;
uniform fixed4 _ColorAdd;
uniform fixed _Alpha;
uniform fixed _SaturationAmount;
uniform fixed _BrightnessAmount;
uniform fixed _ContrastAmount;

fixed4 _TextureSampleAdd;
float4 _ClipRect;

// AUX METHODS /////////////////////////////////////////////////////////////////////////////////////////////
// Aux method to apply brightness/saturation/contrast factors to a given color
fixed3 ContrastSaturationBrightness(fixed3 _color, fixed _b, fixed _s, fixed _c) {
	// Refs:
	// http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
	// https://accessibility.kde.org/hsl-adjusted.php
	// http://www.laurenscorijn.com/articles/colormath-basics
	// http://www.rapidtables.com/convert/color/rgb-to-hsl.htm
	// http://stackoverflow.com/questions/2353211/hsl-to-rgb-color-conversion

	// Brightness
	_color.r = _color.r + _b;
	_color.g = _color.g + _b;
	_color.b = _color.b + _b;

	// Saturation
	// [AOC] The proper way to do it would be to move from rgb to hsl color space, but this is too expensive for the fragment shader!
	// We will do this that is much more quick and it's a good approximation:
	// - Calculate the grayscale value of the color (luminosityFactor), and consider that to be the color at sat delta -1
	// - Consider input color as the color at sat delta 0
	// - Use these two values to interpolate maximum saturated color at an arbitrary value, and consider it sat delta 1
	fixed3 luminosityConstants = fixed3(0.2126, 0.7152, 0.0722);
	fixed luminosityFactor = dot(_color, luminosityConstants);	// (r * lr + g * lg + b * lb)
	fixed3 desaturatedColor = fixed3(luminosityFactor, luminosityFactor, luminosityFactor);
	fixed3 saturatedColor = lerp(desaturatedColor, _color, 2);	// Arbitrary interpolation factor
	fixed delta = (_s - (-1)) * (1 - 0) / (1 - (-1)) + 0;	// Convert from input [-1..1] to lerp [0..1] (http://stackoverflow.com/questions/1456000/rescaling-ranges)
	_color = lerp(desaturatedColor, saturatedColor, delta);

	// Contrast
	// From http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
	// Again a quick'n'dirty implementation to do the deed, similar to how saturation is computed
	fixed3 minContrastColor = fixed3(0.5, 0.5, 0.5);
	fixed3 maxContrastColor = lerp(minContrastColor, _color, 2);	// Arbitrary interpolation factor
	delta = (_c - (-1)) * (1 - 0) / (1 - (-1)) + 0;	// Convert from input [-1..1] to lerp [0..1] (http://stackoverflow.com/questions/1456000/rescaling-ranges)
	_color = lerp(minContrastColor, maxContrastColor, delta);

	// Done!
	return _color;
}

// Aux method to apply color changes to the output vertex color.
// To be called right before the vertex shader return.
void ApplyVertexColorModifiers(inout fixed4 _color) {
	// Apply color multiplier and extra alpha
	_color *= _ColorMultiply;
	_color.a *= _Alpha;
}

// Aux method to apply color changes to the output pixel color.
// To be called right before the fragment shader return.
void ApplyFragmentColorModifiers(inout fixed4 _color) {
	// Apply contrast/saturation/brightness
	_color.rgb = ContrastSaturationBrightness(_color.rgb, _BrightnessAmount, _SaturationAmount, _ContrastAmount);
	
	// Apply additive color - after contrast/saturation/brightness to be able to do sepia-like effects
	_color += _ColorAdd;
	
	// Alpha clipping (don't render below certain alpha levels
	clip(_color.a - 0.01);
}

#endif