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

        fruitonInfo.Append("\n<b>Abilities</b>\n");
        foreach (Ability ability in kernelFruiton.abilities.ToList())
        {
            fruitonInfo.Append(string.Format(ability.text, kernelFruiton.currentAttributes.heal));
        }

        fruitonInfo.Append("\n<b>Effects</b>\n");
        foreach (Effect effect in kernelFruiton.effects.ToList())
        {
            fruitonInfo.Append(effect.text + "\n");
        }

        foreach (int immunity in kernelFruiton.currentAttributes.immunities.ToList())
        {
            if (immunity == HealAction.ID) fruitonInfo.Append("Can't be healed.\n");
            else if (immunity == AttackAction.ID) fruitonInfo.Append("Can't be attacked.\n");
        }

        return fruitonInfo.ToString();
    }
}
