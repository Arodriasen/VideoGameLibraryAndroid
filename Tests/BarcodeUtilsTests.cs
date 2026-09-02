using VideoGameLibraryAndroid.Core;

namespace VideoGameLibraryAndroid.Tests;

public class BarcodeUtilsTests
{
    [Theory]
    [InlineData("045496590048", "045496590048")]
    [InlineData("0454-9659-0048", "045496590048")]
    [InlineData("045 496 590 048", "045496590048")]
    [InlineData("abc123def456", "123456")]
    public void NormalizeBarcode_deja_solo_digitos(string raw, string expected)
    {
        Assert.Equal(expected, BarcodeUtils.NormalizeBarcode(raw));
    }

    [Fact]
    public void GetBarcodeVariants_UPCA_de_12_digitos_anade_variante_EAN13_con_cero_inicial()
    {
        var variants = BarcodeUtils.GetBarcodeVariants("045496590048");
        Assert.Equal(new[] { "045496590048", "0045496590048" }, variants);
    }

    [Fact]
    public void GetBarcodeVariants_EAN13_con_cero_inicial_anade_variante_UPCA_sin_el()
    {
        var variants = BarcodeUtils.GetBarcodeVariants("0045496590048");
        Assert.Equal(new[] { "0045496590048", "045496590048" }, variants);
    }

    [Fact]
    public void GetBarcodeVariants_otras_longitudes_no_anaden_variante()
    {
        var variants = BarcodeUtils.GetBarcodeVariants("1234567890123456");
        Assert.Equal(new[] { "1234567890123456" }, variants);
    }

    [Theory]
    [InlineData("Released 2017-10-27", 2017)]
    [InlineData("2017", 2017)]
    [InlineData("sin fecha", null)]
    [InlineData("", null)]
    [InlineData("1969-01-01", null)] // por debajo del rango válido (>= 1970)
    public void ExtractYear_encuentra_el_primer_grupo_de_4_digitos_valido(string text, int? expected)
    {
        Assert.Equal(expected, BarcodeUtils.ExtractYear(text));
    }
}
