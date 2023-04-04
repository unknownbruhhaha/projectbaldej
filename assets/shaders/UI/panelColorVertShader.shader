#version 330

in vec3 vertPosition;

uniform float r;
uniform float g;
uniform float b;
uniform float a;
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out float R;
out float G;
out float B;
out float A;

void main(void)
{
    gl_Position = vec4(vertPosition, 1) * model * view * projection;
	R = r;
	G = g;
	B = b;
	A = a;
}
