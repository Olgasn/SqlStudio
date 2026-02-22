using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using SlqStudio.DTO;
using SlqStudio.Persistence.Models;

namespace SlqStudio.Application.Services.ReportBuider;

public class PdfReportBuilder : IPdfReportBuilder, IDisposable
{
    private readonly MemoryStream _memoryStream = new();
    private readonly PdfWriter _writer;
    private readonly PdfDocument _pdfDocument;
    private readonly Document _document;

    private readonly PdfFont _normalFont;
    private readonly PdfFont _boldFont;
    private readonly PdfFont _headerFont;
    private readonly PdfFont _subHeaderFont;
    private readonly PdfFont _codeFont;

    private readonly DeviceRgb _successColor = new(212, 237, 218);
    private readonly DeviceRgb _dangerColor = new(248, 215, 218);
    private readonly DeviceRgb _tableHeaderColor = new(200, 200, 200);
    private readonly DeviceRgb _resultBoxColor = new(227, 227, 227);
    private readonly DeviceRgb _codeBackgroundColor = new(39, 40, 34);
    private readonly DeviceRgb _codeForegroundColor = new(248, 248, 242);

    private bool _isClosed;

    public PdfReportBuilder()
    {
        _writer = new PdfWriter(_memoryStream);
        _pdfDocument = new PdfDocument(_writer);
        _document = new Document(_pdfDocument, PageSize.A4);
        _document.SetMargins(40, 40, 40, 40);

        (_normalFont, _boldFont, _headerFont, _subHeaderFont, _codeFont) = CreateFonts();
    }

    public IReportBuilder AddUserInfo(UserDto user)
    {
        _document.Add(
            new Paragraph($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}")
                .SetFont(_normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT));

        _document.Add(
            new Paragraph("Отчет по тесту")
                .SetFont(_headerFont)
                .SetFontSize(16)
                .SetMarginBottom(15));

        _document.Add(
            new Paragraph($"Пользователь: {user.Name}")
                .SetFont(_normalFont)
                .SetFontSize(10));

        _document.Add(
            new Paragraph($"Email: {user.Email}")
                .SetFont(_normalFont)
                .SetFontSize(10)
                .SetMarginBottom(10));

        return this;
    }

    public IReportBuilder AddWorkInfo(List<SolutionResultDto> solutions, List<LabWork> labWorks)
    {
        foreach (var lab in labWorks)
        {
            var totalTasks = 0;
            var completedTasks = 0;

            _document.Add(
                new Paragraph(lab.Name)
                    .SetFont(_subHeaderFont)
                    .SetFontSize(12)
                    .SetMarginBottom(10));

            _document.Add(
                new Paragraph($"Курс: {lab.Course?.Name ?? "Не указан"}")
                    .SetFont(_normalFont)
                    .SetFontSize(10)
                    .SetMarginBottom(8));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1f, 3f, 4f, 2f }))
                .UseAllAvailableWidth();

            AddTableHeaderCell(table, "№ Задачи");
            AddTableHeaderCell(table, "Название");
            AddTableHeaderCell(table, "Решение");
            AddTableHeaderCell(table, "Статус");

            foreach (var task in lab.Tasks ?? Enumerable.Empty<LabTask>())
            {
                var solution = solutions.FirstOrDefault(s => s.TaskId == task.Id);
                var isSuccess = solution?.IsSuccess == true;
                var status = isSuccess ? "Выполнено" : "Не выполнено";
                var userSolution = solution?.UserSolutionText ?? "Решение отсутствует";
                var shortSolution = userSolution.Length > 50
                    ? userSolution[..50] + "..."
                    : userSolution;

                var cellColor = isSuccess ? _successColor : _dangerColor;

                AddTableCell(table, task.Number.ToString(), cellColor);
                AddTableCell(table, task.Title, cellColor);
                AddTableCell(table, shortSolution, cellColor);
                AddTableCell(table, status, cellColor);

                totalTasks++;
                if (isSuccess)
                {
                    completedTasks++;
                }
            }

            _document.Add(table);

            if (totalTasks > 0)
            {
                var score = (double)completedTasks / totalTasks * 10;
                var percentage = (double)completedTasks / totalTasks * 100;

                var resultBox = new Div()
                    .SetBackgroundColor(_resultBoxColor)
                    .SetPadding(10)
                    .SetMarginTop(10)
                    .SetMarginBottom(20);

                resultBox.Add(
                    new Paragraph("Результаты лабораторной")
                        .SetFont(_boldFont)
                        .SetFontSize(10)
                        .SetMarginBottom(5));

                resultBox.Add(
                    new Paragraph($"Оценка: {score:0.00} из 10,00 ({percentage:0.00}%)")
                        .SetFont(_normalFont)
                        .SetFontSize(10)
                        .SetMargin(0));

                _document.Add(resultBox);
            }
            else
            {
                _document.Add(
                    new Paragraph("Нет данных для оценки.")
                        .SetFont(_normalFont)
                        .SetFontSize(10)
                        .SetMarginBottom(20));
            }
        }

        return this;
    }

    public IReportBuilder AddSolutionDetails(List<SolutionResultDto> solutions, List<LabWork> labWorks)
    {
        _document.Add(
            new Paragraph("Детали решений")
                .SetFont(_subHeaderFont)
                .SetFontSize(12)
                .SetMarginBottom(10));

        foreach (var solution in solutions)
        {
            var lab = labWorks.FirstOrDefault(l => l.Tasks.Any(t => t.Id == solution.TaskId));
            var task = lab?.Tasks.FirstOrDefault(t => t.Id == solution.TaskId);

            _document.Add(
                new Paragraph($"{lab?.Name ?? "Неизвестная лабораторная"} - {task?.Title ?? "Неизвестная задача"}")
                    .SetFont(_boldFont)
                    .SetFontSize(10)
                    .SetMarginBottom(5));

            var codeBlock = new Div()
                .SetBackgroundColor(_codeBackgroundColor)
                .SetPadding(10)
                .SetMarginBottom(15);

            codeBlock.Add(
                new Paragraph(solution.UserSolutionText)
                    .SetFont(_codeFont)
                    .SetFontColor(_codeForegroundColor)
                    .SetFontSize(8)
                    .SetMargin(0));

            _document.Add(codeBlock);
        }

        return this;
    }

    public byte[] Build()
    {
        CloseDocumentIfNeeded();
        return _memoryStream.ToArray();
    }

    public void Dispose()
    {
        CloseDocumentIfNeeded();
        _memoryStream.Dispose();
    }

    private static (PdfFont normal, PdfFont bold, PdfFont header, PdfFont subHeader, PdfFont code) CreateFonts()
    {
        var regularFontPath = ResolveFontPath(
            @"C:\Windows\Fonts\arial.ttf",
            @"C:\Windows\Fonts\segoeui.ttf");

        var boldFontPath = ResolveFontPath(
            @"C:\Windows\Fonts\arialbd.ttf",
            @"C:\Windows\Fonts\segoeuib.ttf",
            regularFontPath ?? string.Empty);

        var codeFontPath = ResolveFontPath(
            @"C:\Windows\Fonts\consola.ttf",
            @"C:\Windows\Fonts\cour.ttf",
            regularFontPath ?? string.Empty);

        var regular = CreateFontOrFallback(regularFontPath);
        var bold = CreateFontOrFallback(boldFontPath);
        var code = CreateFontOrFallback(codeFontPath);

        return (regular, bold, bold, bold, code);
    }

    private static string? ResolveFontPath(params string[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static PdfFont CreateFontOrFallback(string? fontPath)
    {
        if (!string.IsNullOrWhiteSpace(fontPath) && File.Exists(fontPath))
        {
            return PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);
        }

        return PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
    }

    private void AddTableHeaderCell(Table table, string text)
    {
        table.AddHeaderCell(
            new Cell()
                .Add(new Paragraph(text).SetFont(_boldFont).SetFontSize(10))
                .SetBackgroundColor(_tableHeaderColor)
                .SetBorder(new SolidBorder(ColorConstants.WHITE, 1))
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT));
    }

    private void AddTableCell(Table table, string text, DeviceRgb backgroundColor)
    {
        table.AddCell(
            new Cell()
                .Add(new Paragraph(text).SetFont(_normalFont).SetFontSize(10))
                .SetBackgroundColor(backgroundColor)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT));
    }

    private void CloseDocumentIfNeeded()
    {
        if (_isClosed)
        {
            return;
        }

        _document.Close();
        _isClosed = true;
    }
}
