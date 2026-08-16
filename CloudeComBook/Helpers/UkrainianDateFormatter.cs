namespace CloudeComBook.API.Helpers;

public static class UkrainianDateFormatter
{
    public static string GetDateInWords(DateTime date)
    {
        string[] months = {
            "січня", "лютого", "березня", "квітня", "травня", "червня",
            "липня", "серпня", "вересня", "жовтня", "листопада", "грудня"
        };

        string[] ones = { "", "першого", "другого", "третього", "четвертого", "п'ятого", "шостого", "сьомого", "восьмого", "дев'ятого",
            "десятого", "одинадцятого", "дванадцятого", "тринадцятого", "чотирнадцятого", "п'ятнадцятого", "шістнадцятого",
            "сімнадцятого", "вісімнадцятого", "дев'ятнадцятого" };

        string[] tens = { "", "десять", "двадцять", "тридцять", "сорок", "п'ятдесят", "шістдесят", "сімдесят", "вісімдесят", "дев'яносто" };
        string[] teens = { "", "десятого", "двадцятого", "тридцятого", "сорокового", "п'ятдесятого", "шістдесятого", "сімдесятого", "вісімдесятого", "дев'яностого" };
        string[] hundreds = { "", "сто", "двісті", "триста", "чотириста", "п'ятсот", "шістсот", "сімсот", "вісімсот", "дев'ятсот" };
        string[] thousands = { "", "тисяча", "дві тисячі", "три тисячі", "чотири тисячі", "п'ять тисяч", "шість тисяч", "сім тисяч", "вісім тисяч", "дев'ять тисяч" };

        string yearWords = ConvertNumberToWords(date.Year, ones, tens, teens, hundreds, thousands);
        string dayWords = ConvertDayToWords(date.Day, ones, tens, teens);
        string monthWords = months[date.Month - 1];

        return $"{dayWords} {monthWords} {yearWords} року";
    }

    private static string ConvertDayToWords(int day, string[] ones, string[] tens, string[] teens)
    {
        if (day < 20)
            return ones[day];

        int ten = day / 10;
        int one = day % 10;

        return one == 0 ? teens[ten] : $"{tens[ten]} {ones[one]}";
    }

    private static string ConvertNumberToWords(int number, string[] ones, string[] tens, string[] teens, string[] hundreds, string[] thousands)
    {
        string words = "";

        int thousand = number / 1000;
        number %= 1000;

        int hundred = number / 100;
        number %= 100;

        int ten = number / 10;
        int one = number % 10;

        if (thousand > 0) words += thousands[thousand] + " ";
        if (hundred > 0) words += hundreds[hundred] + " ";
        if (ten > 1 && one > 0) words += tens[ten] + " " + ones[one];
        if (ten > 1 && one == 0) words += teens[ten];

        return words.Trim();
    }
}
