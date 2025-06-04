using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TecnicasPreProcessamentoDeImagens.Tecnicas;

public class AplicadorFiltroHighBoost
{
    public static void ExecutarComparacaoDeResultados()
    {
        string outputPathHighBoost = "3_highBoost.png";
        Console.WriteLine("Insira o caminho da imagem:"); 
        string? imagePath = Console.ReadLine();
        if (string.IsNullOrEmpty(imagePath))
        {
            Console.WriteLine("Nenhum imagem foi encontrada...");
            return;
        }
        
        using var outputImageHighBoost = AplicarFiltroHighBoost(imagePath);
        outputImageHighBoost.SaveAsPng(outputPathHighBoost);
        Console.WriteLine($"Imagem processada salva em: {outputPathHighBoost}");

        string outputPathPassaAlta = "3_passaAlta.png";
        using var outputImagePassaAlta = AplicarFiltroPassaAlta(imagePath);
        outputImagePassaAlta.SaveAsPng(outputPathPassaAlta);
        
        Console.WriteLine($"Imagem processada salva em: {outputPathPassaAlta}");
    }
    
    private static Image<Rgba32> AplicarFiltroHighBoost(string imagePath, float boostFactor = 1f)
    {
        // Clonar a imagem original
        var original = Image.Load<Rgba32>(imagePath);
        var originalClone = original.Clone();

        // Criar imagem borrada
        var blurred = original.Clone(ctx => ctx.GaussianBlur(1.0f)); // sigma = 1.0

        int width = original.Width;
        int height = original.Height;

        for (int y = 0; y < height; y++)
        {
            var origRow = originalClone.DangerousGetPixelRowMemory(y).Span;
            var blurRow = blurred.DangerousGetPixelRowMemory(y).Span;

            for (int x = 0; x < width; x++)
            {
                var orig = origRow[x];
                var blur = blurRow[x];

                // Detalhe = original - borrada
                int detailR = orig.R - blur.R;
                int detailG = orig.G - blur.G;
                int detailB = orig.B - blur.B;

                // HighBoost = original + fator * detalhe
                int newR = orig.R + (int)(boostFactor * detailR);
                int newG = orig.G + (int)(boostFactor * detailG);
                int newB = orig.B + (int)(boostFactor * detailB);

                origRow[x] = new Rgba32((byte)newR, (byte)newG, (byte)newB, orig.A);
            }
        }

        return originalClone;
    }
    
    private static Image<Rgba32> AplicarFiltroPassaAlta(string imagePath)
    {
        // Clonar a imagem original
        var original = Image.Load<Rgba32>(imagePath);
        var originalClone = original.Clone();

        // Criar imagem borrada
        var blurred = original.Clone(ctx => ctx.GaussianBlur(1.0f)); // sigma = 1.0

        int width = original.Width;
        int height = original.Height;

        for (int y = 0; y < height; y++)
        {
            var origRow = originalClone.DangerousGetPixelRowMemory(y).Span;
            var blurRow = blurred.DangerousGetPixelRowMemory(y).Span;

            for (int x = 0; x < width; x++)
            {
                var orig = origRow[x];
                var blur = blurRow[x];

                // HighBoost = original - blur
                int newR = orig.R - blur.R;
                int newG = orig.G - blur.G;
                int newB = orig.B - blur.B;

                origRow[x] = new Rgba32((byte)newR, (byte)newG, (byte)newB, orig.A);
            }
        }

        return originalClone;
    }
}