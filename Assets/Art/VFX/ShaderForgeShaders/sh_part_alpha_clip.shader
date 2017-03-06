// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:32724,y:32693,varname:node_4795,prsc:2|emission-8136-OUT,alpha-5298-OUT,clip-4579-OUT;n:type:ShaderForge.SFN_Tex2d,id:562,x:31707,y:32730,ptovrint:False,ptlb:diffuse TEX,ptin:_diffuseTEX,varname:_node_8994,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:0255f300b1d4cf245874f5a591c18ad5,ntxv:0,isnm:False;n:type:ShaderForge.SFN_VertexColor,id:8386,x:31760,y:32942,varname:node_8386,prsc:2;n:type:ShaderForge.SFN_Multiply,id:675,x:32225,y:32977,varname:node_675,prsc:2|A-562-G,B-8386-A;n:type:ShaderForge.SFN_Multiply,id:8136,x:32208,y:32733,varname:node_8136,prsc:2|A-8386-RGB,B-5985-OUT;n:type:ShaderForge.SFN_Power,id:4579,x:32467,y:33038,varname:node_4579,prsc:2|VAL-675-OUT,EXP-9853-OUT;n:type:ShaderForge.SFN_Slider,id:9853,x:32086,y:33188,ptovrint:False,ptlb:clip mask,ptin:_clipmask,varname:node_8312,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:2;n:type:ShaderForge.SFN_Slider,id:5985,x:31611,y:32538,ptovrint:False,ptlb:emissive,ptin:_emissive,varname:_clipmask_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:5;n:type:ShaderForge.SFN_Multiply,id:5298,x:32411,y:32486,varname:node_5298,prsc:2|A-5985-OUT,B-562-G;proporder:562-9853-5985;pass:END;sub:END;*/

Shader "Shader Forge/sh_part_alpha_clip" {
    Properties {
        _diffuseTEX ("diffuse TEX", 2D) = "white" {}
        _clipmask ("clip mask", Range(0, 2)) = 1
        _emissive ("emissive", Range(0, 5)) = 1
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
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
//            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _diffuseTEX; uniform float4 _diffuseTEX_ST;
            uniform float _clipmask;
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
            float4 frag(VertexOutput i) : COLOR {
                float4 _diffuseTEX_var = tex2D(_diffuseTEX,TRANSFORM_TEX(i.uv0, _diffuseTEX));
                float node_4579 = pow((_diffuseTEX_var.g*i.vertexColor.a),_clipmask);
                clip(node_4579 - 0.5);
////// Lighting:
////// Emissive:
                float3 node_8136 = (i.vertexColor.rgb*_emissive);
                float3 emissive = node_8136;
                float3 finalColor = emissive;
                return fixed4(finalColor,(_emissive*_diffuseTEX_var.g));
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
