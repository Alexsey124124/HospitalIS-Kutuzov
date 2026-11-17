namespace HospitalIS.WebApi.Models;

public static class EnumDisplay
{
    private static readonly Dictionary<string, string> Ru = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Pending"] = "Ожидает",
        ["New"] = "Новая",
        ["Created"] = "Создана",
        ["Confirmed"] = "Подтверждена",
        ["Scheduled"] = "Запланирована",
        ["InProgress"] = "В процессе",
        ["Active"] = "Активна",
        ["Completed"] = "Завершена",
        ["Done"] = "Завершена",
        ["Finished"] = "Завершена",
        ["Cancelled"] = "Отменена",
        ["Canceled"] = "Отменена",
        ["Rejected"] = "Отклонена",
        ["Closed"] = "Закрыта",
        ["Archived"] = "В архиве"
    };

    private static readonly HashSet<string> Ok = new(StringComparer.OrdinalIgnoreCase)
        { "Completed", "Done", "Finished", "Closed" };
    private static readonly HashSet<string> Bad = new(StringComparer.OrdinalIgnoreCase)
        { "Cancelled", "Canceled", "Rejected" };
    private static readonly HashSet<string> Info = new(StringComparer.OrdinalIgnoreCase)
        { "Confirmed", "Scheduled", "InProgress", "Active" };

    public static string Text(Enum value)
        => Ru.TryGetValue(value.ToString(), out var t) ? t : value.ToString();
    public static string Css(string status) => status switch
    {
        DispensaryStatusesNames.Observation => "info",
        DispensaryStatusesNames.Completed => "ok",
        DispensaryStatusesNames.Removed => "bad",
        _ => "wait"
    };

    private static class DispensaryStatusesNames
    {
        public const string Observation = "Наблюдение";
        public const string Completed = "Завершён";
        public const string Removed = "Снят с учёта";
    }
    public static string Css(Enum value)
    {

        var n = value.ToString();
        if (Ok.Contains(n)) return "ok";
        if (Bad.Contains(n)) return "bad";
        if (Info.Contains(n)) return "info";
        return "wait";
    }
}