Shader "Unlit/ScrollTexture"
{
    Properties
    {
        MainTex ("Texture", 2D) = "white" {}
        [ShowAsVector2] Scroll ("Water scroll", Vector) = (1, 1, 0, 0)
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True" }
        LOD 100

        Pass
        {
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            
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
                float4 worldPos : TEXCOORD1;
            };

            sampler2D MainTex;
            float4 MainTex_ST;

            float2 Scroll;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 projection = i.worldPos.xy / 300;
                //Scroll *= _Time.z;
                fixed4 col = tex2D(MainTex, projection + Scroll);

                fixed4 newCol = fixed4(col.r, col.g, col.b, col.a) * fixed4(0, 0, 0, 0.6f);
                return newCol;
            }
            ENDCG
        }
    }
}
