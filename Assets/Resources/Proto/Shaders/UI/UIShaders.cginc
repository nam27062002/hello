// Shared stuff for UI custom shaders

// INCLUDES AND PREPROCESSOR ///////////////////////////////////////////////////////////////////////////////
#ifndef UI_SHADERS_INCLUDED
#define UI_SHADERS_INCLUDED
#include "UnityCG.cginc"

// TYPES ///////////////////////////////////////////////////////////////////////////////////////////////////
struct appdata_t {
	fixed4 vertex   : POSITION;
	fixed4 color    : COLOR;
	fixed2 texcoord : TEXCOORD0;
};

struct v2f {
	fixed4 vertex   : SV_POSITION;
	fixed4 color    : COLOR;
	fixed2 texcoord  : TEXCOORD0;
};

// PROPERTIES //////////////////////////////////////////////////////////////////////////////////////////////
sampler2D _MainTex;
uniform fixed4 _ColorMultiply;
uniform fixed4 _ColorAdd;
uniform fixed _Alpha;
uniform fixed _SaturationAmount;
uniform fixed _BrightnessAmount;
uniform fixed _ContrastAmount;

// AUX METHODS /////////////////////////////////////////////////////////////////////////////////////////////
// Aux method to apply brightness/saturation/contrast factors to a given color
// @see http://armedunity.com/topic/4950-brightnesscontrastsaturation-shader/
fixed3 ContrastSaturationBrightness(fixed3 color, fixed brt, fixed sat, fixed con) {
	//RGB Color Channels
	fixed AvgLumR = 0.5;
	fixed AvgLumG = 0.5;
	fixed AvgLumB = 0.5;
	
	//Luminace Coefficients for brightness of image
	fixed3 LuminaceCoeff = fixed3(0.2125, 0.7154, 0.0721);
	
	//Brigntess calculations
	fixed3 AvgLumin = fixed3(AvgLumR,AvgLumG,AvgLumB);
	fixed3 brtColor = color * brt;
	fixed intensityf = dot(brtColor, LuminaceCoeff);
	fixed3 intensity = fixed3(intensityf, intensityf, intensityf);
	
	//Saturation calculation
	fixed3 satColor = lerp(intensity, brtColor, sat);
	
	//Contrast calculations
	fixed3 conColor = lerp(AvgLumin, satColor, con);
	
	return conColor;
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
	
	// Apply additive color
	_color += _ColorAdd;
	
	// Alpha clipping (don't render below certain alpha levels
	clip(_color.a - 0.01);
}

#endif