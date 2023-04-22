GameObject = clr.BaldejFramework.GameObject
Assets = clr.BaldejFramework.Assets
Components = clr.BaldejFramework.Components
Math = clr.OpenTK.Mathematics
Input = clr.BaldejFramework.Input
Keys = clr.OpenTK.Windowing.GraphicsLibraryFramework.Keys
Render = clr.BaldejFramework.Render.Render

cube = GameObject('cube')
ground = GameObject('ground')

function start()
		Render.VSync = true 
		Render.camera.Position = Math.Vector3(0, 5, 0)
		transform = Components.BoxRigidTransform3D(Math.Vector3(0, 10, 7), Math.Vector3(0, 0, 0), Math.Vector3(1, 1, 1), Math.Vector3(2, 2, 2), 1)
		cube:AddComponent(transform)
		cube:AddComponent(Components.Mesh(Assets.ObjMeshAsset('models\\cube', 'cube', 'Cube', 0, 0), Assets.TextureAsset('textures\\Palette.png')))

		transform1 = Components.BoxStaticTransform3D(Math.Vector3(0, 0, 20), Math.Vector3(0, 0, 0), Math.Vector3(500, 1, 500), Math.Vector3(750, 2, 750))
		ground:AddComponent(transform1)
		ground:AddComponent(Components.Mesh(Assets.ObjMeshAsset('models\\cube', 'cube', 'Cube', 0, 0)))
		print('abc')
end

function update()
		if Input.IsKeyPressed(Keys.P) then
				transform.Position = Math.Vector3(0, 0, 7)
		end
		if Input.IsKeyPressed(Keys.R) then
				transform.Rotation = Math.Vector3(0, 0, 0)
		end
		if Input.IsKeyPressed(Keys.M) then
				transform.Position = Math.Vector3(9, 50, 7)
				transform.Rotation = Math.Vector3(0, 45, 0)
		end
end

function unpausedUpdate()

end
