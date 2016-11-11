// Shader created with Shader Forge v1.25 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.25;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True;n:type:ShaderForge.SFN_Final,id:4795,x:32724,y:32693,varname:node_4795,prsc:2|emission-7629-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:31799,y:32545,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:faa141712fbaf3446931178edf8dceeb,ntxv:0,isnm:False|UVIN-3548-UVOUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32034,y:32802,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_OneMinus,id:3381,x:32271,y:32805,varname:node_3381,prsc:2|IN-2053-RGB;n:type:ShaderForge.SFN_Subtract,id:7629,x:32376,y:32609,varname:node_7629,prsc:2|A-5127-OUT,B-3381-OUT;n:type:ShaderForge.SFN_Multiply,id:5127,x:32104,y:32653,varname:node_5127,prsc:2|A-6074-RGB,B-9602-OUT;n:type:ShaderForge.SFN_Slider,id:9602,x:31658,y:32761,ptovrint:False,ptlb:emissive,ptin:_emissive,varname:node_9602,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:10;n:type:ShaderForge.SFN_UVTile,id:3548,x:31585,y:32562,varname:node_3548,prsc:2|UVIN-4104-UVOUT,WDT-7294-OUT,HGT-7294-OUT,TILE-2578-OUT;n:type:ShaderForge.SFN_Vector1,id:7294,x:31383,y:32590,varname:node_7294,prsc:2,v1:2;n:type:ShaderForge.SFN_ValueProperty,id:2578,x:31382,y:32734,ptovrint:False,ptlb:sprite,ptin:_sprite,varname:node_2578,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_TexCoord,id:4104,x:31379,y:32406,varname:node_4104,prsc:2,uv:0;proporder:6074-9602-2578;pass:END;sub:END;*/

Shader "Shader Forge/sh_part_atlas_exposure_overlay_always" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _emissive ("emissive", Range(0, 10)) = 1
        _sprite ("sprite", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Overlay"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            ZTest Always
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
//            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _emissive;
            uniform float _sprite;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float node_7294 = 2.0;
                float2 node_3548_tc_rcp = float2(1.0,1.0)/float2( node_7294, node_7294 );
                float node_3548_ty = floor(_sprite * node_3548_tc_rcp.x);
                float node_3548_tx = _sprite - node_7294 * node_3548_ty;
                float2 node_3548 = (i.uv0 + float2(node_3548_tx, node_3548_ty)) * node_3548_tc_rcp;
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3548, _MainTex));
                float3 emissive = ((_MainTex_var.rgb*_emissive)-(1.0 - i.vertexColor.rgb));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
