GameObject = clr.BaldejFramework.GameObject
Assets = clr.BaldejFramework.Assets
Components = clr.BaldejFramework.Components
Math = clr.OpenTK.Mathematics
cube = GameObject('cube')

function start()
		transform = Components.BoxRigidTransform3D(Math.Vector3(0, 0, 10), Math.Vector3(0, 0, 0), Math.Vector3(1, 1, 1), Math.Vector3(1, 1, 1))
		cube:AddComponent(transform)
		cube:AddComponent(Components.Mesh(Assets.ObjMeshAsset('models\\cube', 'cube', 'Cube', 0, 0)))
end

function update()
		print(transform.Position)
end

function unpausedUpdate()

end
