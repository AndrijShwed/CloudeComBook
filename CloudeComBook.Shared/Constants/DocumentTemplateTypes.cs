namespace CloudeComBook.Shared.Constants;

public static class DocumentTemplateTypes
{
    public const string FamilyComposition = "family_composition";
    public const string Characteristic = "characteristic";
    public const string Testament = "testament";
    public const string Subsidy = "subsidy";
    public const string Benefits = "benefits";
    public const string TestamentRegistration = "testament_registration";

    public static readonly string[] All =
    {
        FamilyComposition, Characteristic, Testament,
        Subsidy, Benefits, TestamentRegistration
    };

    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        [FamilyComposition] = "Довідка про склад сім'ї",
        [Characteristic] = "Характеристика",
        [Testament] = "Заява (заповіт)",
        [Subsidy] = "Довідка на субсидію",
        [Benefits] = "Довідка на пільги",
        [TestamentRegistration] = "Заява про реєстрацію заповіту"
    };
}
