Shader "Unlit/InstanceGrass"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ClippingCoef("Clipping coef", Range(0,1)) = 0.5
        [HideInInspector]_GrassWindFreq("Wind frequency", Float) = 1
        [HideInInspector]_GrassWindAmp("Wind amplitude", Float) = 0
        [HideInInspector]_AtlasVariant("Atlassing variance", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Alphatest"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct VisibleGrassSample
            {
                float4x4 Matrix;
                float4 LowColor;
                float4 HighColor;
                int AtlasID;
            };

            StructuredBuffer<VisibleGrassSample>_visibleGrassSamples;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _ClippingCoef)
                UNITY_DEFINE_INSTANCED_PROP(float, _GrassWindFreq)
                UNITY_DEFINE_INSTANCED_PROP(float, _GrassWindAmp)
                UNITY_DEFINE_INSTANCED_PROP(half4, _ambientColor)
                UNITY_DEFINE_INSTANCED_PROP(half4, _DirLightColor)
                UNITY_DEFINE_INSTANCED_PROP(half4, _DirLightDirection)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                half4 lowColor = _visibleGrassSamples[instanceID].LowColor;
                half4 highColor = _visibleGrassSamples[instanceID].HighColor;
                float2 idleWindDir = float2(lowColor.a,highColor.a);
                half atlasID = _visibleGrassSamples[instanceID].AtlasID;
                half uvMultipler = 1-step(0, atlasID)*0.5;
                half uvAdder = step(2,atlasID);
                o.uv = v.uv;
                o.uv.x *= uvMultipler;
                o.uv.x += uvAdder*0.5;
                half heightMask = step(0.5, o.uv.y);
                
                v.normal.xz += sin((_Time.y-1800)*
                    UNITY_ACCESS_INSTANCED_PROP(Props, _GrassWindFreq))*idleWindDir*
                        UNITY_ACCESS_INSTANCED_PROP(Props, _GrassWindAmp)*heightMask;
                v.normal = normalize(v.normal);
                float4 normal4 = float4(v.normal, 0.0);
                o.normal = normalize(mul(normal4, unity_WorldToObject).xyz);
                
                float4x4 objToWorldMatrix = _visibleGrassSamples[instanceID].Matrix;
                v.vertex.xz += sin((_Time.y-1800)*
                    UNITY_ACCESS_INSTANCED_PROP(Props, _GrassWindFreq))*idleWindDir*
                        UNITY_ACCESS_INSTANCED_PROP(Props, _GrassWindAmp)*heightMask;
                float4 worldPos = mul(objToWorldMatrix, v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                o.color = highColor*heightMask + lowColor * (1-heightMask);
                o.color *= _ambientColor;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                half4 col = tex2D(_MainTex, i.uv);
                
                half diffuseLightCoef = max(
                    dot(UNITY_ACCESS_INSTANCED_PROP(Props, _DirLightDirection).xyz*(-1), i.normal),0.0
                    );
                half3 diffColor = UNITY_ACCESS_INSTANCED_PROP(Props, _DirLightColor)*diffuseLightCoef;
                half3 ambientColor = i.color.rgb;
                col.rgb *=diffColor*ambientColor;
                clip(col.a-UNITY_ACCESS_INSTANCED_PROP(Props, _ClippingCoef));
                return col;
            }
            ENDCG
        }
    }
}