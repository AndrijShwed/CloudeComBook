using CloudeComBook.Web.Models;

namespace CloudeComBook.Web.Helpers;

public static class DocumentTypeConfig
{
    public static readonly Dictionary<string, string> DisplayNames = new()
    {
        ["family_composition"] = "Довідка про склад сім'ї",
        ["characteristic"] = "Характеристика",
        ["testament"] = "Заява (заповіт)",
        ["subsidy"] = "Довідка на субсидію",
        ["benefits"] = "Довідка на пільги",
        ["testament_registration"] = "Заява про реєстрацію заповіту"
    };

    public static List<DocumentFieldSpec> GetFields(string templateType) => templateType switch
    {
        "family_composition" => new()
        {
            new() { Key = "НомерДовідки", Label = "Номер довідки" }
        },
        "characteristic" => new()
        {
            new() { Key = "НомерДовідки", Label = "Номер довідки" }
        },
        "testament" => new()
        {
            new() { Key = "місценародження", Label = "Місце народження" },
            new() { Key = "ПІБкому", Label = "ПІБ (кому)" },
            new() { Key = "Датанародженнякому", Label = "Дата народження (кому)", IsDate = true },
            new() { Key = "НомерЗаповіту", Label = "Номер заповіту" }
        },
        "subsidy" => new()
        {
            new() { Key = "НомерДовідки", Label = "Номер довідки" }
        },
        "benefits" => new()
        {
            new() { Key = "НомерДовідки", Label = "Номер довідки" }
        },
        "testament_registration" => new()
        {
            new() { Key = "regnumber", Label = "Реєстраційний номер" },
            new() { Key = "place", Label = "Місце народження" },
            new() { Key = "ПоштовийКодЗаповідача", Label = "Поштовий код заповідача" },
            new() { Key = "ДатаРеєстраціїЗаповіту", Label = "Дата реєстрації заповіту", IsDate = true }
        },
        _ => new()
    };
}
