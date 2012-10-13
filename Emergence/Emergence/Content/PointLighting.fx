#define NUM_LIGHTS 5

float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;

float3 lightPoses[NUM_LIGHTS];
float4 lightColours[NUM_LIGHTS];
int lightRadii[NUM_LIGHTS];
float4 Amb = float4(1,1,1,0.4);

texture Tex;
sampler2D texSampler = sampler_state{

	Texture = (Tex);
	MagFilter = Linear;
	MinFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;

};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;

};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Normal : TEXCOORD1;
    float3 Pos3D : TEXCOORD2;

};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord = input.TexCoord;

	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose));
	output.Pos3D = worldPosition;

    return output;
}

float4 FullPixelShaderFunction(VertexShaderOutput input) : COLOR0
{

	float4 texColour = tex2D(texSampler, input.TexCoord);
    texColour.a = 1;

	float4 allLightColour = float4(0,0,0,0);
	for(int i = 0; i < NUM_LIGHTS; ++i){
	
	//Diffuse
	float3 LightDir = normalize(input.Pos3D - lightPoses[i]);
	float NdL = max(0, dot(input.Normal, -LightDir));
	float4 Color = saturate(lightColours[i] * NdL);
	
	float atten = saturate((lightRadii[i] * lightRadii[i])/dot(lightPoses[i] - input.Pos3D, lightPoses[i] - input.Pos3D));
	allLightColour += Color * atten;
	
	}

	return texColour * (Amb +allLightColour);
   
}

float4 TexPixelShaderFunction(VertexShaderOutput input) : COLOR0
{

    float4 texColour = tex2D(texSampler, input.TexCoord);
    texColour.a = 1;

	return texColour;
   
}

technique Lighting
{
    pass Lighting
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 FullPixelShaderFunction();
    }
    
}

technique Texturing{

	pass Texturing{
    
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 TexPixelShaderFunction(); 
    
    }

}