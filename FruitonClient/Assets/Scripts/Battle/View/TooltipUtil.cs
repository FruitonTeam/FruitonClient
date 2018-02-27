using System.Collections.Generic;
using System.Text;
using Extensions;
using fruiton.kernel;
using fruiton.kernel.abilities;
using fruiton.kernel.actions;
using fruiton.kernel.effects;

namespace Battle.View
{
    public static class TooltipUtil
    {
        public static string GenerateTooltip(Fruiton kernelFruiton)
        {
            var fruitonInfo = new StringBuilder("<b>" + kernelFruiton.name.ToUpper() + "</b>\n");

            fruitonInfo.Append("<b>Movement range: </b>");
            foreach (MoveGenerator moveGenerator in kernelFruiton.moveGenerators.ToList())
            {
                fruitonInfo.Append(moveGenerator);
            }

            fruitonInfo.Append("\n<b>Attack range: </b>");
            foreach (AttackGenerator attackGenerator in kernelFruiton.attackGenerators.ToList())
            {
                fruitonInfo.Append(attackGenerator);
            }

            List<object> abilities = kernelFruiton.abilities.ToList();
            if (abilities.Count > 0)
            {
                fruitonInfo.Append("\n<b>Abilities</b>\n");
                foreach (Ability ability in abilities)
                {
                    fruitonInfo.Append(string.Format(ability.text, kernelFruiton.currentAttributes.heal));
                }
            }

            List<object> effects = kernelFruiton.effects.ToList();
            int decayCount = 0;
            if (effects.Count > 0)
            {
                fruitonInfo.Append("\n<b>Effects</b>\n");
                foreach (Effect effect in effects)
                {
                    if (effect.GetType() == typeof(DecayEffect))
                    {
                        decayCount++;
                    }
                    else
                    {
                        fruitonInfo.Append(effect.getDescription()).Append("\n");
                    }
                }
                if (decayCount > 0)
                {
                    fruitonInfo.Append("Decay");
                    if (decayCount > 1)
                    {
                        fruitonInfo.Append(" (").Append(decayCount).Append("x)");
                    }
                    fruitonInfo.Append("\n");
                }
            }

            foreach (int immunity in kernelFruiton.currentAttributes.immunities.ToList())
            {
                if (immunity == HealAction.ID) fruitonInfo.Append("Can't be healed.\n");
                else if (immunity == AttackAction.ID) fruitonInfo.Append("Can't be attacked.\n");
            }

            return fruitonInfo.ToString();
        }
    }
}
