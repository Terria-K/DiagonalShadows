float4x4 projection;
float4x4 view;

float2 vs_pos;
static const float len = 100000.0;

struct Vertex 
{
    float4 Pos  : SV_POSITION;
    float4 Distance: COLOR;
};

Vertex VS(float4 inPosition : POSITION) 
{
    Vertex vert = (Vertex)0;
    float2 pos = inPosition.xy;
    float2 dis = pos - vs_pos;
    float scalarDist = length(dis);
    if (inPosition.z > 1.) {
        pos += dis/scalarDist * len;
        vert.Distance.r = inPosition.z - 2.;
        vert.Distance.g = 1.;
    } else {
        vert.Distance.g = scalarDist/len;
        vert.Distance.r = lerp(0.5, inPosition.z, vert.Distance.g);
    }
    float4 viewPos = mul(float4(pos.x, pos.y, 0., 1), view);
    float4 finalPos = mul(viewPos, projection); 
    finalPos.z = 0;
    vert.Pos = finalPos;
    return vert;
}

float4 PS(in Vertex vert) : COLOR0
{
    float str = (1. - abs(vert.Distance.r - 0.5) * 2 / vert.Distance.g) * 5.;
    return float4(str, str, str, str);
}


technique Light 
{
    pass ShadowPass
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}