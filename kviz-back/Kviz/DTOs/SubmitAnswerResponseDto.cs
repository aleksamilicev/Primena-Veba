namespace Kviz.DTOs
{
    // DTO za odgovor na slanje odgovora
    public class SubmitAnswerResponseDto
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public int AttemptId { get; set; }
        public bool IsCorrect { get; set; }
        public string Message { get; set; }
    }
}
