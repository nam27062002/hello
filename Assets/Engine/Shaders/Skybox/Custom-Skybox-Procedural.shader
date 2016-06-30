Shader "Hungry Dragon/Skybox/Procedural" {
Properties {
	_SunSize ("Sun size", Range(0,1)) = 0.04
	_SkyColor ("Sky Color", Color) = (.5, .5, .5, 1)
	_HorizonColor ("Horizon Color", Color) = (.5, .5, .5, 1)
	_HorizonHeight ("Horizon Height", Range(0,1)) = 1
	_GroundColor ("Ground Color", Color) = (.369, .349, .341, 1)
	_SunPos( "Sun Pos", Vector) = (0,0,0,0)
	// _AddColor ("Add Color", Color) = (1, 1, 1, 0)
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
	Cull Off ZWrite Off

	Pass {
		
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#pragma multi_compile __ UNITY_COLORSPACE_GAMMA

		uniform half3 _GroundColor;
		uniform half _SunSize;
		uniform half3 _SkyColor;
		uniform half3 _HorizonColor;
		uniform half _HorizonHeight;
		uniform float4 _SunPos;
		// uniform half3 _AddColor;

		// RGB wavelengths
		#define kSUN_BRIGHTNESS 20.0 	// Sun brightness
		static const half kSunScale = 400.0 * kSUN_BRIGHTNESS;

		struct appdata_t {
			float4 vertex : POSITION;
		};

		struct v2f 
		{
				float4 pos : SV_POSITION;
				half3 rayDir : TEXCOORD0;	// Vector for incoming ray, normalized ( == -eyeRay )
   		}; 

		float scale(float inCos)
		{
			float x = 1.0 - inCos;
			return 0.25 * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
		}

		v2f vert (appdata_t v)
		{
			v2f OUT;
			OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			// Get the ray from the camera to the vertex and its length (which is the far point of the ray passing through the atmosphere)
			float3 eyeRay = normalize(mul((float3x3)_Object2World, v.vertex.xyz));
			OUT.rayDir = half3(-eyeRay);
			return OUT;
		}

		half calcSunSpot(half3 vec1, half3 vec2)
		{
			half3 delta = vec1 - vec2;
			half dist = length(delta);
			half spot = 1.0 - smoothstep(0.0, _SunSize, dist);
			return kSunScale * spot * spot;
		}

		half4 frag (v2f IN) : SV_Target
		{
			half3 col = half3(0.0, 0.0, 0.0);
			// < 0.0 means over horizon, add a bit because we need to lerp to hide the horizon line
			if(IN.rayDir.y < 0.02)
			{
				half3 ray = normalize(IN.rayDir.xyz);
				half eyeCos = dot(_SunPos.xyz, ray);
				half eyeCos2 = eyeCos * eyeCos;

				// half mie = getMiePhase(eyeCos, eyeCos2);
				half mie = calcSunSpot(_SunPos.xyz, -ray);

				col = _SkyColor;
				if ( -IN.rayDir.y < _HorizonHeight )
				{
					col = lerp( _HorizonColor, _SkyColor, -IN.rayDir.y / _HorizonHeight);	
				}


				if(IN.rayDir.y >= 0.0)
				{
					half3 groundColor = _GroundColor;
					col = lerp(col, groundColor, IN.rayDir.y / 0.02);
				}
				else
				{
					col += mie;
				}
			}
			else
			{
				col = _GroundColor;
			}

 
			// return half4(col + _AddColor,1.0);
			return half4(col,1.0);

		}
		ENDCG 
	}
} 	


Fallback Off

}
