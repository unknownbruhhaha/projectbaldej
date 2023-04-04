using CSCore;
using CSCore.Codecs;
using CSCore.Utils;
using CSCore.XAudio2;
using CSCore.XAudio2.X3DAudio;

namespace BaldejFramework
{
    static class AudioManager
    {
        public static Listener listener = new Listener()
        {
            Position = new Vector3(0, 0, 0),
            OrientFront = new Vector3(0, 0, 1),
            OrientTop = new Vector3(0, 1, 0),
            Velocity = new Vector3(0, 0, 0)
        };
    }
}
