using UnityEngine;

namespace Android
{
    public static class Vibrator
    {

        private static readonly AndroidJavaObject VIBRATOR =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer")// Get the Unity Player.
                .GetStatic<AndroidJavaObject>("currentActivity")// Get the Current Activity from the Unity Player.
                .Call<AndroidJavaObject>("getSystemService", "vibrator");// Then get the Vibration Service from the Current Activity.

        static Vibrator()
        {
            // Trick Unity into giving the App vibration permission when it builds.
            // This check will always be false, but the compiler doesn't know that.
#if UNITY_ANDROID
            if (Application.isEditor) Handheld.Vibrate();
#endif
        }

        public static void Vibrate(long milliseconds)
        {
            VIBRATOR.Call("vibrate", milliseconds);
        }

        public static void Vibrate(long[] pattern, int repeat)
        {
            VIBRATOR.Call("vibrate", pattern, repeat);
        }
    }
}
