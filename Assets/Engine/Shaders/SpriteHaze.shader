Shader "Hungry Dragon/Sprite Haze"
{
	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_IntensityAndScrolling ("Intensity (XY); Scrolling (ZW)", Vector) = (0.1,0.1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent+10" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off 
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		GrabPass { "_GrabTexture" }

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				// float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;	
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				// fixed4 color    : COLOR;
				half4 texcoord  : TEXCOORD0; // xy = uv, zw = distort uv 
				half4 screenuv : TEXCOORD1; // xy=screenuv, z=distance dependend intensity, w=depth
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _GrabTexture;
			float4 _IntensityAndScrolling;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord.xy = TRANSFORM_TEX(IN.texcoord, _MainTex);
				OUT.texcoord.zw = OUT.texcoord.xy + (_Time.gg * _IntensityAndScrolling.zw); // Apply texture scrolling.

				half4 screenpos = ComputeGrabScreenPos(OUT.vertex);
				OUT.screenuv.xy = screenpos.xy / screenpos.w;

				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				half2 distort = tex2D(_MainTex, IN.texcoord.zw).xy;
				half2 offset = (distort.xy * 2 - 1) * _IntensityAndScrolling.xy;

				half  mask = tex2D(_MainTex, IN.texcoord.xy).b;
				offset *= mask;

				// get screen space position of current pixel
				half2 uv = IN.screenuv.xy + offset;

				half4 color = tex2D(_GrabTexture, uv);
				// UNITY_OPAQUE_ALPHA(color.a);
				return color;

			}
		ENDCG
		}
	}
}
