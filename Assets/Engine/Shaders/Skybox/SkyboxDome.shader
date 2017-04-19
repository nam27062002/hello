// - Unlit
// - Scroll 2 layers /w Multiplicative op

Shader "Hungry Dragon/Skybox/Dome Skybox" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_DetailOffset("Initial detail offset:", Vector) = (0.0, 0.0, 0.0, 0.0)
	_UpColor("Up Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_DownColor("Down Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_SatThreshold("Saturation threshold", Range(0.0, 1.0)) = 0.5
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Opaque" }
	
	Lighting Off Fog { Mode Off }
	ZWrite Off
	
	LOD 100
			
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#include "UnityCG.cginc"
	#include "../HungryDragon.cginc"

	sampler2D _MainTex;
	sampler2D _DetailTex;

	float4 _MainTex_ST;
	float4 _DetailTex_ST;

	float2 _DetailOffset;
	float4 _UpColor;
	float4 _DownColor;

	float	_SatThreshold;

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
		float  height : TEXCOORD2;
		HG_FOG_COORDS(3)
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex); 
		o.height = v.texcoord.y;
		o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex);
		HG_TRANSFER_FOG(o, mul(unity_ObjectToWorld, v.vertex));	// Fog
		return o;
	}
	ENDCG


	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest		
		fixed4 frag (v2f i) : COLOR
		{
			
			fixed4 tex = tex2D (_MainTex, i.uv);
			fixed4 tex2 = tex2D (_DetailTex, i.uv2 + _DetailOffset);
			

			fixed4 one = fixed4(1,1,1,1);
			fixed4 col = one - (one-tex) * (one-tex2);

			fixed sat = smoothstep(_SatThreshold, 1.0, 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b);

			float4 grad = lerp(_DownColor, _UpColor, i.height);
//			return grad;

			col = lerp(grad, col, sat);

			float4 colbackup = col;

//			return grad;

			HG_APPLY_FOG(i, col);	// Fog

//			col = lerp(colbackup, col, _DownColor.w);
			col = lerp(colbackup, col, fogCol.w * _DownColor.w);
			

			UNITY_OPAQUE_ALPHA(col.a);	// Opaque

			return col;
		}
		ENDCG 
	}	
}
}
