using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics.IntegralTransforms;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

        long tempoEspacial = AplicarConvolucaoEspacial(imagePath);
        Console.WriteLine($"Tempo (Convolução Espacial): {tempoEspacial} ms");

        long tempoFrequencia = AplicarFiltroFrequencia(imagePath);
        Console.WriteLine($"Tempo (Domínio da Frequência): {tempoFrequencia} ms");
    }
    public static long AplicarConvolucaoEspacial(string imagePath)
    {
        var sw = Stopwatch.StartNew();
        // 1. Extrai a matriz da imagem
        float[,] imagem = ExtrairMatriz(imagePath);

        // 2. Gera o kernel gaussiano
        float[,] kernel = GerarKernelGaussiano2D(6f);

        // 3. Aplica convolução
        float[,] resultado = Convoluir(imagem, kernel);

        // 4. Gera a imagem de saída
        var imagemSaida = CriarImagem(resultado);

        // 5. Salva
        sw.Stop();
        imagemSaida.Save("resultado_espacial_convolucao.png");
        return sw.ElapsedMilliseconds;
    }

    private static float[,] ExtrairMatriz(string imagePath)
    {
        using var image = Image.Load<L8>(imagePath);
        int width = image.Width;
        int height = image.Height;
        float[,] matriz = new float[height, width];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    matriz[y, x] = row[x].PackedValue;
                }
            }
        });

        return matriz;
    }

    private static Image<L8> CriarImagem(float[,] matriz)
    {
        int height = matriz.GetLength(0);
        int width = matriz.GetLength(1);
        var imagem = new Image<L8>(width, height);

        imagem.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    byte value = (byte)Math.Clamp(matriz[y, x], 0, 255);
                    row[x] = new L8(value);
                }
            }
        });

        return imagem;
    }

    private static float[,] GerarKernelGaussiano2D(double sigma)
    {
        int raio = (int)Math.Ceiling(sigma * 3); // geralmente 3 sigma
        int tamanho = 2 * raio + 1;
        float[,] kernel = new float[tamanho, tamanho];

        double sigma2 = sigma * sigma;
        double normalizador = 1.0 / (2 * Math.PI * sigma2);
        double soma = 0;

        for (int y = -raio; y <= raio; y++)
        {
            for (int x = -raio; x <= raio; x++)
            {
                double valor = normalizador * Math.Exp(-(x * x + y * y) / (2 * sigma2));
                kernel[y + raio, x + raio] = (float)valor;
                soma += valor;
            }
        }

        // Normaliza o kernel
        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                kernel[y, x] /= (float)soma;
            }
        }

        return kernel;
    }

    private static float[,] Convoluir(float[,] imagem, float[,] kernel)
    {
        int altura = imagem.GetLength(0);
        int largura = imagem.GetLength(1);
        int kAltura = kernel.GetLength(0);
        int kLargura = kernel.GetLength(1);
        int kCentroY = kAltura / 2;
        int kCentroX = kLargura / 2;

        float[,] resultado = new float[altura, largura];

        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                float soma = 0f;

                for (int j = 0; j < kAltura; j++)
                {
                    for (int i = 0; i < kLargura; i++)
                    {
                        int yi = y + j - kCentroY;
                        int xi = x + i - kCentroX;

                        if (xi >= 0 && xi < largura && yi >= 0 && yi < altura)
                        {
                            soma += imagem[yi, xi] * kernel[j, i];
                        }
                    }
                }

                resultado[y, x] = soma;
            }
        }

        return resultado;
    }

    private static long AplicarFiltroFrequencia(string imagePath)
    {
        // 1. Carrega a imagem e converte para tons de cinza
        var sw = Stopwatch.StartNew();
        using var image = Image.Load<L8>(imagePath);
        int width = image.Width;
        int height = image.Height;

        // 2. Cria matriz de complexos a partir da imagem
        Complex[,] imgComplex = new Complex[height, width];
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    imgComplex[y, x] = new Complex(row[x].PackedValue, 0);
                }
            }
        });

        // 3. Aplica FFT 2D
        FFT2D(imgComplex, true);

        // 4. Cria kernel gaussiano e aplica FFT
        var kernel = GerarKernelGaussiano(width, height, 6f);
        FFT2D(kernel, true);

        // 5. Multiplica espectros
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            imgComplex[y, x] *= kernel[y, x];

        // 6. Aplica Inversa
        FFT2D(imgComplex, false);

        // 7. Normaliza e salva
        var resultado = new Image<L8>(width, height);
        double max = double.MinValue, min = double.MaxValue;

        // Acha os limites para normalização
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            double val = imgComplex[y, x].Real;
            if (val > max) max = val;
            if (val < min) min = val;
        }

        resultado.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    double val = imgComplex[y, x].Real;
                    val = (val - min) / (max - min) * 255.0;
                    val = Math.Clamp(val, 0, 255);
                    row[x] = new L8((byte)val);
                }
            }
        });

        sw.Stop();
        resultado.Save("resultado_frequencia.png");
        return sw.ElapsedMilliseconds;
    }
    
    // FFT 2D para matriz Complex[,]
    private static void FFT2D(Complex[,] data, bool forward)
    {
        int height = data.GetLength(0);
        int width = data.GetLength(1);

        // Linha por linha
        for (int y = 0; y < height; y++)
        {
            var row = new Complex[width];
            for (int x = 0; x < width; x++) row[x] = data[y, x];
            if (forward) Fourier.Forward(row, FourierOptions.Matlab);
            else Fourier.Inverse(row, FourierOptions.Matlab);
            for (int x = 0; x < width; x++) data[y, x] = row[x];
        }

        // Coluna por coluna
        for (int x = 0; x < width; x++)
        {
            var col = new Complex[height];
            for (int y = 0; y < height; y++) col[y] = data[y, x];
            if (forward) Fourier.Forward(col, FourierOptions.Matlab);
            else Fourier.Inverse(col, FourierOptions.Matlab);
            for (int y = 0; y < height; y++) data[y, x] = col[y];
        }
    }

    // Gera um kernel Gaussiano do mesmo tamanho da imagem
    private static Complex[,] GerarKernelGaussiano(int width, int height, double sigma)
    {
        var kernel = new Complex[height, width];
        int cx = width / 2, cy = height / 2;
        double sigma2 = sigma * sigma;

        for (int y = 0; y < height; y++)
        {
            int dy = y - cy;
            for (int x = 0; x < width; x++)
            {
                int dx = x - cx;
                double gauss = Math.Exp(-(dx * dx + dy * dy) / (2 * sigma2));
                kernel[y, x] = new Complex(gauss, 0);
            }
        }

        return kernel;
    }
}