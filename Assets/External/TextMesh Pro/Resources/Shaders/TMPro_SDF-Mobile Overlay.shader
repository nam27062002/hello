// Copyright (C) 2014 - 2016 Stephan Schaem - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

// Simplified SDF shader:
// - No Shading Option (bevel / bump / env map)
// - No Glow Option
// - Softness is applied on both side of the outline

Shader "TMPro/Mobile/Distance Field Overlay" {

Properties { // Serialized
	_FaceColor			("Face Color", Color) = (1,1,1,1)
	_FaceDilate			("Face Dilate", Range(-1,1)) = 0

	_OutlineColor		("Outline Color", Color) = (0,0,0,1)
	_OutlineWidth		("Outline Thickness", Range(0,1)) = 0
	_OutlineSoftness	("Outline Softness", Range(0,1)) = 0

	_UnderlayColor		("Border Color", Color) = (0,0,0,.5)
	_UnderlayOffsetX 	("Border OffsetX", Range(-1,1)) = 0
	_UnderlayOffsetY 	("Border OffsetY", Range(-1,1)) = 0
	_UnderlayDilate		("Border Dilate", Range(-1,1)) = 0
	_UnderlaySoftness 	("Border Softness", Range(0,1)) = 0

	_WeightNormal		("Weight Normal", float) = 0
	_WeightBold			("Weight Bold", float) = .5

	_ShaderFlags		("Flags", float) = 0
	_ScaleRatioA		("Scale RatioA", float) = 1
	_ScaleRatioB		("Scale RatioB", float) = 1
	_ScaleRatioC		("Scale RatioC", float) = 1

	_MainTex			("Font Atlas", 2D) = "white" {}
	_TextureWidth		("Texture Width", float) = 512
	_TextureHeight		("Texture Height", float) = 512
	_GradientScale		("Gradient Scale", float) = 5
	_ScaleX				("Scale X", float) = 1
	_ScaleY				("Scale Y", float) = 1
	_PerspectiveFilter	("Perspective Correction", Range(0, 1)) = 0.875

	_VertexOffsetX		("Vertex OffsetX", float) = 0
	_VertexOffsetY		("Vertex OffsetY", float) = 0
	//_MaskID				("Mask ID", float) = 0
	_MaskCoord			("Mask Coordinates", vector) = (0, 0, 10000, 10000)
	_ClipRect			("Clip Rect", vector) = (-10000, -10000, 10000, 10000)
	_MaskSoftnessX		("Mask SoftnessX", float) = 0
	_MaskSoftnessY		("Mask SoftnessY", float) = 0

	_ColorMask("Color Mask", Float) = 15
}

SubShader {
	Tags {
		"Queue"="Overlay"
		"IgnoreProjector"="True"
		"RenderType"="Transparent"
	}

	Cull [_CullMode]
	ZWrite Off
	Lighting Off
	Fog { Mode Off }
	Ztest Always
	Blend One OneMinusSrcAlpha
	ColorMask[_ColorMask]

	Pass {
		CGPROGRAM
		#pragma vertex VertShader
		#pragma fragment PixShader
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma shader_feature __ OUTLINE_ON
		#pragma shader_feature __ UNDERLAY_ON UNDERLAY_INNER
		#pragma shader_feature __ MASK_HARD MASK_SOFT

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
		#include "TMPro_Properties.cginc"


		struct vertex_t {
			float4	vertex			: POSITION;
			float3	normal			: NORMAL;
			fixed4	color			: COLOR;
			float2	texcoord0		: TEXCOORD0;
			float2	texcoord1		: TEXCOORD1;
		};

		struct pixel_t {
			float4	vertex			: SV_POSITION;
			fixed4	faceColor		: COLOR;
			fixed4	outlineColor	: COLOR1;
			float2	texcoord0		: TEXCOORD0;
			half4	param			: TEXCOORD1;			// Scale(x), BiasIn(y), BiasOut(z), Bias(w)
			half4	mask			: TEXCOORD2;			// Position(xy) in object space, pixel Size(zw) in screen space
		#if (UNDERLAY_ON | UNDERLAY_INNER)
			float2	texcoord1		: TEXCOORD3;
			fixed4	underlayColor	: TEXCOORD4;
			half2	underlayParam	: TEXCOORD5;			// Scale(x), Bias(y)
		#endif
		};

		pixel_t VertShader(vertex_t input)
		{
			float bold = step(input.texcoord1.y, 0);
			
			float4 vert = input.vertex;
			vert.x += _VertexOffsetX;
			vert.y += _VertexOffsetY;
			float4 vPosition = mul(UNITY_MATRIX_MVP, vert);

			float2 pixelSize = vPosition.w;
			pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
			float scale = rsqrt(dot(pixelSize, pixelSize));
			scale *= abs(input.texcoord1.y) * _GradientScale * 1.5;
			if(UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(input.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

			float weight = lerp(_WeightNormal, _WeightBold, bold) / _GradientScale;
			weight += _FaceDilate * _ScaleRatioA * 0.5;

			float layerScale = scale;

			scale /= 1+(_OutlineSoftness*_ScaleRatioA*scale);
			float bias = (.5-weight)*scale - .5;
			float outline = _OutlineWidth*_ScaleRatioA*.5*scale;

			float opacity = input.color.a;
			fixed4 faceColor = fixed4(input.color.rgb, opacity)*_FaceColor;
			fixed4 outlineColor = _OutlineColor;
			faceColor.rgb *= faceColor.a;
			outlineColor.a *= opacity;
			outlineColor.rgb *= outlineColor.a;
			outlineColor = lerp(faceColor, outlineColor, sqrt(min(1.0, (outline*2))));

		#if (UNDERLAY_ON | UNDERLAY_INNER)
			float4 layerColor = _UnderlayColor;
			layerColor.a *= opacity;
			layerColor.rgb *= layerColor.a;

			layerScale /= 1+((_UnderlaySoftness*_ScaleRatioC)*layerScale);
			float layerBias = (.5-weight)*layerScale - .5 - ((_UnderlayDilate*_ScaleRatioC)*.5*layerScale);

			float x = -_UnderlayOffsetX*_ScaleRatioC*_GradientScale/_TextureWidth;
			float y = -_UnderlayOffsetY*_ScaleRatioC*_GradientScale/_TextureHeight;
			float2 layerOffset = float2(x, y);
		#endif

			pixel_t output = {
				vPosition,
				faceColor,
				outlineColor,
				input.texcoord0,
				half4(scale, bias - outline, bias + outline, bias),
				half4(vert.xy, 0.5 / pixelSize.xy),
			#if (UNDERLAY_ON | UNDERLAY_INNER)
				input.texcoord0+layerOffset,
				layerColor,
				half2(layerScale, layerBias),
			#endif
			};

			return output;
		}

		fixed4 PixShader(pixel_t input) : SV_Target
		{
			half d = tex2D(_MainTex, input.texcoord0).a * input.param.x;
			half4 c = input.faceColor * saturate(d - input.param.w);

		#ifdef OUTLINE_ON
			c = lerp(input.outlineColor, input.faceColor, saturate(d - input.param.z));
			c *= saturate(d - input.param.y);
		#endif

		#if UNDERLAY_ON
			half sd2 = saturate(d - input.param.y); // includes outline
			d = tex2D(_MainTex, input.texcoord1).a * input.underlayParam.x;
			c += input.underlayColor * (saturate(d - input.underlayParam.y)) * (1 - sd2) * (1 - c.a);
		#endif

		#if UNDERLAY_INNER
			half sd = saturate(d - input.param.z);
			d = tex2D(_MainTex, input.texcoord1).a * input.underlayParam.x;
			c += input.underlayColor * (1 - saturate(d - input.underlayParam.y)) * sd * (1 - c.a);
		#endif

		// Support for 2D RectMask
		#if UNITY_VERSION < 530
			if (_UseClipRect)
				c *= UnityGet2DClipping(input.mask.xy, _ClipRect);
		#else
			c *= UnityGet2DClipping(input.mask.xy, _ClipRect);
		#endif


		#if MASK_HARD
			half2 m = 1 - saturate((abs(input.mask.xy) - _MaskCoord.zw) * input.mask.zw);
			c *= m.x * m.y;
		#endif

		#if MASK_SOFT
			half2 s = half2(_MaskSoftnessX, _MaskSoftnessY) * input.mask.zw;
			half2 m = 1 - saturate(((abs(input.mask.xy) - _MaskCoord.zw) * input.mask.zw + s) / (1 + s));
			m *= m;
			c *= m.x * m.y;
		#endif

			return c;
		}
		ENDCG
	}
}

CustomEditor "TMPro_SDFMaterialEditor"
}
