using System.Data;
using System.Text.RegularExpressions;

namespace FlatsParser
{
    internal class RegexPageParser
    {
        public static Flat ParsePage(string downloadString)
        {
            var flat = new Flat();
            ParseMainInfo(downloadString, flat);
            ParseAreaSection(downloadString, flat);
            ParsePrice(downloadString, flat);
            ParseState(downloadString, flat);
            return flat;
        }

        private static void ParsePrice(string downloadString, Flat flat)
        {
            var priceRegex = new Regex(@"\<span class\=\'b\-book-apartment__price b-price-style1'\>\n(?'price'([0-9]* )*)");
            var priceMatch = priceRegex.Match(downloadString);
            if (priceMatch.Success)
            {
                var value = priceMatch.Groups["price"].Value;
                var price = value.Replace(" ", string.Empty);
                flat.Price = int.Parse(price);
            }
        }

        private static void ParseMainInfo(string downloadString, Flat flat)
        {
            var mainInfoRegex =
                new Regex(
                    @"������ ������� � ������ ��(?'Section'\d{1}) � (?'Floor'\d{1,2}) ���� � �������� � (?'Number'\d{1,3}) � �������",
                    RegexOptions.IgnoreCase);
            var match = mainInfoRegex.Match(downloadString);
            if (!match.Success)
                return;
            flat.Section = int.Parse(match.Groups["Section"].Value);
            flat.Floor = int.Parse(match.Groups["Floor"].Value);
            flat.Number = int.Parse(match.Groups["Number"].Value);
        }

        private static void ParseAreaSection(string downloadString, Flat flat)
        {
            var regex = new Regex(@"\<span class=\'level\'\>(?'number'([0-9]*(\.|\,)[0-9]*)|([0-9]*))(?!��.)",
                RegexOptions.Multiline);
            var numberMatch = regex.Matches(downloadString);
            for (var i = 0; i < numberMatch.Count; i++)
            {
                var match1 = numberMatch[i];
                if (!match1.Success)
                    continue;
                var str = match1.Groups["number"].Value;
                if (string.IsNullOrEmpty(str))
                    continue;
                str = str.Replace(',', '.');
                var d = double.Parse(str);
                switch (i)
                {
                    case 0:
                        flat.RoomsCount = (int) d;
                        break;
                    case 1:
                        flat.TotalArea = d;
                        break;
                    case 2:
                        flat.LivingArea = d;
                        break;
                    case 3:
                        if ((int) d != flat.Floor)
                            throw new DataException("Floor mismatch");
                        break;
                    case 4:
                        flat.KitchenArea = d;
                        break;
                }
            }
        }

        private static void ParseState(string downloadString, Flat flat)
        {
            if (downloadString.Contains("�������� �������"))
            {
                flat.CurrentState = State.Saled;
                return;
            }
            if (downloadString.Contains("�������������"))
            {
                flat.CurrentState = State.Reserved;
                return;
            }
            flat.CurrentState = State.Free;
        }
    }
}