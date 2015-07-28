 Shader "Custom/DiffuseRimCaustics" 
 {
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Rimsize ("Rim size", Range (0,1.0)) = 1.0
		_RimIntensity ("Rim Intensity", Range (0,1.0)) = 1.0
		_CausticTex ("Caustic (RGB)", 2D) = "white" {}
		_RimColour ("Rim Colour", COLOR) = (1,1,1,1)
		_CausticColour ("Caustic Colour", COLOR) = (1,1,1,1)
		_CausticSpeed ("Caustic speed", Range (0,1.0)) = 0.1
		_CausticScaleX ("Caustic scale X", Range (0,1.0)) = 0.05
		_CausticScaleY ("Caustic scale Y", Range (0,1.0)) = 0.05
		_Tint ("Tint Colour", COLOR) = (1,1,1,1)
		_TintIntensity ("Tint intensity", float) = 0.0

	}
	
	SubShader 
	{
	
		Tags { "Queue" = "Geometry" }
		
	 	Pass
	 	{
		
			CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360; has structs without semantics (struct v2f members colour)
#pragma exclude_renderers xbox360
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0 
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _CausticTex;

			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    float2 uv1 : TEXCOORD1;
				float4 color : COLOR;	 
				float4 causticClamp : TEXCOORD2;  
			};
			
			struct appdata 
			{
    			float4 vertex : POSITION;
    			float3 normal : NORMAL;
  			    float2 texcoord : TEXCOORD0;
 			};
			
			
			float4 _MainTex_ST;
			float4 _CausticTex_ST;

			float _Rimsize;
			float _RimIntensity;
			float4 _RimColour;
			float4 _CausticColour;
			float _CausticSpeed;
			float _CausticScaleX;
			float _CausticScaleY;
			float4 _Tint;
			float _TintIntensity;

			
			v2f vert (appdata v)
			{
				v2f o;

				float fTime = _Time;

			    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			    float3 viewN = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);


			    // Apply the rim effect
 				float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                float dotProduct = 1 - dot(v.normal, viewDir);
                o.color = smoothstep(1 - _Rimsize, 1.0, dotProduct);
                o.color *= _RimIntensity;

                // Caustics
			    float4 pos = mul (_Object2World, v.vertex);
			    float fDepth = 0.0;
			    
			    if(pos.y < 0.0)
			    {
			   	 	fDepth = abs(pos.y);
			    	fDepth /= 256.0;
				    fDepth = 1.0 - fDepth;
			    }

			    // Work out caustics clamp
			    float3 vUp = float3(0.0,1.0,0.0);
			    float vAngle = dot(vUp,viewN);
			    float fClamp = clamp(vAngle,0.0,1.0);
			    fClamp *= fDepth;

			    o.color.w = fClamp;

			    float2 uv;
			    uv.x = o.pos.x;
			    uv.y = o.pos.y;
			    uv.x *= _CausticScaleX;
			    uv.y *= _CausticScaleY;
				uv.xy += _Time * _CausticSpeed;
			    o.uv1 = uv;

			    o.uv = v.texcoord;
			    
			    return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
			    half4 texcol = tex2D (_MainTex, i.uv);
			    half4 tint = _Tint;
			    tint *= _TintIntensity;
			    texcol += tint;
			    half4 caustic = tex2D (_CausticTex, i.uv1);
			    caustic *= _CausticColour;
			    caustic *= i.color.w;
			    texcol += caustic;
			    //texcol *= 1.5;
			    texcol.xyz += (i.color.xyz * _RimColour.xyz);
 			    return texcol;
			}
			
			ENDCG	
		}
	}
	FallBack "Diffuse"
}
