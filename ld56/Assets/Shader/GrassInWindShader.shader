Shader "Custom/GradientWindEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WindSpeed ("Wind Speed", Float) = 1.0
        _WindStrength ("Wind Strength", Float) = 0.05
        _Frequency ("Wave Frequency", Float) = 2.0
        _Color ("Color", Color) = (1,1,1,1)  // Farb-Property
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            // Transparenz-Blending aktivieren
            Blend SrcAlpha OneMinusSrcAlpha

            // ZWrite deaktivieren, um Tiefenprobleme bei transparenten Objekten zu vermeiden
            ZWrite Off

            HLSLPROGRAM
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
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WindSpeed;
            float _WindStrength;
            float _Frequency;
            float4 _Color;  // Farb-Variable

            // Vertex Shader
            v2f vert(appdata v)
            {
                v2f o;

                // Verwende die eingebaute Unity-Zeit mit '_Time'
                float timeValue = _Time.y;  // _Time.y ist die Zeit in Sekunden

                // Berechne die relative Y-Position des Vertex innerhalb des Sprites
                float heightFactor = saturate((v.vertex.y + 1.0) * 0.5); // Von 0 unten bis 1 oben

                // Berechne den Windeffekt und skaliere ihn mit der relativen Höhe
                float wave = sin(v.vertex.y * _Frequency + timeValue * _WindSpeed) * _WindStrength * heightFactor;

                // Modifiziere die X-Position des Vertex basierend auf dem Windeffekt
                v.vertex.x += wave;

                // Transformiere die Vertex-Position in den Clip-Raum
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            // Fragment-Shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample die Textur
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Multipliziere die Texturfarbe mit der angegebenen Farbe
                fixed4 finalColor = texColor * _Color;

                // Gib die finale Farbe zurück
                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Transparent/Cutout/VertexLit"
}
