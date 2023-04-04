#version 330

out vec4 outputColor;

in float R;
in float G;
in float B;
in float A;

void main()
{
    outputColor = vec4(R, G, B, A); //white color
}
