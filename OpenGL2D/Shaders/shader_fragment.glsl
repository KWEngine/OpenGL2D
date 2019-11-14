#version 430

in		vec2 texturkoordinate;
in		vec3 pixelposition;
in		vec3 normalenvektor;
uniform sampler2D uTexture;

out vec4 pixelfarbe;

void main()
{
	pixelfarbe = texture(uTexture, texturkoordinate);
}