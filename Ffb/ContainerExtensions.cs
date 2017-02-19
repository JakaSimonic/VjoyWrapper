using System.Collections.Generic;

namespace Ffb
{
    internal static class ContainerExtensions
    {
        public static IManageableEffect GetEffect(this List<IManageableEffect> effectsList, int effectBlockIndex)
        {
            return effectsList[effectBlockIndex - 1];
        }

        public static void InsertEffect(this List<IManageableEffect> effectsList, int effectBlockIndex, IManageableEffect effect)
        {
            effectsList[effectBlockIndex - 1] = effect;
        }

        public static int GetFirstFreeSlot(this List<IManageableEffect> effectsList)
        {
            int index = effectsList.FindIndex(x => x == null);
            if (index == -1)
            {
                effectsList.Add(null);
                return effectsList.Count;
            }
            else
            {
                return index + 1;
            }
        }
    }
}