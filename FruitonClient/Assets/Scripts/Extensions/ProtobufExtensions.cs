using Cz.Cuni.Mff.Fruiton.Dto;

public static class ProtobufExtensions
{
    public static string GetReadableName(this Fraction fraction)
    {
        switch (fraction)
        {
            case Fraction.CranberryCrusade:
                return "Cranberry Crusade";
            case Fraction.TzatzikiTsardom:
                return "Tzatziki Tsardom";
            case Fraction.GuacamoleGuerillas:
                return "Guacamole Guerrillas";
        }
        return fraction.ToString();
    }
}