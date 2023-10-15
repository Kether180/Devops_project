using System;
namespace Minitwit.Models
{
	public class Message
	{
		public int MessageId { get; set; } = 0;
		public int AuthorId { get; set; }
		public string text { get; set; } = string.Empty;
		public DateTime PubDate { get; set; }
		public int Flagged { get; set; }
	}
}

