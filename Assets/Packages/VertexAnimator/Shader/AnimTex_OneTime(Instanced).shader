// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "VertexAnim/OneTime (Instanced)" { 
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_HanabiraMaskTex("Hanabira Mask", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Velocity("Velocity", Vector) = (0,0,0,0)

		
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
				float2 texcoord1 : TEXCOORD1;
                uint vid: SV_VertexID;
                UNITY_INSTANCE_ID
            };

            struct vs2ps {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_INSTANCE_ID
            };

			sampler2D _HanabiraMaskTex;
            sampler2D _MainTex;
            float4 _Color;
			//float3 _Velocity;
			//float _Scale;

			//float _Scale;

			float4 _Move;
			//uniform float4x4 _Position;
            
            vs2ps vert(vsin v) {
                vs2ps OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, OUT);

                float t = UNITY_ACCESS_INSTANCED_PROP(_AnimTex_T);
				float s = UNITY_ACCESS_INSTANCED_PROP(_Scale);
				float4x4 mtx = UNITY_ACCESS_INSTANCED_PROP(_Position);
				float3 vel = UNITY_ACCESS_INSTANCED_PROP(_Velocity);

                t = clamp(t, 0, _AnimTex_AnimEnd.x);
                v.vertex.xyz = AnimTexVertexPos(v.vertex, v.vid, t);

				float4 tex = tex2Dlod(_HanabiraMaskTex, float4(v.texcoord1.xy, 0, 0));

				float4x4 m;

				if (tex.x == 1 && tex.y == 1 && tex.z == 1 && t >= 29)
				{
					m = mtx;
				}
				else 
				{
					m = unity_ObjectToWorld;
					vel = float3(0, 0, 0);
				}
				
				float4 worldPos = mul(m, float4((v.vertex.xyz) * s, 1));

				OUT.vertex = mul(UNITY_MATRIX_VP, float4(worldPos.xyz + vel,1));
                OUT.uv = v.texcoord;
                
				return OUT;
            }

            float4 frag(vs2ps IN) : COLOR {
                UNITY_SETUP_INSTANCE_ID(IN);
				//float4 tex = tex2Dlod(_HanabiraMaskTex, float4(IN.uv,0,0));
				return tex2D(_MainTex, IN.uv) * _Color;
            }
			ENDCG
		}
	}
	FallBack Off
}
