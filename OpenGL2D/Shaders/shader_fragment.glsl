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

// tells the shader which color needs to go to the bloom texture:
uniform vec4 uBloom;

// the output variables (rgba)
out vec4 color;
out vec4 bloom;

void main()
{
	// texture() determines which part of the texture must
	// be painted on the current pixel:
	color = texture(uTexture, vTexture);

	// draw the bloom color to the second texture output:
	bloom = vec4(uBloom.xyz, color.w * uBloom.w);
}