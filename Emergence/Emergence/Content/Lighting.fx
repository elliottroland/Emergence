float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;

texture Tex;
sampler2D texSampler = sampler_state{

	Texture = (Tex);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;

};

// TODO: add effect parameters here.
float4 Ambient = float4(1,0,0,1);
float AmbIntensity = 0.3;
float3 DifDir = float3(1, 0, 0);
float4 Diffuse = float4(0.4f,0.4f,0.4f,1);
float DifIntensity = 1;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color: COLOR0;
    float2 TexCoord : TEXCOORD1;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // TODO: add your vertex shader code here.
    float4 normal = mul(input.Normal, WorldInverseTranspose);
    float lightIntensity = dot(normal, DifDir);
    output.Color = saturate(Diffuse * DifIntensity * lightIntensity);

	output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.
    float4 texColour = tex2D(texSampler, input.TexCoord);
    texColour.a = 1;
    
    float4 lightColour = input.Color + Ambient * AmbIntensity;

    return saturate(texColour * lightColour);
}

technique Technique1
{
    pass Lighting
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
