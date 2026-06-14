using System.Text;
using SlqStudio.Application.Services.ReportBuider;
using SlqStudio.Application.DTO;
using SlqStudio.Persistence.Models;

namespace SlqStudio.Application.Services.ReportBuider.Implementation;

public class HtmlReportBuilder : IHtmlReportBuilder
{
    private StringBuilder _html;

    public HtmlReportBuilder()
    {
        _html = new StringBuilder();
        _html.Append("<html><head><meta charset='UTF-8'><title>Отчет по тесту</title>");
        _html.Append("<style>");
        _html.Append("body { font-family: Arial, sans-serif; margin: 20px; background-color: #f4f4f4; }");
        _html.Append(".container { width: 90%; max-width: 900px; margin: auto; background: #fff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1); }");
        _html.Append("h2, h3 { color: #333; }");
        _html.Append(".table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
        _html.Append(".table th, .table td { border: 1px solid #ccc; padding: 10px; text-align: left; }");
        _html.Append(".table th { background-color: #c8c8c8; color: black; }");
        _html.Append(".success { background-color: #d4edda; }");
        _html.Append(".danger { background-color: #f8d7da; }");
        _html.Append(".result-box { padding: 15px; margin-top: 20px; background: #e3e3e3; border-radius: 5px; }");
        _html.Append(".code-block { background-color: #272822; color: #f8f8f2; padding: 10px; border-radius: 5px; font-family: 'Courier New', monospace; white-space: pre-wrap; }");
        _html.Append("</style>");
        _html.Append("</head><body>");
        _html.Append("<div class='container'>");
    }

    public IReportBuilder AddUserInfo(UserDto user)
    {
        _html.Append("<h2>Отчет по тесту</h2>");
        _html.AppendFormat("<p><strong>Пользователь:</strong> {0}</p>", user.Name);
        _html.AppendFormat("<p><strong>Email:</strong> {0}</p>", user.Email);
        return this;
    }

    public IReportBuilder AddWorkInfo(List<SolutionResultDto> solutions, List<LabWork> labWorks)
    {
        foreach (var lab in labWorks)
        {
            int totalTasks = 0;
            int completedTasks = 0;

            _html.AppendFormat("<h3>{0}</h3>", lab.Name);
            _html.AppendFormat("<p><strong>Курс:</strong> {0}</p>", lab.Course?.Name ?? "Не указан");

            _html.Append("<table class='table'>");
            _html.Append("<thead><tr>");
            _html.Append("<th>№ Задачи</th><th>Название</th><th>Решение</th><th>Статус</th>");
            _html.Append("</tr></thead><tbody>");

            foreach (var task in lab.Tasks)
            {
                var solution = solutions.FirstOrDefault(s => s.TaskId == task.Id);
                bool isSuccess = solution?.IsSuccess == true;
                string rowClass = isSuccess ? "success" : "danger";
                string status = isSuccess ? "✅ Выполнено" : "❌ Не выполнено";
                string userSolution = solution?.UserSolutionText ?? "Решение отсутствует";
                string shortSolution = userSolution.Length > 20 ? userSolution.Substring(0, 20) + "..." : userSolution;

                _html.AppendFormat("<tr class='{0}'>", rowClass);
                _html.AppendFormat("<td>{0}</td>", task.Number);
                _html.AppendFormat("<td>{0}</td>", task.Title);
                _html.AppendFormat("<td title='{0}'>{1}</td>", userSolution, shortSolution);
                _html.AppendFormat("<td>{0}</td>", status);
                _html.Append("</tr>");

                totalTasks++;
                if (isSuccess) completedTasks++;
            }

            _html.Append("</tbody></table>");

            if (totalTasks > 0)
            {
                double score = (double)completedTasks / totalTasks * 10;
                double percentage = (double)completedTasks / totalTasks * 100;

                _html.Append("<div class='result-box'>");
                _html.Append("<h4>Результаты лабораторной</h4>");
                _html.AppendFormat("<p><strong>Оценка:</strong> {0:0.00} из 10,00 ({1:0.00}%)</p>", score, percentage);
                _html.Append("</div>");
            }
            else
            {
                _html.Append("<p><strong>Нет данных для оценки.</strong></p>");
            }
        }

        return this;
    }

    public IReportBuilder AddSolutionDetails(List<SolutionResultDto> solutions, List<LabWork> labWorks)
    {
        _html.Append("<h4>Детали решений</h4>");
        foreach (var solution in solutions)
        {
            var lab = labWorks.FirstOrDefault(l => l.Tasks.Any(t => t.Id == solution.TaskId));
            var task = lab?.Tasks.FirstOrDefault(t => t.Id == solution.TaskId);

            string formattedSql = solution.UserSolutionText?.Replace("<", "&lt;")?.Replace(">", "&gt;")?.Replace("\n", "<br>") ?? "Решение отсутствует";
            
            _html.AppendFormat("<div class='mb-3' id='task-{0}'>", solution.TaskId);
            _html.AppendFormat("<h5>{0} - {1}</h5>", lab?.Name ?? "Неизвестная лабораторная", task?.Title ?? "Неизвестная задача");
            _html.AppendFormat("<pre class='code-block'>{0}</pre></div>", formattedSql);
        }
        return this;
    }

    public string Build()
    {
        _html.Append("</html>");
        return _html.ToString();
    }
}
