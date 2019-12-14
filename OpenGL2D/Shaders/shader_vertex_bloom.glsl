#version 430
in		vec3 aPosition;
in		vec2 aTexture;

out		vec2 vTexture;

uniform mat4 uMVP;

void main()
{
	vTexture = aTexture;
	
	gl_Position = uMVP * vec4(aPosition, 1.0);
}