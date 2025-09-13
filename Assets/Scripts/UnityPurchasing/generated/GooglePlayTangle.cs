// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("U9LQA+FKr220EsZk+e+bqUCVcNcVw88hJN2fm05KrF1ptAaq8ag7DiBK1YJ1q2NuYU39Lx/x9ChiOk9DYHcpBwr/1M8iA12xkjXoyB0t3Q9Ed97bw6/7768aVOJizuohCC+U8KBDrBpsnVFjM2IOwDdWuJ0+Vdp+b+zi7d1v7Ofvb+zs7Xu86x++Jw+auQdv4ID89Bv1ILCa0Ezytw2Q504paE9rHAm4zQHTlXvF5mIaaAUl1FcnPz3CBlOxSfWk65leZXmoyYy/OjQLmsexTq1pSMwm0PS8MVj/8t1v7M/d4Ovkx2ulaxrg7Ozs6O3u9RX1qNdEjZACY2EWqgK/tS12YaS9LdNl6PcTifd61UlFXjCeBSf38kFuWuvofKDS1O/u7O3s");
        private static int[] order = new int[] { 2,13,12,6,8,7,13,9,13,13,13,12,12,13,14 };
        private static int key = 237;

        public static readonly bool IsPopulated = false;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
