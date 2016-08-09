// - Unlit
// - Scroll 2 layers /w Multiplicative op

Shader "Hungry Dragon/Skybox/Dome Skybox" {
Properties {
	_MainTex ("Base layer (RGB)", 2D) = "white" {}
	_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
	_Scroll2X ("2nd layer Scroll speed X", Float) = 1.0
	_Color("Color", Color) = (1,1,1,1)
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Opaque" }
	
	Lighting Off Fog { Mode Off }
	ZWrite Off
	
	LOD 100
	
		
	CGINCLUDE
	#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	sampler2D _DetailTex;

	float4 _MainTex_ST;
	float4 _DetailTex_ST;

	float _Scroll2X;

	fixed4 _Color;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		// fixed4 color : COLOR;
	};
	
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
		fixed4 color : TEXCOORD2;
	};

	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
		o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex) + frac(float2(_Scroll2X, 0) * _Time);
		o.color = lerp( fixed4(1,1,1,1), _Color, o.uv.y);
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
			fixed4 o;
			fixed4 tex = tex2D (_MainTex, i.uv);
			fixed4 tex2 = tex2D (_DetailTex, i.uv2);
			
			// o = (tex + tex2);
			fixed4 one = fixed4(1,1,1,1);
			o = one - (one-tex) * (one-tex2);

			o = o * i.color;
			
			return o;
		}
		ENDCG 
	}	
}
}
