using Cz.Cuni.Mff.Fruiton.Dto;

namespace Extensions
{
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

        public static string GetDescription(this Status status)
        {
            switch (status)
            {
                case Status.Offline: return "Offline";
                case Status.InBattle: return "In Battle";
                case Status.InMatchmaking: return "Looking for an opponent";
                case Status.MainMenu: return "Chilling in menu";
            }
            return status.ToString();
        }
    }
}