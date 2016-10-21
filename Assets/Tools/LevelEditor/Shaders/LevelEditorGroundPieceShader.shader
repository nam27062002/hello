// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// @see http://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
Shader "LevelEditor/LevelEditorGroundPieceShader" {
	//--------------------------------------------------------------//
	// EXPOSED PROPERTIES						//
	//--------------------------------------------------------------//
	Properties {
		_Color("Color", Color) = (1, 0, 0, 1)
	}
	
	//--------------------------------------------------------------//
	// SHADERS							//
	//--------------------------------------------------------------//
	SubShader {
		//--------------------------------------------------------------//
		// SETUP							//
		//--------------------------------------------------------------//
		Tags { "RenderType" = "Opaque" }
		
		// First pass
		Pass {
			CGPROGRAM
			
			//--------------------------------------------------------------//
			// INCLUDES AND PREPROCESSOR					//
			//--------------------------------------------------------------//
			#include "UnityCG.cginc"
			
			#pragma vertex vert
	        	#pragma fragment frag
	        	#pragma target 3.0
			
			//--------------------------------------------------------------//
			// CONSTANTS							//
			//--------------------------------------------------------------//
			struct Vertex2Fragment {
				float4 position : POSITION;
	  			float4 customColor : COLOR;
			};
			
			//--------------------------------------------------------------//
			// PARAMETERS							//
			//--------------------------------------------------------------//
			fixed4 _Color;
			
			//--------------------------------------------------------------//
			// VERTEX SHADER						//
			//--------------------------------------------------------------//
			Vertex2Fragment vert(appdata_full _v) {
				Vertex2Fragment v2f;
				v2f.position = mul(UNITY_MATRIX_MVP, _v.vertex);
				
				float4 vertexWorldPos = mul(unity_ObjectToWorld, _v.vertex);
				float3 cameraVector = _WorldSpaceCameraPos - vertexWorldPos;
				float3 worldNormal = mul(unity_ObjectToWorld, float4(_v.normal, 0));
				float delta = dot(worldNormal, normalize(cameraVector));	// Default diffuse lambert formula
				
				v2f.customColor.xyz = _Color * delta;
          			v2f.customColor.w = 1.0;
				return v2f;
			}
			
			//--------------------------------------------------------------//
			// FRAGMENT SHADER						//
			//--------------------------------------------------------------//
			float4 frag(Vertex2Fragment _in) : SV_Target {
		  		float4 ret = _in.customColor;
		  		return ret;
			}
	      		ENDCG
      		}
	}
	FallBack "Diffuse"
}
