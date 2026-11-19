namespace Cinewave.Catalogue.Domain;

/// <summary>An ISO 3166-1 alpha-2 country code, the unit licences are sold in.</summary>
internal readonly record struct Territory
{
    private Territory(string code) => Code = code;

    public string Code { get; }

    public static Territory Parse(string raw)
    {
        var code = (raw ?? string.Empty).Trim().ToUpperInvariant();
        if (code.Length != 2 || !code.All(c => c is >= 'A' and <= 'Z'))
        {
            throw new ArgumentException($"'{raw}' is not a two-letter territory code.", nameof(raw));
        }

        return new Territory(code);
    }

    public override string ToString() => Code;
}
