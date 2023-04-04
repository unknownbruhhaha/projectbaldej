namespace BaldejFramework
{
    public class Scene
    {
        public List<GameObject> gameObjects = new List<GameObject>();

        public GameObject FindByName(string name)
        {
            foreach (GameObject obj in gameObjects)
            {
                if (obj.Name == name)
                    return obj;
            }

            return gameObjects[0];
        }
    }
}
