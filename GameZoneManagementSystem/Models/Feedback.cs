public class Feedback
{
    public int ID { get; set; }
    public int UserID { get; set; }
    public int GameID { get; set; }
    public string Message { get; set; }
    public DateTime SubmittedAt { get; set; }

    public string GameName { get; set; } // optional for view
}
