#version 430

// this is just the texture coordinates that are passed through
// from the vertex shader:
in		vec2 vTexture; 

// this is the pixel's position in 
// the 3d / 2d WORLD (NOT in relation to the screen!):
in		vec3 vPosition; 

// this is the normal of the current face after rotation.
// you could use it to calculate lighting intensity if you passed
// another vec3 uniform with the light's position.
in		vec3 vNormal; 

// this is the texture uniform that you upload to the shader:
uniform sampler2D uTexture;

// the output variable (rgba)
out vec4 color;

void main()
{
	// texture() determines which part of the texture must
	// be painted on the current pixel:
	color = texture(uTexture, vTexture);
}