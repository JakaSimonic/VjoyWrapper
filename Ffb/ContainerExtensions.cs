using System.Collections.Generic;

namespace Ffb
{
    internal static class ContainerExtensions
    {
        public static IEffect GetEffect(this List<IEffect> effectsList, int effectBlockIndex)
        {
            return effectsList[effectBlockIndex - 1];
        }

        public static void InsertEffect(this List<IEffect> effectsList, int effectBlockIndex, IEffect effect)
        {
            effectsList[effectBlockIndex - 1] = effect;
        }

        public static int GetFirstFreeSlot(this List<IEffect> effectsList)
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