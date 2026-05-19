Shader "Custom/Overlay"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        // This is the magic line:
        ZTest Always 
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Color [_Color]
        }
    }
}