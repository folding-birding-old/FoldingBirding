Shader "Custom/NoFogForUnlit"
{
    Properties
    {
        _TopSkyColor("Top Sky Color", Color) = (0.2, 0.4, 1, 1)
        _MiddleSkyColor("Middle Sky Color", Color) = (0.5, 1.0, 1.0, 1)
        _HorizonColor("Horizon Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fixed4 _TopSkyColor;
            fixed4 _MiddleSkyColor;
            fixed4 _HorizonColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float height = saturate((i.worldPos.y + 10.0) / 20.0);
                float t1 = saturate(height * 2.0);
                float t2 = saturate((height - 0.5) * 2.0);
                float3 color = lerp(_HorizonColor.rgb, _MiddleSkyColor.rgb, t1);
                color = lerp(color, _TopSkyColor.rgb, t2);
                return float4(color, 1.0);
            }
            ENDCG
        }
    }

    FallBack Off
}
