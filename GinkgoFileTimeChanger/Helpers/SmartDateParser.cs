using System.Globalization;
using System.Text.RegularExpressions;

public class SmartDateParser
{
    // 调用前面写好的智能解析器
    public static DateTime? ExtractDateFromFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        // 正则模式，尽量涵盖常见的日期/时间戳形式
        string[] patterns = {
            @"\d{4}[-_]\d{1,2}[-_]\d{1,2}([ _-]\d{1,2}[:\-]\d{1,2}([:\-]\d{1,2})?)?", // yyyy-MM-dd HH-mm-ss
            @"\d{4}年\d{1,2}月\d{1,2}日([ _]?\d{1,2}点\d{1,2}分(\d{1,2}秒)?)?",       // 中文日期
            @"\d{8}(\d{6}(\d{3})?)?",                                                  // yyyyMMdd / yyyyMMddHHmmss / yyyyMMddHHmmssfff
            @"\d{10,13}"                                                               // 时间戳
        };

        foreach (var pattern in patterns)
        {
            foreach (Match m in Regex.Matches(fileName, pattern))
            {
                var candidate = m.Value;
                var dt = SmartDateParser.ExtractDate(candidate);
                if (dt != null)
                    return dt;
            }
        }

        return null;
    }

    public static DateTime? ExtractDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        // 1. 检查是否是纯数字（可能是时间戳或紧凑日期）
        if (Regex.IsMatch(input, @"^\d{10,17}$"))
        {
            if (long.TryParse(input, out long num))
            {
                // 秒级时间戳 (10位)
                if (input.Length == 10)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(num).LocalDateTime;
                }
                // 毫秒级时间戳 (13位)
                else if (input.Length == 13)
                {
                    return DateTimeOffset.FromUnixTimeMilliseconds(num).LocalDateTime;
                }
                // 紧凑日期格式 (8位=yyyyMMdd / 14位=yyyyMMddHHmmss / 17位=yyyyMMddHHmmssfff)
                else
                {
                    string[] compactFormats = {
                        "yyyyMMdd",
                        "yyyyMMddHHmmss",
                        "yyyyMMddHHmmssfff"
                    };
                    foreach (var fmt in compactFormats)
                    {
                        if (DateTime.TryParseExact(input, fmt, CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime dt))
                            return dt;
                    }
                }
            }
        }

        // 2. 常见日期格式 (带分隔符)
        string[] dateFormats = {
            "yyyy-MM-dd HH-mm-ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy_MM_dd_HH_mm_ss",
            "yyyyMMddHHmmss",
            "yyyyMMdd",
            "yyyy-MM-dd",
            "yyyy_MM_dd",
            "yyyy.MM.dd-HH.mm.ss",
        };

        foreach (var fmt in dateFormats)
        {
            if (DateTime.TryParseExact(input, fmt, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime dt))
                return dt;
        }

        // 3. 中文日期格式
        string normalized = input
            .Replace("年", "-")
            .Replace("月", "-")
            .Replace("日", " ")
            .Replace("点", ":")
            .Replace("分", ":")
            .Replace("秒", "")
            .Trim();

        if (DateTime.TryParse(normalized, out DateTime chineseDate))
            return chineseDate;

        // 4. 最后尝试通用解析器
        if (DateTime.TryParse(input, out DateTime generalDate))
            return generalDate;

        return null;
    }
}
