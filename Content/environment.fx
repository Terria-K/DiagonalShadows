sampler2D passedImage : register(s0);

float4 PS(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 frag = tex2D(passedImage, texCoord);
    return float4(float3(color.a * frag.rgb + 1 - color.a), 1.);
}


technique Light 
{
    pass LightPass
    {
        PixelShader = compile ps_3_0 PS();
    }
}