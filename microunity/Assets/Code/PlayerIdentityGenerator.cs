using UnityEngine;

class PlayerIdentityGenerator
{
    static string[] namePt1 = new string[]
    {
        "Amazing",
        "Brilliant",
        "Curious",
        "Dangerous",
        "Eager",
        "Fragile",
        "Gentle",
        "Hungry",
        "Iridescent",
        "Jolly",
        "Killer",
        "Luminous",
        "Monsterous",
        "Notorious",
        "Opulent",
        "Perfect",
        "Quick",
        "Radioactive",
        "Strange",
        "Troubled",
        "Unusual",
        "Vain",
        "Walloping",
        "Xylotomous",
        "Youthful",
        "Zany"
    };

    static string[] namePt2 = new string[]
    {
        "Armadillo",
        "Beaver",
        "Cat",
        "Dog",
        "Emu",
        "Frog",
        "Goat",
        "Horse",
        "Iguana",
        "Jellyfish",
        "Kiwi",
        "Lion",
        "Monkey",
        "Newt",
        "Octopus",
        "Parrot",
        "Quail",
        "Rabbit",
        "Sloth",
        "Tiger",
        "Urubu",
        "Vulture",
        "Walrus",
        "Xerus",
        "Yak",
        "Zebra"
    };

    static Color[] colors = new Color[]
    {
        Color.red,
        Color.green,
        Color.cyan,
        Color.blue,
        Color.magenta,
        Color.yellow
    };

    public static Color PlayerIDToColor(long id)
    {
        return colors[Mathf.Abs(id.GetHashCode()) % colors.Length];
    }

    public static string PlayerIDToName(long id)
    {
        long id1 = Mathf.Abs((id ^ 1).GetHashCode()) % namePt1.Length;
        long id2 = Mathf.Abs((id ^ 2).GetHashCode()) % namePt2.Length;

        return string.Format("{0} {1}", namePt1[id1], namePt2[id2]);
    }

    public static string PlayerIDToColorNameString(long id)
    {
        return string.Format("<color=#{0}>{1}</color>", PlayerIDToColor(id).ToHexStringRGBA(), PlayerIDToName(id));
    }
}