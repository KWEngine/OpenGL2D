#version 430

in		vec2 vTexture;

uniform sampler2D uTextureScene;
uniform sampler2D uTextureBloom;
uniform int uMerge;
uniform int uHorizontal;
uniform vec2 uResolution;

out		vec4 color;

float weight[11] = float[] (0.4, 0.2, 0.10, 0.075, 0.05, 0.03, 0.01 , 0.0075, 0.005, 0.0025, 0.001 );
vec2 tex_offset = vec2(1.0, 1.0);


float bloomSizeFactor = 0.00075; // the bigger, the wider the bloom
float multiplier = 1.0; // the bigger, the stronger the bloom

void main()
{
	ivec2 size = textureSize(uTextureBloom, 0);
	vec3 result = texture(uTextureBloom, vTexture).rgb * weight[0];
    
	if(uHorizontal > 0)
    {
	
        for(int i = 1; i < 11; i++)
        {
            result += texture(uTextureBloom, vTexture + vec2(tex_offset.x * (float(i) * (bloomSizeFactor * float(i))), 0.0)).rgb * (weight[i] * multiplier);
            result += texture(uTextureBloom, vTexture - vec2(tex_offset.x * (float(i) * (bloomSizeFactor * float(i))), 0.0)).rgb * (weight[i] * multiplier);
        }
		color.x = result.x;
		color.y = result.y;
		color.z = result.z;
		color.w = 1.0;
	}
    else
    {
	
        for(int i = 1; i < 11; i++)
        {
            result += texture(uTextureBloom, vTexture + vec2(0.0, tex_offset.y * (float(i) * (bloomSizeFactor * float(i))))).rgb * (weight[i] * multiplier);
            result += texture(uTextureBloom, vTexture - vec2(0.0, tex_offset.y * (float(i) * (bloomSizeFactor * float(i))))).rgb * (weight[i] * multiplier);
        }	

		if(uMerge > 0){
			result += texture(uTextureScene, vTexture).rgb;
		}

		color.x = result.x;
		color.y = result.y;
		color.z = result.z;
		color.w = 1.0;
	}
}