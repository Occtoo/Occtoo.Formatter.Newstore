namespace Occtoo.Formatter.Newstore.Models
{
    public interface INewstoreOutboxDto
    {
        string FileName { get; set; }

        string Locale { get; set; }
    }

    public class NewstoreOutboxDto : INewstoreOutboxDto
    {
        public string FileName { get; set; }

        public string Locale { get; set; }
    }
}
