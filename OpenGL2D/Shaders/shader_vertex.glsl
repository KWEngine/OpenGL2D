#version 430
in		vec3 aPosition;
in		vec3 aNormal;
in		vec2 aTexture;
in		vec3 aNormalTangent;
in		vec3 aNormalBiTangent;

out		vec3 vNormal;
out		vec2 vTexture;
out		vec3 vPosition;

uniform mat4 uModelMatrix;
uniform mat4 uNormalMatrix;
uniform mat4 uMVP;

void main()
{
	vNormal = normalize(vec3(uNormalMatrix * vec4(aNormal, 0.0)));
	vTexture = aTexture;
	vPosition = (uModelMatrix * vec4(aPosition, 1.0)).xyz;

	gl_Position = uMVP * vec4(aPosition, 1.0);
}