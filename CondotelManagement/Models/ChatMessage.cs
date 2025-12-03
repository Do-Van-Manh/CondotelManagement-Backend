using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CondotelManagement.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }   
        public string? Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public ChatConversation Conversation { get; set; } = null!;
    }
}
