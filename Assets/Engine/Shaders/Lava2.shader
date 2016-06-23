Shader "Sandstorm/Lava2" {
Properties 
	{
		_MainTex ("Base layer (RGB)", 2D) = "white" {}
		_DetailTex ("2nd layer (RGB)", 2D) = "white" {}
		
		_SpeedX ("Scroll speed X", Float) = 1.0
		_SpeedY ("Wave speed Y", Float) = 0.0
	}
	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		
		Pass  
		{ 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc" 
			
			sampler2D _MainTex;
			sampler2D _DetailTex;
			
			float _SpeedX;
			float _SpeedY;
	
			float4 _MainTex_ST;
			float4 _DetailTex_ST;
			
			struct v2f 
			{ 
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};
			
			float _Distance;
			
			v2f vert (appdata_base v)
			{
				v2f o; 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
				o.uv2 = TRANSFORM_TEX(v.texcoord.xy,_DetailTex) + frac(float2(_SpeedX, 0) * _Time);
		
				return o;
			}
			
			
			
			fixed4 frag (v2f i) : COLOR
			{	
    			fixed4 tex = tex2D (_MainTex, i.uv);
				
				fixed2 uv2;
				uv2.x = i.uv2.x;
				uv2.y = i.uv2.y + sin(_Time* _SpeedY+i.uv2.y*4.0)*0.025;
				fixed4 tex2 = tex2D (_DetailTex, uv2);
				
				return lerp( tex, tex2 * tex * 4, (1,1,1,1) * (tex.r / 2.0));
				// return tex2;
			}
			ENDCG
		}
	} 
}
