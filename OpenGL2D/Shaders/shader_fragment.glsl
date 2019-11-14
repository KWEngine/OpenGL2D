#version 430

in		vec2 vTexture;
in		vec3 vPosition;
in		vec3 vNormal;
uniform sampler2D uTexture;

out vec4 color;

void main()
{
	color = texture(uTexture, vTexture);
}