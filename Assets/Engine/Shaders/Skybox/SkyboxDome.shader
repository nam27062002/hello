// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// - Unlit
// - Scroll 2 layers /w Multiplicative op

Shader "Hungry Dragon/Skybox/Dome Skybox" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_MoonTex("Moon(RGB)", 2D) = "white" {}
	_DetailOffset("Initial detail offset:", Vector) = (0.0, 0.0, 0.0, 0.0)
	_MoonOffset("Initial moon offset:", Vector) = (0.0, 0.0, 0.0, 0.0)
	_MoonColor("Moon Color:", Color) = (1.0, 1.0, 1.0, 1.0)
	_UpColor("Up Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_DownColor("Down Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_SatThreshold("Saturation threshold", Range(0.0, 1.0)) = 0.5
	[Toggle(AUTOMATIC_PANNING)] _AutomaticPanning("Automatic Panning", Float) = 0
	_PanSpeed("Pan speed", float) = 0.0
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Opaque" }
	
	Lighting Off Fog { Mode Off }
	ZWrite Off
	
	LOD 100
			
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#pragma shader_feature __ AUTOMATIC_PANNING
    #pragma multi_compile __ NIGHT
	#include "UnityCG.cginc"
	#include "../HungryDragon.cginc"

	sampler2D	_MainTex;
	sampler2D	_DetailTex;
	sampler2D	_MoonTex;

	float4		_MainTex_ST;
	float4		_DetailTex_ST;
	float4		_MoonTex_ST;

	float2		_DetailOffset;
	float2		_MoonOffset;
	float4		_UpColor;
	float4		_DownColor;
	float4		_MoonColor;

	float		_SatThreshold;

#ifdef AUTOMATIC_PANNING
	float		_PanSpeed;
#endif

	HG_FOG_VARIABLES

	struct appdata
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
		float2 uv3 : TEXCOORD2;
		float  height : TEXCOORD3;
		HG_FOG_COORDS(4)
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
#ifdef AUTOMATIC_PANNING
		float2 anim = float2(_Time.y * _PanSpeed, 0.0f);
		o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex) + anim;
		anim = float2(_Time.y * _PanSpeed * 0.5, 0.0f);
		o.uv2 = TRANSFORM_TEX(v.texcoord.xy, _DetailTex) + anim + _DetailOffset;
#else
		o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
		o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex) + _DetailOffset;
#endif
		o.uv3 = TRANSFORM_TEX(v.texcoord.xy, _MoonTex) + _MoonOffset;
		o.height = v.texcoord.y;
		HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
//		#pragma fragmentoption ARB_precision_hint_fastest		
		fixed4 frag (v2f i) : COLOR
		{			
/*#ifdef AUTOMATIC_PANNING
			float2 anim = float2(_Time.y * _PanSpeed, 0.0f);
			fixed4 tex = tex2D(_MainTex, i.uv + anim);
			anim = float2(_Time.y * _PanSpeed * 0.5, 0.0f);
			fixed4 tex2 = tex2D(_DetailTex, i.uv2 + _DetailOffset + anim);
#else*/
#if defined(NIGHT)
            fixed4 night = fixed4(0.25, 0.25, 0.5, 1.0);
#else
            fixed4 night = fixed4(1.0, 1.0, 1.0, 1.0);
#endif


			fixed4 tex = tex2D(_MainTex, i.uv) * night;
			fixed4 tex2 = tex2D (_DetailTex, i.uv2) * night;
//#endif
			fixed4 tex3 = tex2D(_MoonTex, i.uv3) * _MoonColor;

			fixed4 one = fixed4(1,1,1,1);
			fixed4 col = one - ((one - tex) * (one - tex2) * (one - tex3));

//			col = max(col, tex3);


			fixed sat = smoothstep(_SatThreshold, 1.0, 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b);
//			fixed satMoon = smoothstep(_SatThreshold, 1.0, 0.2126 * tex3.r + 0.7152 * tex3.g + 0.0722 * tex3.b) * 1.0;

			float4 grad = lerp(_DownColor * night, _UpColor * night, i.height);
//			return grad;

			col = lerp(grad, col, sat);

			float4 colbackup = col;

//			return grad;

			HG_APPLY_FOG(i, col);	// Fog

//			col = lerp(colbackup, col, _DownColor.w);
			col = lerp(colbackup, col, (fogCol.w * _DownColor.w * night));
			col = lerp(col, colbackup, clamp(tex3.r, 0.0, 1.0) * _UpColor.w * night);


			UNITY_OPAQUE_ALPHA(col.a);	// Opaque

			return col;
		}
		ENDCG 
	}	
}
}
