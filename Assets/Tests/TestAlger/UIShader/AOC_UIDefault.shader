Shader "AOC/UI/Default"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
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

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = mul(UNITY_MATRIX_MVP, OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				
//				#ifdef UNITY_HALF_TEXEL_OFFSET
//				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
//				#endif
				
				OUT.color = IN.color * _Color;
				//return OUT;

				// =============================================
				// OUTLINE TEST
				// From http://stackoverflow.com/questions/19487162/how-to-make-a-outline-shader-with-custom-attributes-in-unity3d
				// =============================================
				// just make a copy of incoming vertex data but scaled according to normal direction
//				v2f o;
//				o.pos = mul(UNITY_MATRIX_MVP, IN.vertex);
//
//				float3 norm   = mul((float3x3)UNITY_MATRIX_IT_MV, IN.normal);
//				float2 offset = TransformViewToProjection(norm.xy);
//
//				o.pos.xy += offset * o.pos.z * _Outline;
//				o.color = _OutlineColor;
//				return o;
				// =============================================

				// =============================================
				// RANDOM TEST
				// =============================================
				//OUT.color = fixed4(IN.normal.xyz, IN.color.a);
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

				//color = half4(IN.color.rgb, tex2D(_MainTex, IN.texcoord).a * IN.color.a);
				//color = half4(IN.texcoord.x, IN.texcoord.y, 0, 1);
				color = half4(0, 1, 0, 1);
				
				//color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				//clip (color.a - 0.001);
				#endif

				return color;
			}
		ENDCG
		}
	}
}
