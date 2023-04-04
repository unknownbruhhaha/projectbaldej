using BaldejFramework;
using OpenTK.Windowing.GraphicsLibraryFramework;
namespace ProjectBaldej
{
		public class Game 
		{
				public void Start()
				{
						
				}
				
				public void Update()
				{

				}
				
				public void UnpausedUpdate()
				{
						if (Input.IsKeyPressed(Keys.Escape))
						{
								Console.WriteLine("esc");
						}
				}
		}
}
