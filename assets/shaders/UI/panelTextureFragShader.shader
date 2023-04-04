#version 330

out vec4 outputColor;

in vec2 texCoord;

in float R;
in float G;
in float B;
in float A;
uniform sampler2D texture0;

void main()
{
	outputColor = texture(texture0, texCoord);
}
