Shader "Custom/Transparent2Faces" {

 	Properties 
{
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
	SubShader 
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent"}
		
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off
		Lighting Off
		Pass
		{
			SetTexture [_MainTex] 
		}
		
	} 
}
