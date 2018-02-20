using System.Collections.Generic;
using System.Text;
using fruiton.kernel;
using fruiton.kernel.abilities;
using fruiton.kernel.actions;
using fruiton.kernel.effects;

public static class TooltipUtil
{
    public static string GenerateTooltip(Fruiton kernelFruiton)
    {
        var fruitonInfo = new StringBuilder("<b>" + kernelFruiton.name + "</b>\n");

        fruitonInfo.Append("\n<b>Movement</b>\n");
        foreach (MoveGenerator moveGenerator in kernelFruiton.moveGenerators.ToList())
        {
            fruitonInfo.Append(moveGenerator);
        }

        fruitonInfo.Append("\n<b>Attack</b>\n");
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
        if (effects.Count > 0)
        {
            fruitonInfo.Append("\n<b>Effects</b>\n");
            foreach (Effect effect in effects)
            {
                fruitonInfo.Append(effect.getDescription()).Append("\n");
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
