#version 330

in vec3 vertPosition;
layout(location = 1) in vec2 vertTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 texCoord;

void main(void)
{
    gl_Position = vec4(vertPosition, 1) * model * view * projection;
	texCoord = vertTexCoord;
}
