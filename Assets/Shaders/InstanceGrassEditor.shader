Shader "Grass/InstanceGrassEditor"
{
    Properties
    {
        [HideInInspector]_LowColor("Low color", Color) = (1,1,1,1)
        [HideInInspector]_HighColor("High color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _ClippingCoef("Clipping coef", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Alphatest"}
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
            };
            
            half _ClippingCoef, _AtlasID;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _LowColor, _HighColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                half uvMultipler = 1-step(0, _AtlasID)*0.5;
                half uvAdder = step(2,_AtlasID);
                o.uv.x *= uvMultipler;
                o.uv.x += uvAdder*0.5;

                half heightMask = step(0.5, o.uv.y);
                o.color = _HighColor*heightMask + _LowColor * (1-heightMask);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                col.rgb *= i.color.rgb;
                clip(col.a-_ClippingCoef);
                return col;
            }
            ENDCG
        }
    }
}