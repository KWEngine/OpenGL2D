#version 430
// this is from the VBOs:
in		vec3 aPosition;
in		vec3 aNormal;
in		vec2 aTexture;

// the out-parameters of the vertex shader
// are the fragment shader's input-parameters:
out		vec3 vNormal;
out		vec2 vTexture;
out		vec3 vPosition;

// these are uniform matrices that were sent to
// the shader in the Draw()-method:
uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform mat4 uMVP;

void main()
{
	// rotate the vertex' normal according to the current model matrix:
	vNormal = vec3(uNormalMatrix * vec4(aNormal, 0.0));

	// just give the texture coordinates to the fragment shader
	vTexture = aTexture;
	vPosition = (uModelMatrix * vec4(aPosition, 1.0)).xyz;

	gl_Position = uMVP * vec4(aPosition, 1.0);
}