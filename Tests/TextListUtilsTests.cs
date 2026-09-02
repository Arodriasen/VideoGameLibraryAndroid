using VideoGameLibraryAndroid.Core;

namespace VideoGameLibraryAndroid.Tests;

public class TextListUtilsTests
{
    [Theory]
    [InlineData("Acción, Aventura", new[] { "Acción", "Aventura" })]
    [InlineData("Acción,Aventura", new[] { "Acción", "Aventura" })]
    [InlineData("  Acción  ,  Aventura  ", new[] { "Acción", "Aventura" })]
    [InlineData("Acción", new[] { "Acción" })]
    [InlineData("", new string[0])]
    [InlineData("Acción,,Aventura", new[] { "Acción", "Aventura" })]
    public void SplitGenres_separa_recorta_y_quita_vacios(string input, string[] expected)
    {
        Assert.Equal(expected, TextListUtils.SplitGenres(input));
    }

    [Theory]
    [InlineData("Favorito, Prestado", new[] { "Favorito", "Prestado" })]
    [InlineData("", new string[0])]
    public void SplitTags_mismo_comportamiento_que_SplitGenres(string input, string[] expected)
    {
        Assert.Equal(expected, TextListUtils.SplitTags(input));
    }
}
