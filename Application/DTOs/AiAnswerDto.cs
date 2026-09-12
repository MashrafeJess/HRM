namespace Application.DTOs;

public class AiAnswerDto
{
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public List<string> ToolsUsed { get; set; } = [];
}
