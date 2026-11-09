namespace RaceDay.Web.Services;

public static class FormatHelper
{
    // 1 -> "1st", 2 -> "2nd", 3 -> "3rd", 11 -> "11th", 47 -> "47th"
    public static string Ordinal(int number)
    {
        int lastTwoDigits = number % 100;

        // 11th, 12th and 13th are special cases
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return number + "th";
        }

        return (number % 10) switch
        {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th"
        };
    }
}