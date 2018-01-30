namespace SiteBlocker.Models
{
    public class ValidateResult
    {
        public ValidateResult(bool isOk, string message, int intTime)
        {
            IsOK = isOk;
            Message = message;
            IntTime = intTime;
        }

        public bool IsOK { get; }
        public string Message { get; }
        public int IntTime { get; }
    }
}
