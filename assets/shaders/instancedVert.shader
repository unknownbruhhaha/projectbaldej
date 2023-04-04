#version 330

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
uniform mat4 modelMatrix[120];
out vec2 texCoord;
out vec3 Normal;
out vec3 FragPos;
uniform mat4 view;
uniform mat4 projection;

void main(void)
{
    gl_Position = vec4(aPosition, 1.0) * modelMatrix[gl_InstanceID - 1] * view * projection;
    texCoord = aTexCoord;

    FragPos = vec3(vec4(aPosition, 1.0) * modelMatrix[gl_InstanceID - 1]);
    Normal = aNormal * mat3(transpose(inverse(modelMatrix[gl_InstanceID - 1])));
}