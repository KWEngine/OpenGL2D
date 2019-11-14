#version 430
in		vec3 aPosition;
in		vec3 aNormal;
in		vec2 aTexture;
in		vec3 aNormalTangent;
in		vec3 aNormalBiTangent;

out		vec3 normalenvektor;
out		vec2 texturkoordinate;
out		vec3 pixelposition;

uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform mat4 uMVP;

void main()
{
	normalenvektor = normalize(vec3(uNormalMatrix * vec4(aNormal, 0.0)));
	texturkoordinate = aTexture;
	pixelposition = (uModelMatrix * vec4(aPosition, 1.0)).xyz;

	gl_Position = uMVP * vec4(aPosition, 1.0);
}