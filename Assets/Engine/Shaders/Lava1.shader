Shader "Hungry Dragon/Lava1" {
Properties 
	{
		_MainTex ("Base layer (RGB)", 2D) = "white" {}
		_Speed ("Speed", Float) = 1.0
	}
	SubShader {
		Tags { "Queue"="Geometry" "RenderType"="Opaque" "LightMode" = "ForwardBase" }
		
		Lighting Off
		ZWrite On
		LOD 100
		
		CGINCLUDE
		#pragma vertex vert
		#pragma fragment frag
		#include "UnityCG.cginc" 
		#pragma multi_compile_fwdbase 
		#include "AutoLight.cginc"

		sampler2D _MainTex;
		
		float _Speed;

		float4 _MainTex_ST;
		
		struct v2f 
		{ 
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;				
			
			#ifndef LIGHTMAP_OFF
			float2 lmap : TEXCOORD3; 
			#endif		
			
			#ifdef DYNAMIC_SHADOWS
			LIGHTING_COORDS(6, 7)
			#endif
		};
		
		float _Distance;
		
		v2f vert (appdata_base v)
		{
			v2f o; 
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = TRANSFORM_TEX(v.texcoord.xy,_MainTex);					

			#ifndef LIGHTMAP_OFF
			o.lmap = v.texcoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			#endif

			#ifdef DYNAMIC_SHADOWS
			TRANSFER_VERTEX_TO_FRAGMENT(o);
			#endif
			
			return o;
		}
		ENDCG 
		
		Pass  
		{ 
			CGPROGRAM						
			#pragma vertex vert
			#pragma fragment frag			
			
			
			fixed4 frag (v2f i) : COLOR
			{	
				fixed2 uv;				
    			uv.x = i.uv.x + sin(_Time* _Speed+i.uv.x*4.0)*0.025 - cos(_Time*_Speed+i.uv.y*2.0)*0.035;
    			uv.y = i.uv.y + sin(_Time* _Speed+i.uv.y*4.0)*0.025 - cos(_Time*_Speed+i.uv.x*2.0)*0.035;
    			
    			fixed4 tex = tex2D (_MainTex, uv);
								
				#ifndef LIGHTMAP_OFF
				fixed3 lm = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lmap));
				tex.rgb *= lm;
				#endif

				#ifdef DYNAMIC_SHADOWS
				float attenuation = LIGHT_ATTENUATION(i);
				tex.rgb *= attenuation;											
				#endif
				
    			return tex;	
			}
			ENDCG
		}
	}
}
