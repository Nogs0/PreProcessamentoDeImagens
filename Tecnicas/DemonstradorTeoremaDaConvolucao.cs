using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.LinearAlgebra;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TecnicasPreProcessamentoDeImagens.Tecnicas;

public class DemonstradorTeoremaDaConvolucao
{
    public static void ExecutarDemonstracaoDeGanho()
    {
        Console.WriteLine("Insira o caminho da imagem:");
        string? imagePath = Console.ReadLine();
        if (string.IsNullOrEmpty(imagePath))
        {
            Console.WriteLine("Nenhum imagem foi encontrada...");
            return;
        }

        Console.WriteLine("--- Comparação de Tempo ---");

        long tempoEspacial = AplicarFiltroEspacial(imagePath);
        Console.WriteLine($"Tempo (Convolução Espacial): {tempoEspacial} ms");

        long tempoFrequencia = AplicarFiltroFrequencia(imagePath);
        Console.WriteLine($"Tempo (Domínio da Frequência): {tempoFrequencia} ms");
    }

    private static long AplicarFiltroEspacial(string imagePath)
    {
        var image = Image.Load<L8>(imagePath);
        var sw = Stopwatch.StartNew();
        var resultado = image.Clone(ctx => ctx.GaussianBlur(6f));
        sw.Stop();
        resultado.Save("resultado_espacial.png");
        return sw.ElapsedMilliseconds;
    }

    private static long AplicarFiltroFrequencia(string imagePath)
    {
        var image = Image.Load<Rgba32>(imagePath);
        var sw = Stopwatch.StartNew();

        int width = image.Width;
        int height = image.Height;

        // Converter para tons de cinza
        double[,] gray = new double[height, width];
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    var pixel = row[x];
                    gray[y, x] = 0.299 * pixel.R + 0.587 * pixel.G + 0.114 * pixel.B;
                }
            }
        });

        // FFT da imagem
        Complex[,] freq = new Complex[height, width];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            freq[y, x] = new Complex(gray[y, x], 0);

        FFT2(freq);

        // Gerar e aplicar FFT ao kernel gaussiano (com shift)
        double[,] kernel = GerarKernelGaussiano(width, height, sigma: 6);
        FFTShift(kernel); // importante!
        Complex[,] kernelFreq = new Complex[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                kernelFreq[y, x] = new Complex(kernel[y, x], 0);
        }

        FFT2(kernelFreq);

        // Multiplicação ponto a ponto (convolução no domínio da frequência)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                freq[y, x] *= kernelFreq[y, x];
        }

        // Inversa
        InverseFFT2(freq);

        // Normalizar pela dimensão (MathNet não normaliza automaticamente)
        double scale = width * height;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                freq[y, x] /= scale;
        }

        // Encontrar o menor e maior valor para normalização
        double min = double.MaxValue;
        double max = double.MinValue;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double valor = freq[y, x].Real;
                if (valor < min) min = valor;
                if (valor > max) max = valor;
            }
        }

        // Criar imagem final com normalização 0–255
        var resultado = new Image<L8>(width, height);
        resultado.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    double valor = freq[y, x].Real;
                    valor = (valor - min) / (max - min) * 255.0;
                    row[x] = new L8((byte)Math.Clamp(valor, 0, 255));
                }
            }
        });

        sw.Stop();
        resultado.Save("resultado_frequencia.png");
        return sw.ElapsedMilliseconds;
    }

    private static double[,] GerarKernelGaussiano(int width, int height, double sigma)
    {
        double[,] kernel = new double[height, width];
        int cx = width / 2, cy = height / 2;
        double sigma2 = sigma * sigma;
        double normalizador = 1.0 / (2 * Math.PI * sigma2);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int dx = x - cx;
                int dy = y - cy;
                kernel[y, x] = normalizador * Math.Exp(-(dx * dx + dy * dy) / (2 * sigma2));
            }
        }

        return kernel;
    }

    private static void FFTShift(double[,] data)
    {
        int height = data.GetLength(0);
        int width = data.GetLength(1);

        for (int y = 0; y < height / 2; y++)
        {
            for (int x = 0; x < width / 2; x++)
            {
                // Coordenadas opostas
                int yOpp = y + height / 2;
                int xOpp = x + width / 2;

                // Troca quadrantes
                (data[y, x], data[yOpp, xOpp]) = (data[yOpp, xOpp], data[y, x]);
                (data[yOpp, x], data[y, xOpp]) = (data[y, xOpp], data[yOpp, x]);
            }
        }
    }


    private static void FFT2(Complex[,] data)
    {
        int height = data.GetLength(0);
        int width = data.GetLength(1);

        // Linha por linha
        for (int y = 0; y < height; y++)
        {
            Complex[] row = new Complex[width];
            for (int x = 0; x < width; x++)
                row[x] = data[y, x];
            Fourier.Forward(row);
            for (int x = 0; x < width; x++)
                data[y, x] = row[x];
        }

        // Coluna por coluna
        for (int x = 0; x < width; x++)
        {
            Complex[] col = new Complex[height];
            for (int y = 0; y < height; y++)
                col[y] = data[y, x];
            Fourier.Forward(col);
            for (int y = 0; y < height; y++)
                data[y, x] = col[y];
        }
    }

    private static void InverseFFT2(Complex[,] data)
    {
        int height = data.GetLength(0);
        int width = data.GetLength(1);

        // Linha por linha
        for (int y = 0; y < height; y++)
        {
            Complex[] row = new Complex[width];
            for (int x = 0; x < width; x++)
                row[x] = data[y, x];
            Fourier.Inverse(row);
            for (int x = 0; x < width; x++)
                data[y, x] = row[x];
        }

        // Coluna por coluna
        for (int x = 0; x < width; x++)
        {
            Complex[] col = new Complex[height];
            for (int y = 0; y < height; y++)
                col[y] = data[y, x];
            Fourier.Inverse(col);
            for (int y = 0; y < height; y++)
                data[y, x] = col[y];
        }
    }
}