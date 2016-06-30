 Shader "Hungry Dragon/WaterShader"
 {
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SecondTex ("Blend (RGB)", 2D) = "white" {}
		_SpeedY1 ("Y Speed Layer 1", Range (-10.0,10.0)) = 1.0
		_SpeedY2 ("Y Speed Layer 2", Range (-10.0,10.0)) = 1.0
		_SpeedX1 ("X Speed Layer 1", Range (-10.0,10.0)) = 1.0
		_SpeedX2 ("X Speed Layer 2", Range (-10.0,10.0)) = 1.0
		_FixedTransp ("Fixed Transparency Value", Range(0,1.0)) = 0.0	
		_WaveAmplitude ("Wave Amplitude", float) = 50.0		
		[HideInInspector]
		_Width ("Mesh Width", float) = 2000.0
		[HideInInspector]
		_Near ("Near Fade", float) = 200.0
		[HideInInspector]
		_Far ("Far Fade", float) = 500.0
	}
	
	SubShader 
	{
		Tags { "Queue" = "Transparent" }

	 	Pass
	 	{

			//LOD 200
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
		
			CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members colour)
#pragma exclude_renderers xbox360
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _SecondTex;

			struct v2f 
			{
			    float4  pos : SV_POSITION;
			    float2  uv : TEXCOORD0;
			    float2  uv1 : TEXCOORD1;
			    float2 depth : TEXCOORD2;
			    float4 color : COLOR;
			};
			
			struct appdata 
			{
    			float4 vertex : POSITION;
  			    float2  texcoord : TEXCOORD0;
  			    float2  texcoord1 : TEXCOORD1;
  				float4 color : COLOR;
 			};
			
			
			float4 _MainTex_ST;
			float4 _SecondTex_ST;
			float _SpeedY1;
			float _SpeedY2;
			float _SpeedX1;
			float _SpeedX2;
			float _FixedTransp;
			float _WaveAmplitude;		
			float _Width;
			float _Near;
			float _Far;
			
			v2f vert (appdata v)
			{
			    v2f o;
			    
			    float fTime = _Time + (v.color.x * 1000.0);
			    float4 pos = v.vertex;
			    
			    float fPeriod = 0.3;
			    
			    // clamp down the value of the sine wave period of zero based vertices
			    //if(pos.z < 0.1f && pos.z > -0.1f)
			    //{
			    	//fPeriod = 0.0f;
			    //}
			 			    
				pos.y *= sin(fTime * 100.0) * fPeriod * _WaveAmplitude;
			   		
			   	o.depth.x = pos.z;
			   	
			   	//  Project the vertices out to intersect with background plane
			   	if(o.depth.x > 5.0)
			   	{
			   		pos.z *= 1.6;
			   	}
			   	
			   	if(o.depth.x > 10.0)
			   	{
			   		o.depth.x -= 10.0;
			   		o.depth.x /= 12.0;
			   		o.depth.x = 1.0 - o.depth.x;
			   	}
			   	else
			   	{
			   		o.depth.x = 2.0;
			   	}
			   	
			   	o.depth.y = pos.z;
			   					   				   				   
			    o.pos = mul (UNITY_MATRIX_MVP, pos);
			    o.uv = v.texcoord;
			   	o.uv1 = v.texcoord1;
			   	
			   	// Adjust UV coordinates to do some scrolling of the textures
			   	o.uv.y += _Time * _SpeedY1;
			   	o.uv1.y += _Time * _SpeedY2;
				
				o.uv.x += _Time * _SpeedX1;
			   	o.uv1.x += _Time * _SpeedX2;
			   	   	
			   	
			   	
			    o.color.x = ((pos.y / 50) + 0.3) / 0.6;
			    
			    return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
			    half4 texcol = tex2D (_MainTex, i.uv);
			    half4 texcol2 = tex2D(_SecondTex, i.uv1);
			    half4 finalTex;
			    
			    float fLerp = i.color.x;
			    clamp(fLerp,0,1);
				finalTex = lerp(texcol,texcol2,fLerp);
				
				if ( _FixedTransp == 0.0)
				{
					// Fade off towards the back of the water
					if(i.depth.y > _Width - _Far)
					{
						float fAlpha = 1.0-(abs(i.depth.y - (_Width - _Far)) / _Far);
						clamp(fAlpha,0,1);
						finalTex.w *= fAlpha;	
					}
					
					// Fade out towards the front of the water
					if(i.depth.y < _Near)
					{
						float fAlpha = (abs(i.depth.y) / _Near);
						clamp(fAlpha,0,1);
						finalTex.w *= fAlpha;	
					}
				}
				else
				{
					finalTex.w *= _FixedTransp;
				}						

			    return finalTex; 
			}
			
			ENDCG	
		}
	}
	FallBack "Diffuse"
}
