sampler2D passedImage : register(s0);

#define PI 3.1415926538

struct Vertex 
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4x4 projection;
float4x4 view;

Vertex VS(float4 inPosition : POSITION, float4 color : COLOR0, float2 inTexCoord : TEXCOORD0) 
{
    Vertex vert = (Vertex)0;
    float4 viewPos = mul(float4(inPosition.x, inPosition.y, 0., 1), view);
    float4 finalPos = mul(viewPos, projection); 
    vert.Pos = finalPos;
    vert.Color = color;
    vert.TexCoord = inTexCoord;
    return vert;
}

float dir;
float fov;

float2 ps_pos;
float ps_str;
float size;

float mod(float x, float y)
{
  return x - y * floor(x/y);
}

float4 PS(in Vertex vert) : COLOR0
{
    float2 dis = vert.Pos.xy - ps_pos;
    float str = 1./(sqrt(dis.x * dis.x + dis.y * dis.y + size * size) - size + 1. - ps_str);

    float rdir = radians(dir);
    float hfov = radians(fov) * 0.5f;

    if (hfov < PI) 
    {
        float rad = atan2(-dis.y, dis.x);
        float andist = abs(mod(rad + 2. * PI, 2. * PI) - rdir);
        andist = min(andist, 2. * PI - andist);
        str = clamp((1.0 - andist/hfov) * 3.0, 0., 1.);
    }
    float4 frag = tex2D(passedImage, vert.TexCoord);
    return vert.Color * float4(str, str, str, 1.0) * frag;
}


technique Light 
{
    pass LightPass
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}