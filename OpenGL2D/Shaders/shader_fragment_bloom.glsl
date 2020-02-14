#version 430

in		vec2 vTexture;

uniform sampler2D uTextureScene;
uniform sampler2D uTextureBloom;
uniform int uMerge;
uniform int uHorizontal;
uniform vec2 uResolution;

out		vec4 color;

float weight[5] = float[] (0.4, 0.2, 0.12, 0.07, 0.01);

float factor = 0.00000000001;
float multiplier = 1.0; // the bigger, the stronger the bloom

void main()
{
	ivec2 size = textureSize(uTextureBloom, 0);
	vec3 result = texture(uTextureBloom, vTexture).rgb * weight[0];
    
	if(uHorizontal > 0)
    {
		float bloomSizeFactor = (size.x * size.y) * factor;
        for(int i = 1; i < 5; i++)
        {
            result += texture(uTextureBloom, vTexture + vec2((float(i) * (bloomSizeFactor * float(i))), 0.0)).rgb * (weight[i] * multiplier);
            result += texture(uTextureBloom, vTexture - vec2((float(i) * (bloomSizeFactor * float(i))), 0.0)).rgb * (weight[i] * multiplier);
        }
		color.x = result.x;
		color.y = result.y;
		color.z = result.z;
		color.w = 1.0;
	}
    else
    {
		float bloomSizeFactor = (size.x * size.y) * factor * float(size.y) / float(size.x);
        for(int i = 1; i < 5; i++)
        {
            result += texture(uTextureBloom, vTexture + vec2(0.0, (float(i) * (bloomSizeFactor * float(i))))).rgb * (weight[i] * multiplier);
            result += texture(uTextureBloom, vTexture - vec2(0.0, (float(i) * (bloomSizeFactor * float(i))))).rgb * (weight[i] * multiplier);
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