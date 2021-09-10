Shader "Custom/DepthSample"
{
    Properties
    {
        _MainTex      ("Texture", 2D) = "white" {}
        _FillCol      ("Fill Color", Color) = (1,1,1,1)
        _VoidCol      ("Void Color", Color) = (0,0,0,0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            fixed4 _FillColor;
            fixed4 _VoidColor;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,1);

                
                float depth = UNITY_SAMPLE_DEPTH(tex2D(_MainTex, i.uv));
                
                depth = 1.0f - Linear01Depth(depth);

                //return fixed4(depth, 0, 0, depth);

                
                //col = lerp(sampleSecond, sampleMain, depth);
                col = lerp(_VoidColor, _FillColor, depth);
                
                return col;
            }
            ENDCG
        }
    }
}
