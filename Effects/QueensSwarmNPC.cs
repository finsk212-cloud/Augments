using Terraria.ModLoader;

namespace Augments
{
    public class QueensSwarmNPC : GlobalNPC
    {
        public const int StacksRequired = 5;

        public override bool InstancePerEntity => true;

        private int swarmStacks;

        // Returns the stack count reached by this hit. Hitting the threshold
        // immediately consumes all stacks so the next hit begins at one.
        public int AddStack()
        {
            swarmStacks++;
            int reachedStacks = swarmStacks;

            if (swarmStacks >= StacksRequired)
                swarmStacks = 0;

            return reachedStacks;
        }
    }
}
