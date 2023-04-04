namespace BaldejFramework.Render
{
    public class AnimationData
    {
        public int AnimationStartFrame;
        public int AnimationEndFrame;
        public string AnimationName;

        public AnimationData(string name, int startFrame, int endFrame)
        {
            AnimationStartFrame = startFrame;
            AnimationEndFrame = endFrame;
            AnimationName = name;
        }

        public static List<AnimationData> NoAnimation()
        {
            AnimationData data = new AnimationData("Idle", 0, 1);
            List<AnimationData> list = new List<AnimationData>();
            list.Add(data);
            return list;
        }
    }
}
