using System;
using System.Globalization;

namespace GymManager
{
    public static class DateHelper
    {
        public static string ToDisplayDate(string dbDate)
        {
            if (string.IsNullOrWhiteSpace(dbDate) || dbDate == "-")
                return dbDate;

            if (DateTime.TryParseExact(dbDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt.ToString("dd.MM.yyyy");
            }
            return dbDate;
        }

        public static string ToDbDate(string displayDate)
        {
            if (string.IsNullOrWhiteSpace(displayDate) || displayDate == "-")
                return "";

            if (DateTime.TryParseExact(displayDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt.ToString("yyyy-MM-dd");
            }

            if (DateTime.TryParseExact(displayDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtIso))
            {
                return dtIso.ToString("yyyy-MM-dd");
            }

            return displayDate;
        }

        public static string ToDisplayDateTime(string dbDateTime)
        {
            if (string.IsNullOrWhiteSpace(dbDateTime))
                return "";

            if (DateTime.TryParseExact(dbDateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt.ToString("dd.MM.yyyy HH:mm:ss");
            }

            if (DateTime.TryParse(dbDateTime, out DateTime parsedDateTime))
            {
                return parsedDateTime.ToString("dd.MM.yyyy HH:mm:ss");
            }

            return dbDateTime;
        }
    }
}
