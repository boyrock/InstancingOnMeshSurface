Shader "VertexAnim/OneTime (Instanced)" { 
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
		
		_AnimTex ("PosTex", 2D) = "white" {}
		_AnimTex_Scale ("Scale", Vector) = (1,1,1,1)
		_AnimTex_Offset ("Offset", Vector) = (0,0,0,0)
		_AnimTex_AnimEnd ("End (Time, Frame)", Vector) = (0, 0, 0, 0)
		_AnimTex_T ("Time", float) = 0
		_AnimTex_FPS ("Frame per Sec(FPS)", Float) = 30
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 700 Cull Off
		
		Pass {
			CGPROGRAM
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Packages/VertexAnimator/Shader/AnimTexture.cginc"

            struct vsin {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                uint vid: SV_VertexID;
                UNITY_INSTANCE_ID
            };

            struct vs2ps {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_INSTANCE_ID
            };
            
            sampler2D _MainTex;
            float4 _Color;
            
            vs2ps vert(vsin v) {
                vs2ps OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, OUT);


                float t = UNITY_ACCESS_INSTANCED_PROP(_AnimTex_T);
                t = clamp(t, 0, _AnimTex_AnimEnd.x);
                v.vertex.xyz = AnimTexVertexPos(v.vertex, v.vid, t);
                
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.uv = v.texcoord;
                return OUT;
            }

            float4 frag(vs2ps IN) : COLOR {
                UNITY_SETUP_INSTANCE_ID(IN);
                return tex2D(_MainTex, IN.uv) * _Color;
            }
			ENDCG
		}
	}
	FallBack Off
}
