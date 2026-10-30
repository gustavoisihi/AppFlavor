
using System.Drawing;
using System.Drawing.Printing;
using System.Runtime.Versioning;

namespace BlazorApp.Services.Impressao;

[SupportedOSPlatform("windows6.1")]
public class ImpressoraWindows
{
    public string? ObterImpressoraPadrao()
    {
        var impressora = new PrinterSettings();

        return impressora.IsValid
            ? impressora.PrinterName
            : null;
    }


    public void ImprimirTexto(string texto, string? nomeImpressora = null)
    {
        using var documento = new PrintDocument();

        if (!string.IsNullOrWhiteSpace(nomeImpressora))
            documento.PrinterSettings.PrinterName = nomeImpressora;

        if (!documento.PrinterSettings.IsValid)
            throw new InvalidOperationException(
                "A impressora selecionada não é válida ou não está instalada.");

        documento.PrintPage += (_, e) =>
        {
            using var fonte = new Font("Arial", 10, FontStyle.Regular);

            e.Graphics!.DrawString(
                texto,
                fonte,
                Brushes.Black,
                new RectangleF(
                    10,
                    10,
                    e.PageBounds.Width - 20,
                    e.PageBounds.Height - 20));
        };

        documento.Print();
    }

}
