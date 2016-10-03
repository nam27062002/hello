// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32956,y:32703,varname:node_3138,prsc:2|emission-6132-OUT,alpha-8928-OUT;n:type:ShaderForge.SFN_Tex2d,id:8842,x:31447,y:32482,ptovrint:False,ptlb:diffuse TEX,ptin:_diffuseTEX,varname:node_8842,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:9259,x:31757,y:32505,varname:node_9259,prsc:2|A-8842-RGB,B-4978-RGB,C-8184-OUT;n:type:ShaderForge.SFN_VertexColor,id:3670,x:31469,y:32969,varname:node_3670,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8928,x:32244,y:33080,varname:node_8928,prsc:2|A-8842-A,B-3670-A;n:type:ShaderForge.SFN_FaceSign,id:9858,x:32155,y:32837,varname:node_9858,prsc:2,fstp:0;n:type:ShaderForge.SFN_Lerp,id:9277,x:32513,y:32686,varname:node_9277,prsc:2|A-9259-OUT,B-7161-OUT,T-415-OUT;n:type:ShaderForge.SFN_Multiply,id:7161,x:32173,y:32703,varname:node_7161,prsc:2|A-9259-OUT,B-9469-OUT;n:type:ShaderForge.SFN_Slider,id:9469,x:31782,y:32716,ptovrint:False,ptlb:backface lightness,ptin:_backfacelightness,varname:node_9469,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_OneMinus,id:415,x:32361,y:32834,varname:node_415,prsc:2|IN-9858-VFACE;n:type:ShaderForge.SFN_SwitchProperty,id:6132,x:32726,y:32533,ptovrint:False,ptlb:use fake backface lighting,ptin:_usefakebackfacelighting,varname:node_6132,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-9259-OUT,B-9277-OUT;n:type:ShaderForge.SFN_Color,id:4978,x:31460,y:32672,ptovrint:False,ptlb:colour,ptin:_colour,varname:node_4978,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:8184,x:31461,y:32877,ptovrint:False,ptlb:emissive,ptin:_emissive,varname:node_8184,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;proporder:8842-9469-6132-4978-8184;pass:END;sub:END;*/

Shader "Shader Forge/sh_part_alpha" {
    Properties {
        _diffuseTEX ("diffuse TEX", 2D) = "white" {}
        _backfacelightness ("backface lightness", Range(0, 1)) = 1
        [MaterialToggle] _usefakebackfacelighting ("use fake backface lighting", Float ) = 0
        _colour ("colour", Color) = (0.5,0.5,0.5,1)
        _emissive ("emissive", Float ) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
//            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _diffuseTEX; uniform float4 _diffuseTEX_ST;
            uniform float _backfacelightness;
            uniform fixed _usefakebackfacelighting;
            uniform float4 _colour;
            uniform float _emissive;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 _diffuseTEX_var = tex2D(_diffuseTEX,TRANSFORM_TEX(i.uv0, _diffuseTEX));
                float3 node_9259 = (_diffuseTEX_var.rgb*_colour.rgb*_emissive);
                float3 emissive = lerp( node_9259, lerp(node_9259,(node_9259*_backfacelightness),(1.0 - isFrontFace)), _usefakebackfacelighting );
                float3 finalColor = emissive;
                return fixed4(finalColor,(_diffuseTEX_var.a*i.vertexColor.a));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
