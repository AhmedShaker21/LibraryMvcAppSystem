namespace LibraryMvcApp.Helpers
{
    /// <summary>
    /// Natural ordering: digit runs are compared as numbers ("P 53-2" &lt; "P 53-10"),
    /// letters are compared case-insensitively, and blank values go last.
    /// </summary>
    public sealed class NaturalStringComparer : IComparer<string?>
    {
        public static readonly NaturalStringComparer Instance = new();

        public int Compare(string? x, string? y)
        {
            bool xBlank = string.IsNullOrWhiteSpace(x);
            bool yBlank = string.IsNullOrWhiteSpace(y);

            if (xBlank && yBlank) return 0;
            if (xBlank) return 1;
            if (yBlank) return -1;

            string a = x!.Trim();
            string b = y!.Trim();

            int i = 0, j = 0;
            while (i < a.Length && j < b.Length)
            {
                if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
                {
                    int si = i, sj = j;
                    while (i < a.Length && char.IsDigit(a[i])) i++;
                    while (j < b.Length && char.IsDigit(b[j])) j++;

                    var na = a.AsSpan(si, i - si).TrimStart('0');
                    var nb = b.AsSpan(sj, j - sj).TrimStart('0');

                    if (na.Length != nb.Length) return na.Length.CompareTo(nb.Length);

                    int numCmp = na.CompareTo(nb, StringComparison.Ordinal);
                    if (numCmp != 0) return numCmp;
                }
                else
                {
                    int c = char.ToUpperInvariant(a[i]).CompareTo(char.ToUpperInvariant(b[j]));
                    if (c != 0) return c;
                    i++;
                    j++;
                }
            }

            return (a.Length - i).CompareTo(b.Length - j);
        }
    }
}
