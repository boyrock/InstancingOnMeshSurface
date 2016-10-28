Shader "VertexAnim/OneTime" { 
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_LeafMaskTex("LeafMaskTex", 2D) = "white" {}
		_LeafBrightness("LeafBrightness", float) = 0.6
		_BloomIntencity ("BloomIntencity", float) = 0.22
		
		_AnimTex ("PosTex", 2D) = "white" {}
		_AnimTex_Scale ("Scale", Vector) = (1,1,1,1)
		_AnimTex_Offset ("Offset", Vector) = (0,0,0,0)
		_AnimTex_AnimEnd ("End (Time, Frame)", Vector) = (0, 0, 0, 0)
		_AnimTex_T ("Time", float) = 0
		_AnimTex_Transition_From_T("TransitionFromTime", float) = 0
		_AnimTex_Transition_To_T("TransitionToTime", float) = 0
		_AnimTex_Transition_T("TransitionTime", Range(0,2)) = 0
		_AnimTex_FPS ("Frame per Sec(FPS)", Float) = 30
	}


	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 700 Cull Off
		
		Pass {
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Packages/VertexAnimator/Shader/AnimTexture.cginc"

            struct vsin {
                uint vid: SV_VertexID;
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct vs2ps {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

			sampler2D _LeafMaskTex;
            sampler2D _MainTex;
			float _LeafBrightness;
			float _BloomIntencity;

			float _AnimTex_Transition_T;
			float _AnimTex_Transition_From_T;
			float _AnimTex_Transition_To_T;
            
            vs2ps vert(vsin v) {
                float t = _AnimTex_T;
				float transition_t = _AnimTex_Transition_T;
				float transition_from_t = _AnimTex_Transition_From_T;
				float transition_to_t = _AnimTex_Transition_To_T;

                t = clamp(t, 0, _AnimTex_AnimEnd.x);

				if (transition_t <= 1)
				{
					float3 from = AnimTexVertexPos(v.vertex, v.vid, transition_from_t);
					float3 to = AnimTexVertexPos(v.vertex, v.vid, transition_to_t);
					v.vertex.xyz = lerp(from, to, transition_t);
				}
				else 
				{
					v.vertex.xyz = AnimTexVertexPos(v.vertex, v.vid, t);
				}
                
                vs2ps OUT;
                OUT.vertex = mul(UNITY_MATRIX_MVP, float4(v.vertex.xyz, 1));
                OUT.uv = v.texcoord;
                return OUT;
            }

            float4 frag(vs2ps IN) : COLOR {
				fixed4 leafMask_col = tex2D(_LeafMaskTex, IN.uv);
				fixed4 main_col = tex2D(_MainTex, IN.uv);

				return main_col;
            }
			ENDCG
		}

        /*Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            #pragma multi_compile_shadowcaster
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Assets/Packages/VertexAnimator/Shader/AnimTexture.cginc"

            struct vsin {
                uint vid: SV_VertexID;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct vs2ps {
                V2F_SHADOW_CASTER;
            };
            
            sampler2D _MainTex;
            float4 _Color;
            
            vs2ps vert(vsin v) {
                float t = _AnimTex_T;
                t = clamp(t, 0, _AnimTex_AnimEnd.x);
                v.vertex.xyz = AnimTexVertexPos(v.vertex, v.vid, t);
                
                vs2ps OUT;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(OUT);
                return OUT;
            }

            float4 frag(vs2ps IN) : COLOR {
                SHADOW_CASTER_FRAGMENT(IN);
            }
            ENDCG
        }*/
	}
	FallBack Off

}
